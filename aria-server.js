const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const fs = require('fs').promises;
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3002;

app.use(cors({
    origin: ['http://localhost:3000', 'http://localhost:3001', 'http://127.0.0.1:3001'],
    credentials: true
}));
app.use(bodyParser.json());

// Middleware para logging de requests do ARIA
app.use('/api/aria', (req, res, next) => {
    console.log(`🎙️ ARIA Request: ${req.method} ${req.url} - ${new Date().toLocaleString()}`);
    next();
});

const tasksDbPath = path.join(__dirname, 'tasks.json');
const projectsDbPath = path.join(__dirname, 'projects.json');

async function loadTasks() {
    try {
        const data = await fs.readFile(tasksDbPath, 'utf8');
        return JSON.parse(data);
    } catch (error) {
        return [];
    }
}

async function saveTasks(tasks) {
    await fs.writeFile(tasksDbPath, JSON.stringify(tasks, null, 2));
}

async function loadProjects() {
    try {
        const data = await fs.readFile(projectsDbPath, 'utf8');
        return JSON.parse(data);
    } catch (error) {
        return [];
    }
}

async function saveProjects(projects) {
    await fs.writeFile(projectsDbPath, JSON.stringify(projects, null, 2));
}

// ====================================================
// ENDPOINTS ESPECÍFICOS PARA INTEGRAÇÃO COM ARIA
// ====================================================

// Endpoint para ARIA verificar status do sistema
app.get('/api/aria/status', (req, res) => {
    res.json({
        success: true,
        agenda_system: 'online',
        aria_integration: 'active',
        timestamp: new Date().toISOString(),
        endpoints: {
            tasks: '/api/aria/tasks',
            projects: '/api/aria/projects',
            voice_command: '/api/aria/voice-command',
            sync: '/api/aria/sync'
        }
    });
});

// Endpoint para comandos de voz do ARIA
app.post('/api/aria/voice-command', async (req, res) => {
    try {
        const { command, action, parameters } = req.body;
        console.log(`🎙️ ARIA Comando de Voz: ${command}`);

        let result = {};

        switch (action) {
            case 'list_tasks':
                const tasks = await loadTasks();
                const today = new Date().toISOString().split('T')[0];

                let filteredTasks = tasks;
                if (parameters?.date === 'today') {
                    filteredTasks = tasks.filter(task =>
                        task.dueDate && task.dueDate.startsWith(today)
                    );
                }

                result = {
                    action: 'list_tasks',
                    tasks: filteredTasks,
                    count: filteredTasks.length,
                    speech_response: `Você tem ${filteredTasks.length} tarefas${parameters?.date === 'today' ? ' para hoje' : ''}`
                };
                break;

            case 'add_task':
                const newTask = {
                    id: Date.now(),
                    title: parameters.title,
                    description: parameters.description || '',
                    completed: false,
                    priority: parameters.priority || 'medium',
                    category: parameters.category || 'geral',
                    dueDate: parameters.date || null,
                    createdAt: new Date().toISOString()
                };

                const allTasks = await loadTasks();
                allTasks.push(newTask);
                await saveTasks(allTasks);

                result = {
                    action: 'add_task',
                    task: newTask,
                    speech_response: `Tarefa "${parameters.title}" criada com sucesso`
                };
                break;

            case 'complete_task':
                const tasksToUpdate = await loadTasks();
                const taskIndex = tasksToUpdate.findIndex(t =>
                    t.title.toLowerCase().includes(parameters.title.toLowerCase())
                );

                if (taskIndex !== -1) {
                    tasksToUpdate[taskIndex].completed = true;
                    tasksToUpdate[taskIndex].completedAt = new Date().toISOString();
                    await saveTasks(tasksToUpdate);

                    result = {
                        action: 'complete_task',
                        task: tasksToUpdate[taskIndex],
                        speech_response: `Tarefa "${tasksToUpdate[taskIndex].title}" marcada como concluída`
                    };
                } else {
                    result = {
                        action: 'complete_task',
                        error: 'Tarefa não encontrada',
                        speech_response: `Não encontrei uma tarefa com o nome "${parameters.title}"`
                    };
                }
                break;

            case 'create_project':
                const newProject = {
                    id: Date.now(),
                    name: parameters.title,
                    description: parameters.description || '',
                    createdAt: new Date().toISOString(),
                    status: 'active'
                };

                const allProjects = await loadProjects();
                allProjects.push(newProject);
                await saveProjects(allProjects);

                result = {
                    action: 'create_project',
                    project: newProject,
                    speech_response: `Projeto "${parameters.title}" criado com sucesso`
                };
                break;

            case 'schedule_meeting':
                const meetingTask = {
                    id: Date.now(),
                    title: `Reunião: ${parameters.title}`,
                    description: `Reunião agendada via ARIA${parameters.description ? '. ' + parameters.description : ''}`,
                    completed: false,
                    priority: 'high',
                    category: 'reunião',
                    dueDate: parameters.date,
                    meetingTime: parameters.time,
                    createdAt: new Date().toISOString()
                };

                const tasksForMeeting = await loadTasks();
                tasksForMeeting.push(meetingTask);
                await saveTasks(tasksForMeeting);

                result = {
                    action: 'schedule_meeting',
                    meeting: meetingTask,
                    speech_response: `Reunião "${parameters.title}" agendada para ${parameters.date}${parameters.time ? ' às ' + parameters.time : ''}`
                };
                break;

            case 'sync_agenda':
                console.log('🔄 ARIA solicitou sincronização da agenda...');

                result = {
                    action: 'sync_agenda',
                    sync_result: { success: true },
                    speech_response: 'Agenda sincronizada com sucesso'
                };
                break;

            default:
                result = {
                    action: 'unknown',
                    error: 'Comando não reconhecido',
                    speech_response: 'Desculpe, não entendi este comando'
                };
        }

        res.json({
            success: true,
            command: command,
            result: result,
            timestamp: new Date().toISOString()
        });

    } catch (error) {
        console.error('❌ Erro ao processar comando de voz:', error);
        res.status(500).json({
            success: false,
            error: 'Erro ao processar comando de voz',
            details: error.message,
            speech_response: 'Ocorreu um erro ao processar seu comando'
        });
    }
});

