const { app, BrowserWindow, Notification, Tray, Menu, nativeImage, shell } = require('electron');
const path = require('path');
const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const fs = require('fs').promises;
const fsSync = require('fs');
const os = require('os');
const crypto = require('crypto');
const { createWriteStream, promises: fsp } = require('fs');
let OpenAI = null;
try { OpenAI = require('openai'); } catch (_) { /* opcional até instalar */ }
const BackupManager = require('./backup-manager');

// Evita múltiplas instâncias do aplicativo
const gotTheLock = app.requestSingleInstanceLock();

if (!gotTheLock) {
  // Se já existe uma instância rodando, fecha esta
  app.quit();
  return;
}

let mainWindow;
let tray;
let server;
let backupManager;
const PORT = 3001;

async function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.js')
    },
    icon: path.join(__dirname, 'agenda.ico'),
    autoHideMenuBar: true,
    show: false // Não mostrar imediatamente
  });

  mainWindow.loadURL('http://localhost:3001');

  mainWindow.once('ready-to-show', () => {
    console.log('Janela pronta, mostrando...');
    mainWindow.show();
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
    console.error('Falha ao carregar página:', errorCode, errorDescription);
  });

  mainWindow.webContents.on('did-finish-load', () => {
    console.log('Página carregada com sucesso');
  });

  mainWindow.on('minimize', (event) => {
    event.preventDefault();
    mainWindow.hide();
    if (tray) {
      tray.displayBalloon({
        title: 'Agenda Minimizada',
        content: 'O aplicativo continua rodando na bandeja do sistema'
      });
    }
  });

  mainWindow.on('close', (event) => {
    if (!app.isQuitting) {
      event.preventDefault();
      mainWindow.hide();
      return false;
    }
  });
}

function createTray() {
  try {
    // Tenta diferentes formatos de ícone (ordem de prioridade)
    const iconPaths = [
      path.join(__dirname, 'agenda.ico'),
      path.join(__dirname, 'agenda.png'),
      path.join(__dirname, 'app.ico'),
      path.join(__dirname, 'app.png'),
      path.join(__dirname, 'tray-icon.bmp'),
      path.join(__dirname, 'icon.png'),
      path.join(__dirname, 'icon.ico')
    ];
    
    let trayIcon = null;
    
    for (const iconPath of iconPaths) {
      if (fsSync.existsSync(iconPath)) {
        trayIcon = nativeImage.createFromPath(iconPath);
        if (!trayIcon.isEmpty()) {
          console.log('Using icon:', iconPath);
          break;
        }
      }
    }
    
    if (!trayIcon || trayIcon.isEmpty()) {
      console.log('No valid icon found, using default');
      // Cria um ícone padrão 16x16 com cor sólida
      trayIcon = nativeImage.createFromBuffer(Buffer.alloc(16 * 16 * 4, 255));
    }
    
    tray = new Tray(trayIcon);
    
    const contextMenu = Menu.buildFromTemplate([
    {
      label: 'Abrir Agenda',
      click: () => {
        mainWindow.show();
        mainWindow.focus();
      }
    },
    {
      label: 'Minimizar',
      click: () => {
        mainWindow.hide();
      }
    },
    { type: 'separator' },
    {
      label: 'Sair',
      click: () => {
        app.isQuitting = true;
        app.quit();
      }
    }
  ]);

  tray.setToolTip('Agenda de Tarefas');
  tray.setContextMenu(contextMenu);

  tray.on('click', () => {
    mainWindow.isVisible() ? mainWindow.hide() : mainWindow.show();
  });
  
  tray.on('double-click', () => {
    mainWindow.show();
    mainWindow.focus();
  });
  } catch (error) {
    console.log('Error creating tray:', error);
  }
}

// Mover funções para escopo global
const tasksDbPath = path.join(__dirname, '..', 'tasks.json');
const projectsDbPath = path.join(__dirname, '..', 'projects.json');
const projectTasksDbPath = path.join(__dirname, '..', 'project-tasks.json');
const meetingsDbPath = path.join(__dirname, '..', 'meetings.json');

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

async function loadMeetings() {
  try {
    const data = await fs.readFile(meetingsDbPath, 'utf8');
    return JSON.parse(data);
  } catch (error) {
    return [];
  }
}

async function saveMeetings(meetings) {
  await fs.writeFile(meetingsDbPath, JSON.stringify(meetings, null, 2));
}

