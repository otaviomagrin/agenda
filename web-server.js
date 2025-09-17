const http = require('http');
const fs = require('fs').promises;
const path = require('path');
const url = require('url');

const PORT = 3001;

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

function getMimeType(filePath) {
    const ext = path.extname(filePath).toLowerCase();
    const mimeTypes = {
        '.html': 'text/html',
        '.css': 'text/css',
        '.js': 'application/javascript',
        '.json': 'application/json',
        '.png': 'image/png',
        '.jpg': 'image/jpeg',
        '.jpeg': 'image/jpeg',
        '.gif': 'image/gif',
        '.svg': 'image/svg+xml',
        '.ico': 'image/x-icon',
        '.woff': 'font/woff',
        '.woff2': 'font/woff2',
        '.ttf': 'font/ttf',
        '.eot': 'application/vnd.ms-fontobject'
    };
    return mimeTypes[ext] || 'text/plain';
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

    console.log(`📊 Request: ${req.method} ${pathname} - ${new Date().toLocaleString()}`);

    try {
        // Serve static files
        if (pathname === '/' || pathname === '/index.html') {
            const htmlPath = path.join(__dirname, 'index.html');
            try {
                const content = await fs.readFile(htmlPath, 'utf8');
                res.setHeader('Content-Type', 'text/html');
                res.writeHead(200);
                res.end(content);
                return;
            } catch (error) {
                console.error('Error serving index.html:', error);
            }
        }

        // Serve other static files
        if (pathname.startsWith('/')) {
            const filePath = path.join(__dirname, pathname);
            try {
                const stats = await fs.stat(filePath);
                if (stats.isFile()) {
                    const content = await fs.readFile(filePath);
                    res.setHeader('Content-Type', getMimeType(filePath));
                    res.writeHead(200);
                    res.end(content);
                    return;
                }
            } catch (error) {
                // File not found, continue to API routes
            }
        }

        // API Routes
        res.setHeader('Content-Type', 'application/json');

        // Tasks API
        if (pathname === '/api/tasks') {
            if (req.method === 'GET') {
                const tasks = await loadTasks();
                res.writeHead(200);
                res.end(JSON.stringify(tasks));
                return;
            }

            if (req.method === 'POST') {
                let body = '';
                req.on('data', chunk => {
                    body += chunk.toString();
                });

                req.on('end', async () => {
                    const newTask = parseJSON(body);
                    if (!newTask) {
                        res.writeHead(400);
                        res.end(JSON.stringify({ error: 'Invalid JSON' }));
                        return;
                    }

                    newTask.id = newTask.id || Date.now();
                    newTask.createdAt = new Date().toISOString();
                    newTask.completed = false;

                    const tasks = await loadTasks();
                    tasks.push(newTask);
                    await saveTasks(tasks);

                    res.writeHead(201);
                    res.end(JSON.stringify(newTask));
                });
                return;
            }
        }

        // Update task
        if (pathname.startsWith('/api/tasks/') && req.method === 'PUT') {
            const taskId = parseInt(pathname.split('/')[3]);
            let body = '';
            req.on('data', chunk => {
                body += chunk.toString();
            });

            req.on('end', async () => {
                const updates = parseJSON(body);
                if (!updates) {
                    res.writeHead(400);
                    res.end(JSON.stringify({ error: 'Invalid JSON' }));
                    return;
                }

                const tasks = await loadTasks();
                const taskIndex = tasks.findIndex(t => t.id === taskId);

                if (taskIndex === -1) {
                    res.writeHead(404);
                    res.end(JSON.stringify({ error: 'Task not found' }));
                    return;
                }

                tasks[taskIndex] = { ...tasks[taskIndex], ...updates };
                if (updates.completed && !tasks[taskIndex].completedAt) {
                    tasks[taskIndex].completedAt = new Date().toISOString();
                }

                await saveTasks(tasks);
                res.writeHead(200);
                res.end(JSON.stringify(tasks[taskIndex]));
            });
            return;
        }

        // Delete task
        if (pathname.startsWith('/api/tasks/') && req.method === 'DELETE') {
            const taskId = parseInt(pathname.split('/')[3]);
            const tasks = await loadTasks();
            const filteredTasks = tasks.filter(t => t.id !== taskId);

            if (filteredTasks.length === tasks.length) {
                res.writeHead(404);
                res.end(JSON.stringify({ error: 'Task not found' }));
                return;
            }

            await saveTasks(filteredTasks);
            res.writeHead(200);
            res.end(JSON.stringify({ success: true }));
            return;
        }

        // Projects API
        if (pathname === '/api/projects') {
            if (req.method === 'GET') {
                const projects = await loadProjects();
                res.writeHead(200);
                res.end(JSON.stringify(projects));
                return;
            }

            if (req.method === 'POST') {
                let body = '';
                req.on('data', chunk => {
                    body += chunk.toString();
                });

                req.on('end', async () => {
                    const newProject = parseJSON(body);
                    if (!newProject) {
                        res.writeHead(400);
                        res.end(JSON.stringify({ error: 'Invalid JSON' }));
                        return;
                    }

                    newProject.id = newProject.id || Date.now();
                    newProject.createdAt = new Date().toISOString();

                    const projects = await loadProjects();
                    projects.push(newProject);
                    await saveProjects(projects);

                    res.writeHead(201);
                    res.end(JSON.stringify(newProject));
                });
                return;
            }
        }

        // ARIA Integration endpoints
        if (pathname === '/api/aria/status') {
            res.writeHead(200);
            res.end(JSON.stringify({
                success: true,
                agenda_system: 'online',
                aria_integration: 'active',
                timestamp: new Date().toISOString(),
                server_type: 'web_fallback',
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

        // Voice command endpoint (redirect to ARIA server)
        if (pathname === '/api/aria/voice-command' && req.method === 'POST') {
            res.writeHead(200);
            res.end(JSON.stringify({
                success: true,
                message: 'Voice commands are handled by ARIA server on port 3002',
                redirect: 'http://localhost:3002/api/aria/voice-command'
            }));
            return;
        }

        // 404 for unknown routes
        res.writeHead(404);
        res.end(JSON.stringify({
            error: 'Not found',
            available_routes: [
                'GET /',
                'GET /api/tasks',
                'POST /api/tasks',
                'PUT /api/tasks/:id',
                'DELETE /api/tasks/:id',
                'GET /api/projects',
                'POST /api/projects',
                'GET /api/aria/status',
                'GET /api/aria/tasks',
                'GET /api/aria/summary'
            ]
        }));

    } catch (error) {
        console.error('❌ Server error:', error);
        res.writeHead(500);
        res.end(JSON.stringify({
            error: 'Internal server error',
            details: error.message
        }));
    }
});

server.listen(PORT, () => {
    console.log(`📊 Agenda Web Server running on http://localhost:${PORT}`);
    console.log(`🌐 Access the agenda at: http://localhost:${PORT}`);
    console.log(`📡 API endpoints available:`);
    console.log(`   - GET  /api/tasks             - List all tasks`);
    console.log(`   - POST /api/tasks             - Create new task`);
    console.log(`   - PUT  /api/tasks/:id         - Update task`);
    console.log(`   - DELETE /api/tasks/:id       - Delete task`);
    console.log(`   - GET  /api/projects          - List all projects`);
    console.log(`   - POST /api/projects          - Create new project`);
    console.log(`   - GET  /api/aria/status       - ARIA status`);
    console.log(`   - GET  /api/aria/tasks        - ARIA task list`);
    console.log(`   - GET  /api/aria/summary      - ARIA summary`);
    console.log(`✅ Server ready! No Electron dependencies required.`);
});