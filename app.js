console.log('app.js is loading...');
const API_URL = window.location.hostname === 'localhost' ? 'http://localhost:3001/api' : '/api';
console.log('API_URL set to:', API_URL);

let tasks = [];
let projects = [];
let projectTasks = [];
let recurringTasks = [];
let currentFilter = 'all';
let currentDate = new Date();
let calendarDate = new Date();
let currentProjectId = null;
let serverTimeOffset = 0; // diferença entre servidor e cliente em ms

// Navegação entre views
function showView(viewName) {
    document.querySelectorAll('.view-container').forEach(container => {
        container.classList.remove('active');
    });
    
    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    
    document.getElementById(`${viewName}-view`).classList.add('active');
    document.querySelector(`[data-view="${viewName}"]`).classList.add('active');
    
    if (viewName === 'dashboard') {
        // Carregar Dashboard de 60 dias
        loadDashboard60Days();
    }
}

// Event listeners para navegação
document.addEventListener('DOMContentLoaded', async function() {
    console.log('DOM Content Loaded - Starting initialization');
    
    try {
        document.querySelectorAll('.nav-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                showView(btn.dataset.view);
            });
        });
        
        // Inicializar arrays vazios se não existirem
        if (!tasks) tasks = [];
        if (!projects) projects = [];
        if (!projectTasks) projectTasks = [];
        
        console.log('Starting initial loads...');
        
        // Atualizar data/hora imediatamente (com hora local)
        updateDateTime();
        
        // Sincronizar tempo com servidor
        await syncServerTime();
        
        // Inicialização
        loadTasks().catch(err => console.log('Load tasks failed:', err));
        loadProjects().catch(err => console.log('Load projects failed:', err));
        loadRecurringTasks().catch(err => console.log('Load recurring failed:', err));
        
        // Iniciar atualização da data/hora em tempo real
        setInterval(updateDateTime, 1000);
        
        console.log('Initializing components...');
        initializeCalendarNavigation();
        initializeForms();
        initializeProjectForms();
        
        // Renderizar calendário imediatamente
        console.log('Rendering calendar...');
        renderCalendar();
        
        // Carregar project tasks em background
        loadProjectTasks().then(() => {
            console.log('Project tasks loaded successfully');
            renderCalendar(); // Re-renderizar com project tasks
        }).catch(error => {
            console.error('Error loading project tasks:', error);
            projectTasks = []; // Garantir que está inicializado
        });
        
        console.log('Initialization completed');
    } catch (error) {
        console.error('Error during initialization:', error);
    }

    // Inicializar assistente de voz
    try { setupVoiceAssistant(); } catch (e) { console.warn('Voice assistant init skipped:', e); }
});

// Calendário acessível para dislexia
function renderCalendar() {
    console.log('renderCalendar() called');
    const calendar = document.getElementById('calendar');
    
    if (!calendar) {
        console.error('Calendar element not found!');
        return;
    }
    
    console.log('Calendar element found:', calendar);
    const monthNames = [
        'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
        'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
    ];
    
    const dayNames = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
    
    document.getElementById('currentMonth').textContent = 
        `${monthNames[calendarDate.getMonth()]} ${calendarDate.getFullYear()}`;
    
    const firstDay = new Date(calendarDate.getFullYear(), calendarDate.getMonth(), 1);
    const lastDay = new Date(calendarDate.getFullYear(), calendarDate.getMonth() + 1, 0);
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - firstDay.getDay());
    
    let calendarHTML = '<div class="calendar-grid">';
    
    // Cabeçalhos dos dias
    dayNames.forEach(day => {
        calendarHTML += `<div class="calendar-header">${day}</div>`;
    });
    
    // Dias do calendário
    for (let i = 0; i < 42; i++) {
        const currentDay = new Date(startDate);
        currentDay.setDate(startDate.getDate() + i);
        
        const isToday = currentDay.toDateString() === getServerTime().toDateString();
        const isCurrentMonth = currentDay.getMonth() === calendarDate.getMonth();
        const dateStr = currentDay.toISOString().split('T')[0];
        
        const hasTasks = tasks.some(task => task.date === dateStr && !task.completed);
        const hasProjects = projects.some(project => {
            const startDate = new Date(project.startDate);
            const endDate = new Date(project.deadline);
            return currentDay >= startDate && currentDay <= endDate && project.progress < 100;
        });
        const hasProjectTasks = projectTasks && projectTasks.some(task => task.date === dateStr && !task.completed);
        
        let classes = 'calendar-day';
        if (isToday) classes += ' today';
        if (!isCurrentMonth) classes += ' other-month';
        if (hasTasks) classes += ' has-tasks';
        if (hasProjects) classes += ' has-projects';
        if (hasProjectTasks) classes += ' has-project-tasks';
        if (hasTasks && hasProjects) classes += ' has-both';
        
        calendarHTML += `
            <div class="${classes}" data-date="${dateStr}">
                ${currentDay.getDate()}
            </div>
        `;
    }
    
    calendarHTML += '</div>';
    calendar.innerHTML = calendarHTML;
    
    // Event listeners para os dias
    document.querySelectorAll('.calendar-day').forEach(day => {
        day.addEventListener('click', () => {
            const date = day.dataset.date;
            showTasksForDate(date);
        });
    });
}

