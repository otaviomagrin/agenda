const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const fs = require('fs').promises;
const path = require('path');
const SyncManager = require('./sync-manager');

const app = express();
const PORT = process.env.PORT || 3001;

// Inicializar gerenciador de sincronização
const syncManager = new SyncManager();

app.use(cors({
    origin: ['http://localhost:3000', 'http://localhost:3001', 'http://127.0.0.1:3001'],
    credentials: true
}));
app.use(bodyParser.json());
app.use(express.static('.'));

// Middleware para logging de requests do ARIA
app.use('/api/aria', (req, res, next) => {
    console.log(`🎙️ ARIA Request: ${req.method} ${req.url} - ${new Date().toLocaleString()}`);
    next();
});

const tasksDbPath = path.join(__dirname, 'tasks.json');
const projectsDbPath = path.join(__dirname, 'projects.json');
const projectTasksDbPath = path.join(__dirname, 'project-tasks.json');
const recurringTasksDbPath = path.join(__dirname, 'recurring-tasks.json');
const travelsDbPath = path.join(__dirname, 'travels.json');

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

async function loadProjectTasks() {
    try {
        const data = await fs.readFile(projectTasksDbPath, 'utf8');
        return JSON.parse(data);
    } catch (error) {
        return [];
    }
}

async function saveProjectTasks(projectTasks) {
    await fs.writeFile(projectTasksDbPath, JSON.stringify(projectTasks, null, 2));
}

async function loadRecurringTasks() {
    try {
        const data = await fs.readFile(recurringTasksDbPath, 'utf8');
        return JSON.parse(data);
    } catch (error) {
        return [];
    }
}

async function saveRecurringTasks(recurringTasks) {
    await fs.writeFile(recurringTasksDbPath, JSON.stringify(recurringTasks, null, 2));
}

async function loadTravels() {
    try {
        const data = await fs.readFile(travelsDbPath, 'utf8');
        return JSON.parse(data);
    } catch (error) {
        return [];
    }
}

async function saveTravels(travels) {
    await fs.writeFile(travelsDbPath, JSON.stringify(travels, null, 2));
}

// Engine de Recorrência
function calculateNextOccurrence(schedule, fromDate = new Date()) {
    const { type, interval = 1, days_of_week, day_of_month, skip_dates = [] } = schedule;
    let nextDate = new Date(fromDate);
    
    // Função para verificar se data deve ser pulada
    const shouldSkip = (date) => {
        const dateStr = date.toISOString().substring(0, 10);
        return skip_dates.includes(dateStr);
    };
    
    switch (type) {
        case 'daily':
            do {
                nextDate.setDate(nextDate.getDate() + interval);
            } while (shouldSkip(nextDate));
            break;
            
        case 'weekly':
            if (days_of_week && days_of_week.length > 0) {
                do {
                    nextDate.setDate(nextDate.getDate() + 1);
                } while (!days_of_week.includes(nextDate.getDay()) || shouldSkip(nextDate));
            } else {
                do {
                    nextDate.setDate(nextDate.getDate() + (7 * interval));
                } while (shouldSkip(nextDate));
            }
            break;
            
        case 'monthly':
            if (day_of_month) {
                do {
                    nextDate.setMonth(nextDate.getMonth() + interval);
                    nextDate.setDate(day_of_month);
                } while (shouldSkip(nextDate));
            } else {
                do {
                    nextDate.setMonth(nextDate.getMonth() + interval);
                } while (shouldSkip(nextDate));
            }
            break;
            
        default:
            nextDate.setDate(nextDate.getDate() + 1);
    }
    
    return nextDate.toISOString();
}

// Processa tarefas recorrentes e gera novas instâncias
async function processRecurringTasks() {
    const recurringTasks = await loadRecurringTasks();
    const tasks = await loadTasks();
    const now = new Date();
    const generationWindow = 7; // dias
    let hasChanges = false;
    
    for (const recurring of recurringTasks) {
        if (!recurring.active || !recurring.next_due) continue;
        
        const nextDue = new Date(recurring.next_due);
        const daysDiff = Math.ceil((nextDue - now) / (1000 * 60 * 60 * 24));
        
        // Gerar tarefa se está dentro da janela de geração
        if (daysDiff <= generationWindow && daysDiff >= 0) {
            const newTask = {
                ...recurring.template,
                id: Date.now() + Math.random(),
                date: nextDue.toISOString().substring(0, 10),
                datetime: recurring.next_due,
                recurring_parent_id: recurring.id,
                created: now.toISOString()
            };
            
            // Verificar se tarefa já existe para esta data
            const exists = tasks.some(t => 
                t.recurring_parent_id === recurring.id && 
                t.date === newTask.date
            );
            
            if (!exists) {
                tasks.push(newTask);
                recurring.last_created = newTask.datetime;
                recurring.next_due = calculateNextOccurrence(recurring.schedule, nextDue);
                hasChanges = true;
            }
        }
    }
    
    if (hasChanges) {
        await saveTasks(tasks);
        await saveRecurringTasks(recurringTasks);
    }
}

