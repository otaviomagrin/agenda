const http = require('http');
const fs = require('fs').promises;
const path = require('path');
const url = require('url');

const PORT = 3002;

// Paths to data files
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

function parseJSON(body) {
    try {
        return JSON.parse(body);
    } catch (error) {
        return null;
    }
}

const server = http.createServer(async (req, res) => {
    // CORS headers
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

    if (req.method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
    }

    const urlParts = url.parse(req.url, true);
    const pathname = urlParts.pathname;

    console.log(`🎙️ ARIA Request: ${req.method} ${pathname} - ${new Date().toLocaleString()}`);

    res.setHeader('Content-Type', 'application/json');

    try {
        if (pathname === '/api/aria/status') {
            res.writeHead(200);
            res.end(JSON.stringify({
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
            }));
            return;
        }

        if (pathname === '/api/aria/tasks') {
            const tasks = await loadTasks();
            const { filter, date, completed } = urlParts.query;

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

            res.writeHead(200);
            res.end(JSON.stringify({
                success: true,
                tasks: filteredTasks,
                count: filteredTasks.length,
                timestamp: new Date().toISOString()
            }));
            return;
        }

        if (pathname === '/api/aria/summary') {
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

            res.writeHead(200);
            res.end(JSON.stringify({
                success: true,
                summary: summary,
                speech_response: `Você tem ${pendingTasks.length} tarefas pendentes, ${todayTasks.length} para hoje e ${projects.length} projetos ativos`,
                timestamp: new Date().toISOString()
            }));
            return;
        }

        if (pathname === '/api/aria/voice-command' && req.method === 'POST') {
            let body = '';
            req.on('data', chunk => {
                body += chunk.toString();
            });

            req.on('end', async () => {
                const data = parseJSON(body);
                if (!data) {
                    res.writeHead(400);
                    res.end(JSON.stringify({ success: false, error: 'Invalid JSON' }));
                    return;
                }

                const { command, action, parameters } = data;
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

                res.writeHead(200);
                res.end(JSON.stringify({
                    success: true,
                    command: command,
                    result: result,
                    timestamp: new Date().toISOString()
                }));
            });
            return;
        }

        // 404 for unknown endpoints
        res.writeHead(404);
        res.end(JSON.stringify({ success: false, error: 'Endpoint not found' }));

    } catch (error) {
        console.error('❌ Erro no servidor ARIA:', error);
        res.writeHead(500);
        res.end(JSON.stringify({
            success: false,
            error: 'Erro interno do servidor',
            details: error.message
        }));
    }
});

server.listen(PORT, () => {
    console.log(`🎙️ ARIA Integration Server running on http://localhost:${PORT}`);
    console.log(`📡 Endpoints disponíveis:`);
    console.log(`   - GET  /api/aria/status       - Status do sistema`);
    console.log(`   - POST /api/aria/voice-command - Comandos de voz`);
    console.log(`   - GET  /api/aria/tasks        - Listar tarefas`);
    console.log(`   - GET  /api/aria/summary      - Resumo da agenda`);
    console.log(`✅ Pronto para receber comandos do ARIA!`);
});