function initializeCalendarNavigation() {
    document.getElementById('prevMonth').addEventListener('click', () => {
        calendarDate.setMonth(calendarDate.getMonth() - 1);
        renderCalendar();
    });
    
    document.getElementById('nextMonth').addEventListener('click', () => {
        calendarDate.setMonth(calendarDate.getMonth() + 1);
        renderCalendar();
    });
}

function showTasksForDate(date) {
    const dateTasks = tasks.filter(task => task.date === date);
    const dateProjectTasks = projectTasks ? projectTasks.filter(task => task.date === date) : [];
    const dateProjects = projects.filter(project => {
        const startDate = new Date(project.startDate);
        const endDate = new Date(project.deadline);
        const selectedDate = new Date(date);
        return selectedDate >= startDate && selectedDate <= endDate && project.progress < 100;
    });

    // Adicionar reuniões (meetings) do dia
    const dateMeetings = meetings ? meetings.filter(meeting => meeting.date === date) : [];

    document.getElementById('modalDate').textContent = `📅 ${formatDate(date)}`;

    const modalTasksList = document.getElementById('modalTasksList');
    const modalProjectsList = document.getElementById('modalProjectsList');
    
    // Combinar tarefas normais e de projeto
    const allTasks = [...dateTasks, ...dateProjectTasks];
    
    if (allTasks.length > 0) {
        modalTasksList.innerHTML = allTasks.map(task => {
            const status = task.completed ? '✅' : (task.isOverdue ? '⚠️' : (task.isProjectTask ? '📁' : '⏳'));
            const completedClass = task.completed ? 'completed' : '';
            const overdueClass = task.isOverdue ? 'overdue' : '';
            const priority = task.priority || 'medium';
            const priorityText = { high: 'Alta', medium: 'Média', low: 'Baixa' }[priority];
            
            // Buscar nome do projeto se for tarefa de projeto
            let projectName = '';
            if (task.isProjectTask) {
                const project = projects.find(p => p.id === task.projectId);
                projectName = project ? project.title : 'Projeto';
            }
            
            return `
                <div class="modal-item ${completedClass} ${overdueClass}">
                    <div class="item-title">${status} ${task.title}${task.isOverdue ? ' (ATRASADA)' : ''}${task.isProjectTask ? ` [${projectName}]` : ''}</div>
                    <div class="item-meta">
                        <span>⏰ ${task.time}</span>
                        <span>🔥 ${priorityText}</span>
                        ${task.category ? `<span>🏷️ ${task.category}</span>` : ''}
                        ${task.isProjectTask ? '<span>📁 Tarefa de Projeto</span>' : '<span>📋 Tarefa Individual</span>'}
                    </div>
                    ${task.description ? `<div style="margin-top: 8px; font-size: 0.9em; opacity: 0.8;">${task.description}</div>` : ''}
                </div>
            `;
        }).join('');
        document.getElementById('modalTasks').style.display = 'block';
    } else {
        document.getElementById('modalTasks').style.display = 'none';
    }
    
    if (dateProjects.length > 0) {
        modalProjectsList.innerHTML = dateProjects.map(project => {
            const deadline = new Date(project.deadline);
            const now = getServerTime();
            const daysUntilDeadline = Math.ceil((deadline - now) / (1000 * 60 * 60 * 24));
            
            let deadlineText = `${daysUntilDeadline} dias restantes`;
            if (daysUntilDeadline < 0) {
                deadlineText = 'Atrasado';
            } else if (daysUntilDeadline === 0) {
                deadlineText = 'Hoje é o deadline';
            } else if (daysUntilDeadline === 1) {
                deadlineText = 'Deadline amanhã';
            }
            
            return `
                <div class="modal-item">
                    <div class="item-title">📁 ${project.title}</div>
                    <div class="item-meta">
                        <span>📅 ${deadlineText}</span>
                        <span>🔥 ${project.priority || 'Média'}</span>
                    </div>
                    <div class="item-progress">
                        <div class="progress-bar-mini">
                            <div class="progress-fill-mini" style="width: ${project.progress}%"></div>
                        </div>
                        <span style="font-size: 0.9em;">${project.progress}%</span>
                    </div>
                    ${project.description ? `<div style="margin-top: 8px; font-size: 0.9em; opacity: 0.8;">${project.description}</div>` : ''}
                </div>
            `;
        }).join('');
        document.getElementById('modalProjects').style.display = 'block';
    } else {
        document.getElementById('modalProjects').style.display = 'none';
    }
    
    if (allTasks.length === 0 && dateProjects.length === 0) {
        modalTasksList.innerHTML = '<div class="empty-day">📅 Nenhuma tarefa ou projeto para este dia.<br><br>Clique em "Nova Tarefa" para adicionar algo!</div>';
        document.getElementById('modalTasks').style.display = 'block';
        document.getElementById('modalProjects').style.display = 'none';
    }
    
    document.getElementById('dayInfoModal').classList.add('active');
}

// Carregamento de dados
async function loadTasks() {
    try {
        const response = await fetch(`${API_URL}/tasks`);
        tasks = await response.json();
        renderTasks();
        renderCalendar();
    } catch (error) {
        console.error('Error loading tasks:', error);
        tasks = [];
        renderTasks();
    }
}