// Processar recorrências a cada 5 minutos
setInterval(processRecurringTasks, 5 * 60 * 1000);

// Processar na inicialização
processRecurringTasks();

app.get('/api/tasks', async (req, res) => {
    const tasks = await loadTasks();
    res.json(tasks);
});

app.post('/api/tasks', async (req, res) => {
    const tasks = await loadTasks();
    const newTask = { ...req.body, id: Date.now() };
    tasks.push(newTask);
    await saveTasks(tasks);
    res.json(newTask);
});

app.put('/api/tasks/:id', async (req, res) => {
    const tasks = await loadTasks();
    const index = tasks.findIndex(t => t.id === parseInt(req.params.id));
    if (index !== -1) {
        tasks[index] = { ...tasks[index], ...req.body };
        await saveTasks(tasks);
        res.json(tasks[index]);
    } else {
        res.status(404).json({ error: 'Task not found' });
    }
});

app.delete('/api/tasks/:id', async (req, res) => {
    let tasks = await loadTasks();
    tasks = tasks.filter(t => t.id !== parseInt(req.params.id));
    await saveTasks(tasks);
    res.json({ success: true });
});

// Rotas para projetos
app.get('/api/projects', async (req, res) => {
    const projects = await loadProjects();
    res.json(projects);
});

app.post('/api/projects', async (req, res) => {
    const projects = await loadProjects();
    const newProject = { ...req.body, id: Date.now() };
    projects.push(newProject);
    await saveProjects(projects);
    res.json(newProject);
});

app.put('/api/projects/:id', async (req, res) => {
    const projects = await loadProjects();
    const index = projects.findIndex(p => p.id === parseInt(req.params.id));
    if (index !== -1) {
        projects[index] = { ...projects[index], ...req.body };
        await saveProjects(projects);
        res.json(projects[index]);
    } else {
        res.status(404).json({ error: 'Project not found' });
    }
});

app.delete('/api/projects/:id', async (req, res) => {
    let projects = await loadProjects();
    projects = projects.filter(p => p.id !== parseInt(req.params.id));
    await saveProjects(projects);
    res.json({ success: true });
});

// Rotas para tarefas de projeto
app.get('/api/project-tasks', async (req, res) => {
    const projectTasks = await loadProjectTasks();
    res.json(projectTasks);
});

app.post('/api/project-tasks', async (req, res) => {
    const projectTasks = req.body;
    await saveProjectTasks(projectTasks);
    res.json({ success: true });
});

// Endpoint para sincronização de tempo
app.get('/api/time', (req, res) => {
    const now = new Date();
    res.json({
        server_utc: now.toISOString(),
        server_local: now.toLocaleString('pt-BR'),
        server_epoch: Math.floor(now.getTime() / 1000),
        server_tz: Intl.DateTimeFormat().resolvedOptions().timeZone,
        server_start_epoch: Math.floor(Date.now() / 1000)
    });
});

// Rotas para tarefas recorrentes
app.get('/api/recurring', async (req, res) => {
    const recurringTasks = await loadRecurringTasks();
    const series = recurringTasks.map(r => ({
        id: r.id,
        title: r.template?.title,
        next_due: r.next_due,
        last_created: r.last_created,
        active: r.active !== false,
        schedule: r.schedule
    }));
    res.json({ series, count: series.length });
});