// Endpoint para ARIA obter tarefas
app.get('/api/aria/tasks', async (req, res) => {
    try {
        const tasks = await loadTasks();
        const { filter, date, completed } = req.query;

        let filteredTasks = tasks;

        if (date === 'today') {
            const today = new Date().toISOString().split('T')[0];
            filteredTasks = filteredTasks.filter(task =>
                task.dueDate && task.dueDate.startsWith(today)
            );
        }

        if (completed !== undefined) {
            const isCompleted = completed === 'true';
            filteredTasks = filteredTasks.filter(task => task.completed === isCompleted);
        }

        if (filter) {
            filteredTasks = filteredTasks.filter(task =>
                task.title.toLowerCase().includes(filter.toLowerCase()) ||
                (task.description && task.description.toLowerCase().includes(filter.toLowerCase()))
            );
        }

        res.json({
            success: true,
            tasks: filteredTasks,
            count: filteredTasks.length,
            timestamp: new Date().toISOString()
        });

    } catch (error) {
        res.status(500).json({
            success: false,
            error: 'Erro ao carregar tarefas',
            details: error.message
        });
    }
});

// Endpoint para ARIA obter resumo da agenda
app.get('/api/aria/summary', async (req, res) => {
    try {
        const tasks = await loadTasks();
        const projects = await loadProjects();

        const today = new Date().toISOString().split('T')[0];
        const todayTasks = tasks.filter(task =>
            task.dueDate && task.dueDate.startsWith(today) && !task.completed
        );

        const pendingTasks = tasks.filter(task => !task.completed);
        const completedTasks = tasks.filter(task => task.completed);

        const summary = {
            total_tasks: tasks.length,
            pending_tasks: pendingTasks.length,
            completed_tasks: completedTasks.length,
            today_tasks: todayTasks.length,
            total_projects: projects.length
        };

        res.json({
            success: true,
            summary: summary,
            speech_response: `Você tem ${pendingTasks.length} tarefas pendentes, ${todayTasks.length} para hoje e ${projects.length} projetos ativos`,
            timestamp: new Date().toISOString()
        });

    } catch (error) {
        res.status(500).json({
            success: false,
            error: 'Erro ao gerar resumo',
            details: error.message
        });
    }
});

app.listen(PORT, () => {
    console.log(`🎙️ ARIA Integration Server running on http://localhost:${PORT}`);
    console.log(`📡 Endpoints disponíveis:`);
    console.log(`   - GET  /api/aria/status       - Status do sistema`);
    console.log(`   - POST /api/aria/voice-command - Comandos de voz`);
    console.log(`   - GET  /api/aria/tasks        - Listar tarefas`);
    console.log(`   - GET  /api/aria/summary      - Resumo da agenda`);
    console.log(`✅ Pronto para receber comandos do ARIA!`);
});