// Sincronização de tempo
async function syncServerTime() {
    try {
        const response = await fetch(`${API_URL}/time`);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const timeData = await response.json();
        const clientTime = Date.now();
        const serverTime = timeData.server_epoch * 1000;
        serverTimeOffset = serverTime - clientTime;
        
        console.log('Tempo sincronizado. Offset:', serverTimeOffset);
    } catch (error) {
        console.warn('Falha na sincronização de tempo:', error.message);
        serverTimeOffset = 0;
    }
    
    // Sempre atualizar exibição, mesmo com falha
    updateDateTime();
}

function getServerTime() {
    return new Date(Date.now() + serverTimeOffset);
}

function updateDateTime() {
    const now = getServerTime();
    const currentDateElement = document.getElementById('current-date');
    const currentTimeElement = document.getElementById('current-time');
    
    if (currentDateElement) {
        const dateText = now.toLocaleDateString('pt-BR', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
        });
        currentDateElement.textContent = dateText;
    }
    
    if (currentTimeElement) {
        const timeText = now.toLocaleTimeString('pt-BR', {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
        currentTimeElement.textContent = timeText;
    }
}

// Tarefas Recorrentes
async function loadRecurringTasks() {
    try {
        const response = await fetch(`${API_URL}/recurring`);
        const data = await response.json();
        recurringTasks = data.series || [];
        renderRecurringTasks();
    } catch (error) {
        console.error('Error loading recurring tasks:', error);
        recurringTasks = [];
    }
}

function renderRecurringTasks() {
    const container = document.getElementById('recurring-tasks-list');
    if (!container) return;
    
    if (recurringTasks.length === 0) {
        container.innerHTML = '<div class="empty-state">Nenhuma série recorrente</div>';
        return;
    }
    
    container.innerHTML = recurringTasks.map(series => `
        <div class="recurring-item ${series.active ? '' : 'inactive'}">
            <div class="recurring-main">
                <span class="recurring-title">${series.title || 'Sem título'}</span>
                <span class="recurring-next">Próxima: ${series.next_due ? formatDateTime(series.next_due) : 'Não definida'}</span>
                ${!series.active ? '<span class="recurring-status">(Pausada)</span>' : ''}
            </div>
            <div class="recurring-actions">
                <button onclick="toggleRecurring('${series.id}')" class="btn btn-sm">
                    ${series.active ? 'Pausar' : 'Ativar'}
                </button>
                <button onclick="skipNext('${series.id}')" class="btn btn-sm" ${!series.next_due ? 'disabled' : ''}>
                    Pular Próxima
                </button>
            </div>
        </div>
    `).join('');
}

async function toggleRecurring(seriesId) {
    try {
        await fetch(`${API_URL}/recurring/${seriesId}/toggle`, { method: 'POST' });
        await loadRecurringTasks();
        console.log('Série alternada com sucesso');
    } catch (error) {
        console.error('Error toggling recurring:', error);
    }
}

async function skipNext(seriesId) {
    try {
        await fetch(`${API_URL}/recurring/${seriesId}/skip`, { method: 'POST' });
        await loadRecurringTasks();
        await loadTasks(); // Recarregar tarefas para atualizar calendario
        console.log('Próxima ocorrência pulada');
    } catch (error) {
        console.error('Error skipping next:', error);
    }
}

function formatDateTime(dateTimeStr) {
    const date = new Date(dateTimeStr);
    return date.toLocaleString('pt-BR', {
        day: '2-digit',
        month: '2-digit', 
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

async function loadProjects() {
    try {
        const response = await fetch(`${API_URL}/projects`);
        projects = await response.json();
        renderProjects();
        renderCalendar();
    } catch (error) {
        console.error('Error loading projects:', error);
        projects = [];
        renderProjects();
    }
}

// Renderização de tarefas
function renderTasks() {
    const tasksList = document.getElementById('tasksList');
    const filteredTasks = filterTasks();
    
    if (filteredTasks.length === 0) {
        tasksList.innerHTML = '<div class="empty-state">Nenhuma tarefa encontrada</div>';
        return;
    }
    
    tasksList.innerHTML = filteredTasks.slice(0, 5).map(task => {
        const priorityClass = task.priority || 'medium';
        const completedClass = task.completed ? 'completed' : '';
        const priorityText = {
            high: 'Alta',
            medium: 'Média',
            low: 'Baixa'
        }[priorityClass];
        
        return `
            <div class="task-item ${completedClass}">
                <div class="task-info">
                    <h3>${task.title}</h3>
                    <div class="task-meta">
                        <span>📅 ${formatDate(task.date)}</span>
                        <span>⏰ ${task.time}</span>
                        <span class="priority ${priorityClass}">${priorityText}</span>
                    </div>
                </div>
                <div class="task-actions">
                    <button class="task-btn complete-btn" onclick="toggleComplete(${task.id})">
                        ${task.completed ? '↩' : '✓'}
                    </button>
                </div>
            </div>
        `;
    }).join('');
}

// Renderização de projetos
function renderProjects() {
    const projectsList = document.getElementById('projectsList');
    const projectCount = document.querySelector('.project-count');
    
    const activeProjects = projects.filter(p => p.progress < 100);
    projectCount.textContent = `${activeProjects.length} ativos`;
    
    if (activeProjects.length === 0) {
        projectsList.innerHTML = '<div class="empty-state">Nenhum projeto ativo</div>';
        return;
    }
    
    projectsList.innerHTML = activeProjects.slice(0, 3).map(project => {
        const deadline = new Date(project.deadline);
        const now = getServerTime();
        const daysUntilDeadline = Math.ceil((deadline - now) / (1000 * 60 * 60 * 24));
        
        let deadlineClass = 'safe';
        let deadlineText = `${daysUntilDeadline} dias`;
        
        if (daysUntilDeadline < 0) {
            deadlineClass = '';
            deadlineText = 'Atrasado';
        } else if (daysUntilDeadline <= 3) {
            deadlineClass = '';
            deadlineText = 'Urgente';
        } else if (daysUntilDeadline <= 7) {
            deadlineClass = 'warning';
        }
        
        return `
            <div class="project-item" onclick="openProjectDetails(${project.id})" style="cursor: pointer;">
                <div class="project-header">
                    <div class="project-title">${project.title}</div>
                    <div class="project-deadline ${deadlineClass}">${deadlineText}</div>
                </div>
                <div class="project-progress">
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: ${project.progress}%"></div>
                    </div>
                    <div class="progress-text">
                        <span>${project.progress}% concluído</span>
                        <span>Deadline: ${formatDate(project.deadline)}</span>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

// Filtros
function filterTasks() {
    const today = getServerTime().toISOString().split('T')[0];
    
    switch (currentFilter) {
        case 'pending':
            return tasks.filter(t => !t.completed);
        case 'completed':
            return tasks.filter(t => t.completed);
        case 'today':
            return tasks.filter(t => t.date === today);
        default:
            return tasks;
    }
}

// Utilitários
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
}

function toggleComplete(id) {
    const task = tasks.find(t => t.id === id);
    if (task) {
        updateTask(id, { completed: !task.completed });
    }
}

// API calls
async function saveTask(task) {
    try {
        const response = await fetch(`${API_URL}/tasks`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(task)
        });
        const newTask = await response.json();
        tasks.push(newTask);
        renderTasks();
        renderCalendar();
        
        if (task.reminder && window.electronAPI && window.electronAPI.isElectron) {
            scheduleNotification(newTask);
        }
    } catch (error) {
        console.error('Error saving task:', error);
    }
}

async function saveProject(project) {
    try {
        const response = await fetch(`${API_URL}/projects`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(project)
        });
        const newProject = await response.json();
        projects.push(newProject);
        renderProjects();
        renderCalendar();
    } catch (error) {
        console.error('Error saving project:', error);
    }
}

async function updateTask(id, updates) {
    try {
        const response = await fetch(`${API_URL}/tasks/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updates)
        });
        const updatedTask = await response.json();
        const index = tasks.findIndex(t => t.id === id);
        if (index !== -1) {
            tasks[index] = updatedTask;
            renderTasks();
            renderCalendar();
        }
    } catch (error) {
        console.error('Error updating task:', error);
    }
}

// Inicialização de formulários
function initializeForms() {
    document.getElementById('taskForm').addEventListener('submit', (e) => {
        e.preventDefault();
        
        const task = {
            title: document.getElementById('title').value,
            description: document.getElementById('description').value,
            date: document.getElementById('date').value,
            time: document.getElementById('time').value,
            priority: document.getElementById('priority').value,
            category: document.getElementById('category').value,
            reminder: document.getElementById('reminder').checked,
            completed: false
        };
        
        saveTask(task);
        e.target.reset();
        showView('dashboard');
    });
    
    document.getElementById('projectForm').addEventListener('submit', (e) => {
        e.preventDefault();
        
        const project = {
            title: document.getElementById('projectTitle').value,
            description: document.getElementById('projectDescription').value,
            startDate: document.getElementById('projectStartDate').value,
            deadline: document.getElementById('projectDeadline').value,
            progress: parseInt(document.getElementById('projectProgress').value),
            priority: document.getElementById('projectPriority').value
        };
        
        saveProject(project);
        e.target.reset();
        showView('dashboard');
    });
    
    // Filtros de tarefas
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            currentFilter = btn.dataset.filter;
            renderTasks();
        });
    });
}