app.post('/api/recurring', async (req, res) => {
    const recurringTasks = await loadRecurringTasks();
    
    // Calcular primeira data se não fornecida
    let nextDue = req.body.next_due;
    if (!nextDue && req.body.schedule) {
        const firstOccurrence = calculateNextOccurrence(req.body.schedule, new Date());
        nextDue = firstOccurrence;
    }
    
    const newRecurring = {
        id: `recurring_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
        template: req.body.template,
        schedule: req.body.schedule,
        next_due: nextDue,
        last_created: null,
        active: true,
        created_at: new Date().toISOString()
    };
    
    recurringTasks.push(newRecurring);
    await saveRecurringTasks(recurringTasks);
    res.json(newRecurring);
});

app.post('/api/recurring/:id/toggle', async (req, res) => {
    const recurringTasks = await loadRecurringTasks();
    const recurring = recurringTasks.find(r => r.id === req.params.id);
    if (recurring) {
        recurring.active = !recurring.active;
        await saveRecurringTasks(recurringTasks);
        res.json({ status: 'ok', active: recurring.active });
    } else {
        res.status(404).json({ error: 'Série não encontrada' });
    }
});

app.post('/api/recurring/:id/skip', async (req, res) => {
    const recurringTasks = await loadRecurringTasks();
    const recurring = recurringTasks.find(r => r.id === req.params.id);
    if (recurring && recurring.next_due) {
        // Adiciona data atual às datas puladas
        const schedule = recurring.schedule || {};
        const skipDates = schedule.skip_dates || [];
        const currentNext = recurring.next_due.substring(0, 10);
        skipDates.push(currentNext);
        schedule.skip_dates = skipDates;
        recurring.schedule = schedule;
        
        // Usa o engine para calcular próxima ocorrência
        const nextDueDate = new Date(recurring.next_due);
        recurring.next_due = calculateNextOccurrence(schedule, nextDueDate);
        
        await saveRecurringTasks(recurringTasks);
        res.json({ status: 'ok', skipped: currentNext, new_next: recurring.next_due });
    } else {
        res.status(404).json({ error: 'Série não encontrada' });
    }
});

// Rotas para viagens
app.get('/api/travels', async (req, res) => {
    const travels = await loadTravels();
    res.json(travels);
});

app.post('/api/travels', async (req, res) => {
    const travels = req.body;
    await saveTravels(travels);
    res.json({ success: true });
});

// Rotas de sincronização
app.get('/api/sync/status', (req, res) => {
    const status = syncManager.getStatus();
    res.json(status);
});

app.post('/api/sync/force', async (req, res) => {
    try {
        // Inicializar pastas se necessário
        const initResults = syncManager.initializeSyncFolders();
        console.log('Pastas inicializadas:', initResults);

        // Executar sincronização
        const results = await syncManager.performSync();
        console.log('Resultados da sincronização:', results);

        res.json({
            success: true,
            results,
            message: 'Sincronização executada com sucesso'
        });
    } catch (error) {
        console.error('Erro na sincronização forçada:', error);
        res.status(500).json({
            error: 'Erro ao sincronizar',
            details: error.message
        });
    }
});

app.post('/api/sync/resolve-conflicts', async (req, res) => {
    try {
        console.log('Iniciando resolução de conflitos...');
        const resolved = syncManager.resolveConflicts();

        if (resolved) {
            console.log('Conflitos resolvidos com sucesso');
            res.json({
                success: true,
                message: 'Conflitos resolvidos com sucesso'
            });
        } else {
            console.log('Nenhum conflito foi encontrado ou resolvido');
            res.json({
                success: true,
                message: 'Nenhum conflito encontrado para resolver'
            });
        }
    } catch (error) {
        console.error('Erro ao resolver conflitos:', error);
        res.status(500).json({
            error: 'Erro ao resolver conflitos',
            details: error.message
        });
    }
});

app.post('/api/sync/toggle', async (req, res) => {
    const { enabled } = req.body;

    if (enabled) {
        syncManager.startAutoSync(5); // Sincronizar a cada 5 minutos
        res.json({ success: true, message: 'Sincronização automática ativada' });
    } else {
        syncManager.stopAutoSync();
        res.json({ success: true, message: 'Sincronização automática desativada' });
    }
});

// ====================================================
// ENDPOINTS ESPECÍFICOS PARA INTEGRAÇÃO COM ARIA
// ====================================================

// Endpoint para ARIA verificar status do sistema
app.get('/api/aria/status', (req, res) => {
    const status = syncManager.getStatus();
    res.json({
        success: true,
        agenda_system: 'online',
        sync_status: status,
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
                const syncResult = await syncManager.performSync();

                result = {
                    action: 'sync_agenda',
                    sync_result: syncResult,
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

// Endpoint para ARIA obter projetos
app.get('/api/aria/projects', async (req, res) => {
    try {
        const projects = await loadProjects();

        res.json({
            success: true,
            projects: projects,
            count: projects.length,
            timestamp: new Date().toISOString()
        });

    } catch (error) {
        res.status(500).json({
            success: false,
            error: 'Erro ao carregar projetos',
            details: error.message
        });
    }
});

// Endpoint para ARIA forçar sincronização
app.post('/api/aria/sync', async (req, res) => {
    try {
        console.log('🔄 ARIA solicitou sincronização forçada...');
        const result = await syncManager.performSync();

        res.json({
            success: true,
            message: 'Sincronização concluída',
            result: result,
            timestamp: new Date().toISOString()
        });

    } catch (error) {
        console.error('❌ Erro na sincronização solicitada pelo ARIA:', error);
        res.status(500).json({
            success: false,
            error: 'Erro ao sincronizar',
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
            total_projects: projects.length,
            sync_status: syncManager.getStatus()
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
    console.log(`Server running on http://localhost:${PORT}`);
    console.log(`🎙️ ARIA Integration endpoints available at /api/aria/*`);

    // Inicializar sincronização ao iniciar o servidor
    const status = syncManager.getStatus();
    if (status.hasOneDrive || status.hasGoogleDrive) {
        console.log('Serviços de nuvem detectados:');
        if (status.hasOneDrive) console.log('✓ OneDrive');
        if (status.hasGoogleDrive) console.log('✓ Google Drive');

        // Inicializar pastas de sincronização
        syncManager.initializeSyncFolders();

        // Iniciar sincronização automática
        syncManager.startAutoSync(5);
        console.log('Sincronização automática iniciada (a cada 5 minutos)');
    } else {
        console.log('Nenhum serviço de nuvem detectado. Sincronização desativada.');
    }
});