async function startServer() {
  const app = express();

  // Inicializar sistema de backup
  backupManager = new BackupManager(path.join(__dirname, '..'));
  await backupManager.loadCurrentVersion();
  backupManager.setupAutoBackup();
  app.use(cors());
  // aumentar limite para aceitar áudio base64
  app.use(bodyParser.json({ limit: '25mb' }));
  app.use(express.static(path.join(__dirname, '..')));

  // Rota específica para a página do projeto
  app.get('/projeto', (req, res) => {
    res.sendFile(path.join(__dirname, '..', 'projeto', 'projeto.html'));
  });

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

  // Rotas para reuniões
  app.get('/api/meetings', async (req, res) => {
    const meetings = await loadMeetings();
    res.json(meetings);
  });

  app.post('/api/meetings', async (req, res) => {
    const meetings = await loadMeetings();
    const newMeeting = { ...req.body, id: Date.now() };
    meetings.push(newMeeting);
    await saveMeetings(meetings);
    res.json(newMeeting);
  });

  app.put('/api/meetings/:id', async (req, res) => {
    const meetings = await loadMeetings();
    const index = meetings.findIndex(m => m.id === parseInt(req.params.id));
    if (index !== -1) {
      meetings[index] = { ...meetings[index], ...req.body };
      await saveMeetings(meetings);
      res.json(meetings[index]);
    } else {
      res.status(404).json({ error: 'Meeting not found' });
    }
  });

  app.delete('/api/meetings/:id', async (req, res) => {
    let meetings = await loadMeetings();
    meetings = meetings.filter(m => m.id !== parseInt(req.params.id));
    await saveMeetings(meetings);
    res.json({ success: true });
  });

  // Endpoint de tempo (para sincronização do cliente)
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

  // ========================
  // ASSISTENTE DE VOZ (pessoal)
  // ========================
  const openaiApiKey = process.env.OPENAI_API_KEY;
  const openai = (OpenAI && openaiApiKey) ? new OpenAI({ apiKey: openaiApiKey }) : null;

  app.post('/api/voice/assist', async (req, res) => {
    try {
      if (!openai) {
        return res.status(500).json({ error: 'OPENAI_API_KEY não configurada' });
      }

      const { audio, mimeType, history, systemPrompt } = req.body || {};
      if (!audio) return res.status(400).json({ error: 'Campo audio (base64) é obrigatório' });

      // Salvar áudio temporariamente para passar ao Whisper
      const buffer = Buffer.from(audio, 'base64');
      const ext = mimeType && mimeType.includes('webm') ? '.webm' : mimeType && mimeType.includes('wav') ? '.wav' : '.mp3';
      const tmpPath = path.join(os.tmpdir(), `voice_${Date.now()}_${crypto.randomBytes(4).toString('hex')}${ext}`);
      await fsp.writeFile(tmpPath, buffer);

      // 1) STT (Whisper)
      const stt = await openai.audio.transcriptions.create({
        file: fsSync.createReadStream(tmpPath),
        model: 'whisper-1',
        // language opcional: 'pt'
      });
      const transcript = stt?.text || stt?.data?.text || '';

      // 2) CHAT (LLM)
      const messages = [];
      const sys = systemPrompt || 'Você é um assistente pessoal focado em produtividade (agenda, tarefas, projetos). Responda em PT-BR de forma objetiva e amigável.';
      messages.push({ role: 'system', content: sys });
      if (Array.isArray(history)) {
        for (const m of history) {
          if (m.role === 'user' || m.role === 'assistant' || m.role === 'system') {
            messages.push({ role: m.role, content: String(m.content || '') });
          }
        }
      }
      messages.push({ role: 'user', content: transcript || '(sem áudio reconhecido)' });

      const chat = await openai.chat.completions.create({
        model: 'gpt-4o-mini',
        messages,
        temperature: 0.3,
      });
      const replyText = chat.choices?.[0]?.message?.content || 'Certo.';

      // 3) TTS
      const speech = await openai.audio.speech.create({
        model: 'tts-1',
        voice: 'alloy',
        input: replyText,
        format: 'mp3'
      });
      const audioBuffer = Buffer.from(await speech.arrayBuffer());
      const audioB64 = audioBuffer.toString('base64');

      // limpeza do arquivo temp (best-effort)
      fsp.unlink(tmpPath).catch(() => {});

      res.json({
        transcript,
        replyText,
        audio: audioB64,
        mimeType: 'audio/mpeg'
      });
    } catch (err) {
      console.error('Erro em /api/voice/assist:', err);
      res.status(500).json({ error: 'Falha no assistente de voz', details: String(err?.message || err) });
    }
  });

  // ========================
  // ROTAS DE BACKUP
  // ========================

  // Status do sistema de backup
  app.get('/api/backup/status', async (req, res) => {
    try {
      const status = await backupManager.getBackupStatus();
      res.json(status);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });

  // Criar backup manual de dados
  app.post('/api/backup/data', async (req, res) => {
    try {
      const { label } = req.body;
      const result = await backupManager.createDataBackup(label);
      res.json(result);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });

  // Listar backups de dados
  app.get('/api/backup/data', async (req, res) => {
    try {
      const backups = await backupManager.listDataBackups();
      res.json(backups);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });

  // Restaurar backup de dados
  app.post('/api/backup/restore/:label', async (req, res) => {
    try {
      const { label } = req.params;
      const result = await backupManager.restoreDataBackup(label);
      res.json(result);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });

  // Criar backup do programa
  app.post('/api/backup/program', async (req, res) => {
    try {
      const { versionInfo } = req.body;
      const result = await backupManager.createProgramBackup(versionInfo);
      res.json(result);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });

  // Histórico de versões
  app.get('/api/backup/versions', async (req, res) => {
    try {
      const history = await backupManager.getVersionHistory();
      res.json(history);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });

  // Promover versão
  app.post('/api/backup/promote/:stage', async (req, res) => {
    try {
      const { stage } = req.params;
      const result = await backupManager.promoteVersion(stage);
      res.json(result);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });

  // Versão atual
  app.get('/api/backup/version/current', (req, res) => {
    res.json(backupManager.currentVersion);
  });

  return new Promise((resolve) => {
    server = app.listen(PORT, () => {
      console.log(`Server running on port ${PORT}`);
      resolve();
    });
  });
}

function checkNotifications() {
  setInterval(async () => {
    try {
      // Verificar tarefas
      const tasks = await loadTasks();
      const projects = await loadProjects();
      const now = new Date();
      
      tasks.forEach(task => {
        if (!task.completed && task.reminder) {
          const taskTime = new Date(task.date + ' ' + task.time);
          const diff = taskTime - now;
          
          if (diff > 0 && diff <= 60000) {
            new Notification({
              title: 'Lembrete de Tarefa',
              body: task.title,
              icon: path.join(__dirname, 'agenda.ico'),
              timeoutType: 'default'
            }).show();
          }
        }
      });

      // Verificar projetos próximos do deadline (3 dias antes)
      projects.forEach(project => {
        if (project.progress < 100) {
          const deadline = new Date(project.deadline);
          const diff = deadline - now;
          const daysUntilDeadline = diff / (1000 * 60 * 60 * 24);
          
          // Notificação 3 dias antes do deadline
          if (daysUntilDeadline <= 3 && daysUntilDeadline > 2.9) {
            new Notification({
              title: 'Projeto próximo do deadline',
              body: `${project.title} - 3 dias restantes`,
              icon: path.join(__dirname, 'agenda.ico'),
              timeoutType: 'default'
            }).show();
          }
          
          // Notificação no dia do deadline
          if (daysUntilDeadline <= 1 && daysUntilDeadline > 0.9) {
            new Notification({
              title: 'Deadline do projeto hoje!',
              body: `${project.title} - Deadline é hoje!`,
              icon: path.join(__dirname, 'agenda.ico'),
              timeoutType: 'default'
            }).show();
          }
        }
      });
    } catch (error) {
      console.error('Error checking notifications:', error);
    }
  }, 30000);
}

// Quando alguém tenta abrir uma segunda instância, foca na janela existente
app.on('second-instance', (event, commandLine, workingDirectory) => {
  if (mainWindow) {
    if (mainWindow.isMinimized()) mainWindow.restore();
    mainWindow.show();
    mainWindow.focus();
  }
});

app.whenReady().then(async () => {
  await startServer();
  createWindow();
  createTray();
  checkNotifications();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    } else {
      mainWindow.show();
    }
  });
});

app.on('window-all-closed', (event) => {
  if (process.platform !== 'darwin') {
    event.preventDefault();
  }
});

app.on('before-quit', () => {
  if (server) {
    server.close();
  }
});