// Notificações
function scheduleNotification(task) {
    const taskTime = new Date(task.date + ' ' + task.time);
    const now = new Date();
    const timeDiff = taskTime - now;
    
    if (timeDiff > 0) {
        setTimeout(() => {
            if (Notification.permission === 'granted') {
                new Notification('Lembrete de Tarefa', {
                    body: task.title,
                    icon: '/electron/agenda.ico'
                });
            }
        }, timeDiff - 60000);
    }
}

// Permissão para notificações
if ('Notification' in window && Notification.permission === 'default') {
    Notification.requestPermission();
}

// Verificação periódica de notificações
setInterval(() => {
    tasks.forEach(task => {
        if (!task.completed && task.reminder) {
            const taskTime = new Date(task.date + ' ' + task.time);
            const now = new Date();
            const diff = taskTime - now;
            
            if (diff > 0 && diff <= 60000) {
                if (Notification.permission === 'granted') {
                    new Notification('Lembrete de Tarefa', {
                        body: task.title,
                        icon: '/electron/agenda.ico'
                    });
                }
            }
        }
    });
}, 30000);

// Funções do Modal
function closeModal() {
    document.getElementById('dayInfoModal').classList.remove('active');
}

// Event listeners adicionais para o modal
document.addEventListener('DOMContentLoaded', function() {
    // Já existe um DOMContentLoaded, vou adicionar essas funcionalidades nele
    
    const modal = document.getElementById('dayInfoModal');
    const closeBtn = document.querySelector('.close');
    
    // Fechar modal ao clicar no X
    if (closeBtn) {
        closeBtn.addEventListener('click', closeModal);
    }
    
    // Fechar modal ao clicar fora do conteúdo
    if (modal) {
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                closeModal();
            }
        });
    }
    
    // Fechar modal com ESC
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && modal && modal.classList.contains('active')) {
            closeModal();
        }
    });
});

