// Integração do sistema de sincronização com a aplicação principal
const SyncManager = require('./sync-manager');

// Criar instância global do gerenciador de sincronização
let syncManager = null;

// Inicializar sincronização quando o app carregar
function initializeSync() {
    try {
        syncManager = new SyncManager();

        // Verificar e criar pastas de sincronização
        const results = syncManager.initializeSyncFolders();
        console.log('Pastas de sincronização inicializadas:', results);

        // Detectar caminhos das nuvens
        const status = syncManager.getStatus();

        if (status.hasOneDrive || status.hasGoogleDrive) {
            // Sincronizar dados existentes imediatamente
            syncManager.performSync().then(syncResults => {
                console.log('Sincronização inicial completa:', syncResults);

                // Iniciar sincronização automática a cada 5 minutos
                syncManager.startAutoSync(5);

                // Mostrar notificação de sucesso
                showSyncNotification('Sincronização ativada', 'success');
            });
        } else {
            console.log('Nenhum serviço de nuvem detectado');
            showSyncNotification('Nenhum serviço de nuvem encontrado', 'warning');
        }

        // Adicionar listener para mudanças no banco de dados
        watchDatabaseChanges();

    } catch (error) {
        console.error('Erro ao inicializar sincronização:', error);
        showSyncNotification('Erro ao inicializar sincronização', 'error');
    }
}

// Observar mudanças no banco de dados
function watchDatabaseChanges() {
    const fs = require('fs');
    const path = require('path');

    const dbPath = path.join(process.cwd(), 'data', 'agenda_database.json');

    // Usar fs.watch para detectar mudanças
    if (fs.existsSync(dbPath)) {
        fs.watch(dbPath, (eventType, filename) => {
            if (eventType === 'change') {
                console.log('Banco de dados modificado, sincronizando...');

                // Aguardar um pouco para evitar múltiplas sincronizações
                setTimeout(() => {
                    if (syncManager && syncManager.syncEnabled) {
                        syncManager.performSync();
                    }
                }, 2000);
            }
        });
    }
}

// Função para salvar dados com sincronização
async function saveWithSync(data) {
    const fs = require('fs');
    const path = require('path');

    try {
        const dbPath = path.join(process.cwd(), 'data', 'agenda_database.json');
        const dataDir = path.dirname(dbPath);

        // Criar diretório se não existir
        if (!fs.existsSync(dataDir)) {
            fs.mkdirSync(dataDir, { recursive: true });
        }

        // Adicionar metadados
        data._syncMetadata = {
            lastSync: new Date().toISOString(),
            deviceName: require('os').hostname(),
            syncVersion: '1.0'
        };

        // Salvar localmente
        fs.writeFileSync(dbPath, JSON.stringify(data, null, 2));

        // Sincronizar com as nuvens
        if (syncManager && syncManager.syncEnabled) {
            await syncManager.performSync();
        }

        return true;
    } catch (error) {
        console.error('Erro ao salvar com sincronização:', error);
        return false;
    }
}

// Função para carregar dados com sincronização
async function loadWithSync() {
    const fs = require('fs');
    const path = require('path');

    try {
        // Primeiro, tentar sincronizar dados da nuvem
        if (syncManager) {
            const conflicts = syncManager.detectConflicts();

            if (conflicts.length > 0) {
                console.log('Conflitos detectados:', conflicts);
                syncManager.resolveConflicts();
            } else {
                // Sincronizar normalmente
                await syncManager.performSync();
            }
        }

        // Carregar dados locais
        const dbPath = path.join(process.cwd(), 'data', 'agenda_database.json');

        if (fs.existsSync(dbPath)) {
            const data = JSON.parse(fs.readFileSync(dbPath));
            return data;
        }

        return null;
    } catch (error) {
        console.error('Erro ao carregar com sincronização:', error);
        return null;
    }
}

// Função para mostrar notificações de sincronização
function showSyncNotification(message, type) {
    // Se estiver no navegador
    if (typeof window !== 'undefined') {
        const notification = document.createElement('div');
        notification.className = `sync-notification ${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 20px;
            border-radius: 8px;
            z-index: 10000;
            animation: slideIn 0.3s ease;
            font-family: Arial, sans-serif;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        `;

        switch(type) {
            case 'success':
                notification.style.backgroundColor = '#4CAF50';
                notification.style.color = 'white';
                break;
            case 'warning':
                notification.style.backgroundColor = '#ff9800';
                notification.style.color = 'white';
                break;
            case 'error':
                notification.style.backgroundColor = '#f44336';
                notification.style.color = 'white';
                break;
            default:
                notification.style.backgroundColor = '#2196F3';
                notification.style.color = 'white';
        }

        document.body.appendChild(notification);

        // Remover após 5 segundos
        setTimeout(() => {
            notification.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => {
                notification.remove();
            }, 300);
        }, 5000);
    } else {
        // Se estiver no Node.js, apenas logar
        console.log(`[SYNC ${type.toUpperCase()}] ${message}`);
    }
}

// Adicionar CSS para animações (se no navegador)
if (typeof window !== 'undefined') {
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideIn {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }

        @keyframes slideOut {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(100%);
                opacity: 0;
            }
        }

        .sync-indicator {
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: white;
            border-radius: 12px;
            padding: 10px 15px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            display: flex;
            align-items: center;
            gap: 10px;
            font-family: Arial, sans-serif;
            font-size: 14px;
            z-index: 9999;
        }

        .sync-indicator.syncing::before {
            content: '';
            width: 16px;
            height: 16px;
            border: 2px solid #4CAF50;
            border-top-color: transparent;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        .sync-indicator.synced::before {
            content: '✓';
            color: #4CAF50;
            font-weight: bold;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }
    `;
    document.head.appendChild(style);
}

// Exportar funções para uso no app
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        initializeSync,
        saveWithSync,
        loadWithSync,
        syncManager,
        showSyncNotification
    };
}