// Gerenciamento de Projetos e suas Tarefas
// currentProjectId já declarado no topo

function openProjectDetails(projectId) {
    currentProjectId = projectId;
    const project = projects.find(p => p.id === projectId);
    
    if (!project) return;
    
    // Preencher os dados do projeto no formulário
    document.getElementById('editProjectId').value = project.id;
    document.getElementById('editProjectTitle').value = project.title;
    document.getElementById('editProjectDescription').value = project.description || '';
    document.getElementById('editProjectStartDate').value = project.startDate;
    document.getElementById('editProjectDeadline').value = project.deadline;
    document.getElementById('editProjectProgress').value = project.progress;
    document.getElementById('editProjectPriority').value = project.priority || 'medium';
    
    document.getElementById('projectDetailsTitle').textContent = `📁 ${project.title}`;
    
    // Carregar tarefas do projeto
    loadProjectTasks(projectId);
    
    // Mostrar a view de detalhes
    showView('project-details');
}

function loadProjectTasks(projectId) {
    // Filtrar tarefas do projeto atual
    const tasksFromProject = projectTasks.filter(task => task.projectId === projectId);
    
    // Verificar tarefas atrasadas e movê-las automaticamente
    const today = new Date().toISOString().split('T')[0];
    let hasOverdueTasks = false;
    
    tasksFromProject.forEach(task => {
        if (!task.completed && task.date < today) {
            task.date = today;
            task.isOverdue = true;
            hasOverdueTasks = true;
        }
    });
    
    if (hasOverdueTasks) {
        saveProjectTasks();
    }
    
    renderProjectTasks(tasksFromProject);
}

function renderProjectTasks(tasks) {
    const tasksList = document.getElementById('projectTasksList');
    
    if (tasks.length === 0) {
        tasksList.innerHTML = '<div class="empty-state">📝 Nenhuma tarefa criada para este projeto ainda.<br><br>Clique em "+ Nova Tarefa" para começar!</div>';
        return;
    }
    
    tasksList.innerHTML = tasks.map(task => {
        const overdueClass = task.isOverdue ? 'overdue' : '';
        const completedClass = task.completed ? 'completed' : '';
        const status = task.completed ? '✅' : (task.isOverdue ? '⚠️' : '📋');
        const priority = task.priority || 'medium';
        const priorityText = { high: 'Alta', medium: 'Média', low: 'Baixa' }[priority];
        
        return `
            <div class="project-task-item ${overdueClass} ${completedClass}">
                <div class="task-title">${status} ${task.title}${task.isOverdue ? ' (ATRASADA)' : ''}</div>
                <div class="task-meta">
                    <span>📅 ${formatDate(task.date)}</span>
                    <span>⏰ ${task.time}</span>
                    <span>🔥 ${priorityText}</span>
                </div>
                ${task.description ? `<div style="margin-top: 8px; font-size: 0.9em; opacity: 0.8;">${task.description}</div>` : ''}
                <div class="task-actions">
                    <button class="task-btn complete-task-btn" onclick="toggleProjectTaskComplete(${task.id})">
                        ${task.completed ? 'Reabrir' : 'Concluir'}
                    </button>
                    <button class="task-btn delete-task-btn" onclick="deleteProjectTask(${task.id})">Excluir</button>
                </div>
            </div>
        `;
    }).join('');
}

function showAddProjectTaskForm() {
    document.getElementById('addProjectTaskForm').style.display = 'block';
    
    // Definir data mínima como hoje
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('projectTaskDate').value = today;
    document.getElementById('projectTaskDate').min = today;
}

function hideAddProjectTaskForm() {
    document.getElementById('addProjectTaskForm').style.display = 'none';
    document.getElementById('newProjectTaskForm').reset();
}

async function addProjectTask(taskData) {
    const newTask = {
        ...taskData,
        id: Date.now(),
        projectId: currentProjectId,
        completed: false,
        isOverdue: false,
        isProjectTask: true
    };
    
    projectTasks.push(newTask);
    await saveProjectTasks();
    loadProjectTasks(currentProjectId);
    hideAddProjectTaskForm();
    renderCalendar(); // Atualizar calendário
}

async function toggleProjectTaskComplete(taskId) {
    const task = projectTasks.find(t => t.id === taskId);
    if (task) {
        task.completed = !task.completed;
        await saveProjectTasks();
        loadProjectTasks(currentProjectId);
        renderCalendar();
    }
}

async function deleteProjectTask(taskId) {
    if (confirm('Tem certeza que deseja excluir esta tarefa?')) {
        projectTasks = projectTasks.filter(t => t.id !== taskId);
        await saveProjectTasks();
        loadProjectTasks(currentProjectId);
        renderCalendar();
    }
}

async function updateProject(projectData) {
    try {
        const response = await fetch(`${API_URL}/projects/${projectData.id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(projectData)
        });
        
        const updatedProject = await response.json();
        const index = projects.findIndex(p => p.id === projectData.id);
        if (index !== -1) {
            projects[index] = updatedProject;
            renderProjects();
            renderCalendar();
        }
    } catch (error) {
        console.error('Error updating project:', error);
        alert('Erro ao atualizar projeto!');
    }
}

async function deleteProject() {
    if (confirm('Tem certeza que deseja excluir este projeto e todas suas tarefas?')) {
        try {
            await fetch(`${API_URL}/projects/${currentProjectId}`, { method: 'DELETE' });
            
            // Remover projeto e suas tarefas
            projects = projects.filter(p => p.id !== currentProjectId);
            projectTasks = projectTasks.filter(t => t.projectId !== currentProjectId);
            
            await saveProjectTasks();
            renderProjects();
            renderCalendar();
            showView('dashboard');
        } catch (error) {
            console.error('Error deleting project:', error);
            alert('Erro ao excluir projeto!');
        }
    }
}

// Funções para salvar/carregar tarefas de projetos
async function saveProjectTasks() {
    try {
        const response = await fetch(`${API_URL}/project-tasks`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(projectTasks)
        });
    } catch (error) {
        console.error('Error saving project tasks:', error);
        // Salvar localmente como fallback
        localStorage.setItem('projectTasks', JSON.stringify(projectTasks));
    }
}

async function loadProjectTasks() {
    try {
        const response = await fetch(`${API_URL}/project-tasks`);
        if (response.ok) {
            projectTasks = await response.json() || [];
        } else {
            projectTasks = [];
        }
    } catch (error) {
        console.error('Error loading project tasks:', error);
        // Carregar do localStorage como fallback
        projectTasks = JSON.parse(localStorage.getItem('projectTasks') || '[]');
    }
}

// Inicialização dos formulários de projeto
function initializeProjectForms() {
    try {
        // Formulário de edição de projeto
        const editProjectForm = document.getElementById('editProjectForm');
        if (editProjectForm) {
            editProjectForm.addEventListener('submit', (e) => {
                e.preventDefault();
                
                const projectData = {
                    id: parseInt(document.getElementById('editProjectId').value),
                    title: document.getElementById('editProjectTitle').value,
                    description: document.getElementById('editProjectDescription').value,
                    startDate: document.getElementById('editProjectStartDate').value,
                    deadline: document.getElementById('editProjectDeadline').value,
                    progress: parseInt(document.getElementById('editProjectProgress').value),
                    priority: document.getElementById('editProjectPriority').value
                };
                
                updateProject(projectData);
            });
        }
        
        // Formulário de nova tarefa de projeto
        const newProjectTaskForm = document.getElementById('newProjectTaskForm');
        if (newProjectTaskForm) {
            newProjectTaskForm.addEventListener('submit', (e) => {
                e.preventDefault();
                
                const taskData = {
                    title: document.getElementById('projectTaskTitle').value,
                    description: document.getElementById('projectTaskDescription').value,
                    date: document.getElementById('projectTaskDate').value,
                    time: document.getElementById('projectTaskTime').value,
                    priority: document.getElementById('projectTaskPriority').value
                };
                
                addProjectTask(taskData);
            });
        }
    } catch (error) {
        console.error('Error initializing project forms:', error);
    }
}

// Atualizar função de renderização do calendário para incluir tarefas de projeto
function updateCalendarWithProjectTasks() {
    // Esta função será chamada dentro do renderCalendar para atualizar a lógica de pontos
    return projectTasks.filter(task => !task.completed);
}

// Project tasks foram integradas na inicialização principal

// Dashboard de 60 dias - Dados de países para requisitos de viagem
const countryData = {
    'EUA': {
        flag: '🇺🇸',
        visa: 'Visto B1/B2 necessário',
        vaccines: 'COVID-19 recomendada',
        currency: 'Dólar (USD)',
        rate: 1,
        language: 'Inglês'
    },
    'França': {
        flag: '🇫🇷',
        visa: 'Não necessário (90 dias)',
        vaccines: 'Nenhuma obrigatória',
        currency: 'Euro (EUR)',
        rate: 5.5,
        language: 'Francês'
    },
    'Japão': {
        flag: '🇯🇵',
        visa: 'Não necessário (90 dias)',
        vaccines: 'Nenhuma obrigatória',
        currency: 'Iene (JPY)',
        rate: 0.034,
        language: 'Japonês'
    }
};

// Carregar Dashboard de 60 dias
async function loadDashboard60Days() {
    try {
        console.log('Loading 60-day dashboard...');
        const today = new Date();
        const endDate = new Date(today);
        endDate.setDate(endDate.getDate() + 60);

        // Atualizar período
        const periodElement = document.getElementById('period-range');
        if (periodElement) {
            periodElement.textContent = `${today.toLocaleDateString('pt-BR')} - ${endDate.toLocaleDateString('pt-BR')}`;
        }

        // Usar dados já carregados
        const tasksData = tasks || [];
        const projectsData = projects || [];

        // Filtrar eventos dos próximos 60 dias
        const next60DaysTasks = tasksData.filter(task => {
            if (!task.date) return false;
            const taskDate = new Date(task.date);
            return taskDate >= today && taskDate <= endDate;
        });

        // Contar eventos por categoria
        const flights = next60DaysTasks.filter(t => t.category === 'voo').length;
        const travels = next60DaysTasks.filter(t => t.category === 'viagem').length;
        const activeProjects = projectsData.filter(p => {
            if (!p.deadline) return false;
            const deadline = new Date(p.deadline);
            return deadline <= endDate && (p.progress || 0) < 100;
        }).length;

        // Atualizar métricas
        updateDashboardElement('total-events-60', next60DaysTasks.length);
        updateDashboardElement('total-meetings-60', 0); // Será atualizado quando meetings estiver implementado
        updateDashboardElement('total-tasks-60', next60DaysTasks.length);
        updateDashboardElement('total-flights-60', flights);
        updateDashboardElement('total-travels-60', travels);
        updateDashboardElement('projects-active-60', activeProjects);

        // Gerar seções
        generateDashboardTimeline60Days(next60DaysTasks, [], projectsData);
        generateDashboardProjectsDeadline(projectsData, endDate);
        generateDashboardTravelSchedule(next60DaysTasks);
        generateDashboardWorkloadChart(next60DaysTasks, []);

        console.log('60-day dashboard loaded successfully');
    } catch (error) {
        console.error('Error loading 60 days dashboard:', error);
    }
}

function updateDashboardElement(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value;
    }
}

function generateDashboardTimeline60Days(tasks, meetings, projects) {
    const timeline = document.getElementById('timeline-60days');
    if (!timeline) return;

    const events = [];

    // Adicionar tarefas importantes
    tasks.filter(t => t.priority === 'alta' || t.category === 'voo' || t.category === 'viagem')
        .forEach(task => {
            events.push({
                date: new Date(task.date),
                type: task.category === 'voo' ? 'flight' : task.category === 'viagem' ? 'travel' : 'task',
                title: task.title,
                icon: task.category === 'voo' ? '✈️' : task.category === 'viagem' ? '🌍' : '📋'
            });
        });

    // Adicionar deadlines de projetos
    projects.filter(p => (p.progress || 0) < 100 && p.deadline).forEach(project => {
        events.push({
            date: new Date(project.deadline),
            type: 'deadline',
            title: `Deadline: ${project.title}`,
            icon: '🎯'
        });
    });

    // Ordenar por data
    events.sort((a, b) => a.date - b.date);
    const topEvents = events.slice(0, 20);

    if (topEvents.length === 0) {
        timeline.innerHTML = '<p style="color: var(--text-muted); text-align: center; padding: 20px;">Nenhum evento importante nos próximos 60 dias</p>';
        return;
    }

    timeline.innerHTML = topEvents.map(event => `
        <div class="timeline-60days-item ${event.type}">
            <div class="timeline-60days-date">
                ${event.date.toLocaleDateString('pt-BR')}
            </div>
            <div class="timeline-60days-content">
                ${event.icon} ${event.title}
            </div>
        </div>
    `).join('');
}

function generateDashboardProjectsDeadline(projects, endDate) {
    const container = document.getElementById('projects-deadline-60');
    if (!container) return;

    const projectsWithDeadline = projects.filter(p => {
        if (!p.deadline) return false;
        const deadline = new Date(p.deadline);
        return deadline <= endDate && (p.progress || 0) < 100;
    }).sort((a, b) => new Date(a.deadline) - new Date(b.deadline));

    if (projectsWithDeadline.length === 0) {
        container.innerHTML = '<p style="color: var(--text-muted); text-align: center; padding: 20px;">Nenhum projeto com prazo nos próximos 60 dias</p>';
        return;
    }

    container.innerHTML = projectsWithDeadline.map(project => {
        const daysLeft = Math.ceil((new Date(project.deadline) - new Date()) / (1000 * 60 * 60 * 24));

        return `
            <div class="project-deadline-item">
                <div>
                    <div class="project-deadline-name">${project.title}</div>
                    <div class="progress-bar" style="width: 200px; height: 4px; background: var(--muted); margin-top: 4px;">
                        <div style="width: ${project.progress || 0}%; height: 100%; background: var(--primary);"></div>
                    </div>
                </div>
                <div>
                    <div class="project-deadline-date">${new Date(project.deadline).toLocaleDateString('pt-BR')}</div>
                    <div style="font-size: 11px; color: ${daysLeft <= 7 ? 'var(--danger)' : 'var(--text-muted)'};">
                        ${daysLeft} dias restantes
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function generateDashboardTravelSchedule(tasks) {
    const container = document.getElementById('travel-schedule-60');
    if (!container) return;

    const travels = tasks.filter(t => t.category === 'voo' || t.category === 'viagem')
        .sort((a, b) => new Date(a.date) - new Date(b.date));

    if (travels.length === 0) {
        container.innerHTML = '<p style="color: var(--text-muted); text-align: center; padding: 20px;">Nenhuma viagem programada nos próximos 60 dias</p>';
        return;
    }

    container.innerHTML = travels.map(travel => {
        const flag = travel.location ? countryData[travel.location]?.flag || '🌍' : '🌍';
        return `
            <div class="travel-schedule-item">
                <div class="travel-flag">${flag}</div>
                <div class="travel-details">
                    <div class="travel-destination">${travel.title}</div>
                    <div class="travel-dates">
                        ${new Date(travel.date).toLocaleDateString('pt-BR')}
                        ${travel.time ? `às ${travel.time}` : ''}
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function generateDashboardWorkloadChart(tasks, meetings) {
    const container = document.getElementById('workload-chart-60');
    if (!container) return;

    // Agrupar eventos por semana
    const weeks = {};
    const today = new Date();

    for (let i = 0; i < 9; i++) { // 9 semanas = ~60 dias
        const weekStart = new Date(today);
        weekStart.setDate(today.getDate() + (i * 7));
        const weekEnd = new Date(weekStart);
        weekEnd.setDate(weekStart.getDate() + 6);

        const weekKey = `S${i + 1}`;
        weeks[weekKey] = 0;

        // Contar eventos da semana
        tasks.forEach(task => {
            if (!task.date) return;
            const taskDate = new Date(task.date);
            if (taskDate >= weekStart && taskDate <= weekEnd) {
                weeks[weekKey]++;
            }
        });
    }

    const maxEvents = Math.max(...Object.values(weeks), 1);

    container.innerHTML = Object.entries(weeks).map(([week, count]) => {
        const height = Math.max((count / maxEvents) * 100, 5); // Mínimo 5% para visibilidade
        return `
            <div class="workload-bar" style="height: ${height}%;" title="${count} eventos na ${week}">
                <div class="workload-bar-label">${week}</div>
            </div>
        `;
    }).join('');
}

// =====================
// Assistente de Voz (pessoal)
// =====================
let mediaRecorder = null;
let audioChunks = [];
let isRecording = false;

function setupVoiceAssistant() {
    const btn = document.getElementById('voice-btn');
    if (!btn) return;

    btn.addEventListener('mousedown', startRecording);
    btn.addEventListener('touchstart', startRecording);
    btn.addEventListener('mouseup', stopRecordingAndSend);
    btn.addEventListener('mouseleave', handleMouseLeaveStop);
    btn.addEventListener('touchend', stopRecordingAndSend);
}

async function startRecording() {
    try {
        if (isRecording) return;
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        audioChunks = [];
        mediaRecorder = new MediaRecorder(stream, { mimeType: 'audio/webm' });
        mediaRecorder.ondataavailable = (evt) => { if (evt.data && evt.data.size > 0) audioChunks.push(evt.data); };
        mediaRecorder.start();
        isRecording = true;
        setVoiceUiState(true);
    } catch (err) {
        console.error('Mic error:', err);
        alert('Não foi possível acessar o microfone. Verifique as permissões.');
    }
}

function handleMouseLeaveStop() {
    if (isRecording) stopRecordingAndSend();
}

async function stopRecordingAndSend() {
    if (!isRecording || !mediaRecorder) return;
    isRecording = false;
    const mr = mediaRecorder;
    setVoiceUiState(false, true);
    mediaRecorder = null;
    mr.onstop = async () => {
        try {
            const blob = new Blob(audioChunks, { type: 'audio/webm' });
            const base64 = await blobToBase64(blob);
            const resp = await fetch(`${API_URL}/voice/assist`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ audio: base64, mimeType: blob.type, history: [], systemPrompt: null })
            });
            if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
            const data = await resp.json();
            if (data?.audio) await playBase64Audio(data.audio, data.mimeType || 'audio/mpeg');
            showVoiceToast(data?.transcript, data?.replyText);
        } catch (err) {
            console.error('Voice assist error:', err);
            alert('Falha ao processar o áudio.');
        } finally {
            setVoiceUiState(false, false);
        }
    };
    mr.stop();
}

function setVoiceUiState(recording, loading) {
    const btn = document.getElementById('voice-btn');
    if (!btn) return;
    if (recording) {
        btn.textContent = '🛑';
        btn.title = 'Solte para enviar';
        btn.style.background = 'var(--danger)';
        btn.style.color = 'white';
    } else if (loading) {
        btn.textContent = '⏳';
        btn.title = 'Processando...';
        btn.style.background = 'var(--warning)';
        btn.style.color = 'white';
    } else {
        btn.textContent = '🎤';
        btn.title = 'Assistente de Voz (aperte para falar)';
        btn.style.background = '';
        btn.style.color = '';
    }
}

function blobToBase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => {
            const base64data = String(reader.result || '').split(',')[1];
            resolve(base64data);
        };
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

async function playBase64Audio(b64, mime = 'audio/mpeg') {
    const src = `data:${mime};base64,${b64}`;
    const audio = new Audio(src);
    await audio.play();
}

function showVoiceToast(transcript, reply) {
    try {
        console.log('Você disse:', transcript);
        console.log('Assistente:', reply);
    } catch {}
}