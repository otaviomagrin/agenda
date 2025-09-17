const fs = require('fs');
const path = require('path');
const os = require('os');

class SyncManager {
    constructor() {
        this.userHome = os.homedir();
        this.cloudPaths = this.detectCloudPaths();
        this.dbFileName = 'agenda_database.json';
        this.syncFolderName = 'AgendaDB';
        // Usar diretório atual para o banco de dados local
        this.localDbPath = path.join(__dirname, 'data', this.dbFileName);
        this.syncInterval = null;
        this.lastSync = null;
        this.syncEnabled = false;

        // Criar diretório de dados se não existir
        const dataDir = path.dirname(this.localDbPath);
        if (!fs.existsSync(dataDir)) {
            fs.mkdirSync(dataDir, { recursive: true });
        }
    }

    detectCloudPaths() {
        const paths = {
            onedrive: null,
            googledrive: null
        };

        // Detectar OneDrive
        const possibleOneDrivePaths = [
            path.join(this.userHome, 'OneDrive'),
            path.join(this.userHome, 'OneDrive - Personal'),
            path.join(this.userHome, 'OneDrive - Pessoal'),
            process.env.OneDrive,
            process.env.OneDriveCommercial,
            process.env.OneDriveConsumer
        ].filter(Boolean);

        for (const onedrivePath of possibleOneDrivePaths) {
            if (fs.existsSync(onedrivePath)) {
                paths.onedrive = onedrivePath;
                break;
            }
        }

        // Detectar Google Drive
        const possibleGoogleDrivePaths = [
            'G:\\My Drive',  // Caminho mais comum primeiro
            'G:\\Meu Drive',
            'G:\\Minha unidade',
            path.join(this.userHome, 'Google Drive'),
            path.join(this.userHome, 'GoogleDrive'),
            path.join(this.userHome, 'My Drive'),
            path.join(this.userHome, 'Meu Drive'),
            path.join(this.userHome, 'Google Drive File Stream', 'My Drive')
        ];

        for (const gdrivePath of possibleGoogleDrivePaths) {
            try {
                if (fs.existsSync(gdrivePath)) {
                    // Verificar se é possível escrever no diretório
                    const testPath = path.join(gdrivePath, '.test_write_' + Date.now() + '.tmp');
                    try {
                        fs.writeFileSync(testPath, 'test');
                        fs.unlinkSync(testPath);
                        paths.googledrive = gdrivePath;
                        console.log(`Google Drive detectado em: ${gdrivePath}`);
                        break;
                    } catch (writeError) {
                        console.log(`Não foi possível escrever em ${gdrivePath}: ${writeError.message}`);
                    }
                }
            } catch (error) {
                console.log(`Erro ao verificar ${gdrivePath}:`, error.message);
            }
        }

        console.log('Caminhos de nuvem detectados:', paths);
        return paths;
    }

    initializeSyncFolders() {
        const results = {
            onedrive: false,
            googledrive: false
        };

        // Criar pasta no OneDrive
        if (this.cloudPaths.onedrive) {
            const syncPath = path.join(this.cloudPaths.onedrive, this.syncFolderName);
            try {
                if (!fs.existsSync(syncPath)) {
                    fs.mkdirSync(syncPath, { recursive: true });
                }
                results.onedrive = true;
                console.log('✓ Pasta de sincronização criada no OneDrive:', syncPath);
            } catch (error) {
                console.error('Erro ao criar pasta no OneDrive:', error.message);
            }
        }

        // Criar pasta no Google Drive
        if (this.cloudPaths.googledrive) {
            const syncPath = path.join(this.cloudPaths.googledrive, this.syncFolderName);
            try {
                if (!fs.existsSync(syncPath)) {
                    fs.mkdirSync(syncPath, { recursive: true });
                }
                results.googledrive = true;
                console.log('✓ Pasta de sincronização criada no Google Drive:', syncPath);
            } catch (error) {
                console.error('Erro ao criar pasta no Google Drive:', error.message);
            }
        }

        return results;
    }

    getCloudDatabasePath(service) {
        if (!this.cloudPaths[service]) return null;
        return path.join(this.cloudPaths[service], this.syncFolderName, this.dbFileName);
    }

    async syncToCloud(service) {
        const cloudPath = this.getCloudDatabasePath(service);
        if (!cloudPath) {
            console.log(`${service} não está disponível`);
            return false;
        }

        try {
            // Criar pasta se não existir
            const syncFolder = path.dirname(cloudPath);
            if (!fs.existsSync(syncFolder)) {
                fs.mkdirSync(syncFolder, { recursive: true });
            }

            // Se não há banco local, criar um vazio
            if (!fs.existsSync(this.localDbPath)) {
                const emptyDb = {
                    tasks: [],
                    projects: [],
                    projectTasks: [],
                    recurringTasks: [],
                    _syncMetadata: {
                        lastSync: new Date().toISOString(),
                        deviceName: os.hostname(),
                        syncVersion: '1.0'
                    }
                };
                const dataDir = path.dirname(this.localDbPath);
                if (!fs.existsSync(dataDir)) {
                    fs.mkdirSync(dataDir, { recursive: true });
                }
                fs.writeFileSync(this.localDbPath, JSON.stringify(emptyDb, null, 2));
            }

            // Ler banco local
            const data = fs.readFileSync(this.localDbPath, 'utf8');
            let dbContent;

            try {
                dbContent = JSON.parse(data);
            } catch (parseError) {
                console.error('Erro ao fazer parse do banco local:', parseError);
                dbContent = { tasks: [], projects: [], projectTasks: [], recurringTasks: [] };
            }

            // Adicionar/atualizar metadados
            dbContent._syncMetadata = {
                lastSync: new Date().toISOString(),
                deviceName: os.hostname(),
                syncVersion: '1.0'
            };

            // Salvar na nuvem
            fs.writeFileSync(cloudPath, JSON.stringify(dbContent, null, 2));
            console.log(`✓ Sincronizado com ${service}:`, new Date().toLocaleString());
            return true;
        } catch (error) {
            console.error(`Erro ao sincronizar com ${service}:`, error.message);
            return false;
        }
    }

    async syncFromCloud(service) {
        const cloudPath = this.getCloudDatabasePath(service);
        if (!cloudPath || !fs.existsSync(cloudPath)) {
            console.log(`Nenhum banco de dados encontrado em ${service}`);
            return false;
        }

        try {
            const cloudDataRaw = fs.readFileSync(cloudPath, 'utf8');
            let cloudData;

            try {
                cloudData = JSON.parse(cloudDataRaw);
            } catch (parseError) {
                console.error(`Erro ao fazer parse do banco em ${service}:`, parseError);
                return false;
            }

            const cloudTimestamp = cloudData._syncMetadata?.lastSync;

            // Verificar se o banco local existe
            if (fs.existsSync(this.localDbPath)) {
                try {
                    const localDataRaw = fs.readFileSync(this.localDbPath, 'utf8');
                    const localData = JSON.parse(localDataRaw);
                    const localTimestamp = localData._syncMetadata?.lastSync;

                    // Comparar timestamps para evitar sobrescrever dados mais recentes
                    if (localTimestamp && cloudTimestamp) {
                        const localDate = new Date(localTimestamp);
                        const cloudDate = new Date(cloudTimestamp);

                        if (localDate > cloudDate) {
                            console.log(`Banco local mais recente que ${service}, mantendo local`);
                            return false;
                        }
                    }
                } catch (localError) {
                    console.error('Erro ao ler banco local, será sobrescrito:', localError);
                }
            }

            // Criar diretório de dados se não existir
            const dataDir = path.dirname(this.localDbPath);
            if (!fs.existsSync(dataDir)) {
                fs.mkdirSync(dataDir, { recursive: true });
            }

            // Fazer backup antes de sobrescrever
            if (fs.existsSync(this.localDbPath)) {
                const backupDir = path.join(dataDir, 'backups');
                if (!fs.existsSync(backupDir)) {
                    fs.mkdirSync(backupDir, { recursive: true });
                }
                const backupPath = path.join(backupDir, `backup_${Date.now()}.json`);
                fs.copyFileSync(this.localDbPath, backupPath);
                console.log('Backup criado:', backupPath);
            }

            // Copiar dados da nuvem para local
            fs.writeFileSync(this.localDbPath, JSON.stringify(cloudData, null, 2));
            console.log(`✓ Sincronizado de ${service}:`, new Date().toLocaleString());
            return true;
        } catch (error) {
            console.error(`Erro ao sincronizar de ${service}:`, error.message);
            return false;
        }
    }

    async performSync() {
        console.log('Iniciando sincronização...');
        const results = {
            onedrive: { upload: false, download: false },
            googledrive: { upload: false, download: false }
        };

        // Sincronizar com OneDrive
        if (this.cloudPaths.onedrive) {
            results.onedrive.download = await this.syncFromCloud('onedrive');
            results.onedrive.upload = await this.syncToCloud('onedrive');
        }

        // Sincronizar com Google Drive
        if (this.cloudPaths.googledrive) {
            results.googledrive.download = await this.syncFromCloud('googledrive');
            results.googledrive.upload = await this.syncToCloud('googledrive');
        }

        this.lastSync = new Date().toISOString();
        return results;
    }

    startAutoSync(intervalMinutes = 5) {
        if (this.syncInterval) {
            clearInterval(this.syncInterval);
        }

        // Sincronizar imediatamente
        this.performSync();

        // Configurar sincronização automática
        this.syncInterval = setInterval(() => {
            this.performSync();
        }, intervalMinutes * 60 * 1000);

        this.syncEnabled = true;
        console.log(`Sincronização automática ativada (a cada ${intervalMinutes} minutos)`);
    }

    stopAutoSync() {
        if (this.syncInterval) {
            clearInterval(this.syncInterval);
            this.syncInterval = null;
        }
        this.syncEnabled = false;
        console.log('Sincronização automática desativada');
    }

    // Detectar conflitos entre versões
    detectConflicts() {
        const conflicts = [];

        try {
            const services = ['onedrive', 'googledrive'];
            const databases = {};

            // Carregar todos os bancos de dados disponíveis
            for (const service of services) {
                const cloudPath = this.getCloudDatabasePath(service);
                if (cloudPath && fs.existsSync(cloudPath)) {
                    databases[service] = JSON.parse(fs.readFileSync(cloudPath));
                }
            }

            // Carregar banco local
            if (fs.existsSync(this.localDbPath)) {
                databases.local = JSON.parse(fs.readFileSync(this.localDbPath));
            }

            // Comparar timestamps
            const timestamps = {};
            for (const [key, db] of Object.entries(databases)) {
                if (db._syncMetadata?.lastSync) {
                    timestamps[key] = new Date(db._syncMetadata.lastSync);
                }
            }

            // Detectar conflitos (diferença maior que 1 hora entre sincronizações)
            const entries = Object.entries(timestamps);
            for (let i = 0; i < entries.length; i++) {
                for (let j = i + 1; j < entries.length; j++) {
                    const diff = Math.abs(entries[i][1] - entries[j][1]);
                    if (diff > 3600000) { // 1 hora em millisegundos
                        conflicts.push({
                            source1: entries[i][0],
                            source2: entries[j][0],
                            time1: entries[i][1],
                            time2: entries[j][1],
                            difference: diff
                        });
                    }
                }
            }
        } catch (error) {
            console.error('Erro ao detectar conflitos:', error);
        }

        return conflicts;
    }

    // Resolver conflitos mantendo a versão mais recente
    resolveConflicts() {
        try {
            const databases = {};
            let mostRecent = { source: null, timestamp: null, data: null };

            // Carregar todos os bancos disponíveis
            const services = ['onedrive', 'googledrive'];
            for (const service of services) {
                const cloudPath = this.getCloudDatabasePath(service);
                if (cloudPath && fs.existsSync(cloudPath)) {
                    try {
                        const dbRaw = fs.readFileSync(cloudPath, 'utf8');
                        const db = JSON.parse(dbRaw);
                        if (db._syncMetadata?.lastSync) {
                            const timestamp = new Date(db._syncMetadata.lastSync);
                            databases[service] = { data: db, timestamp };

                            if (!mostRecent.timestamp || timestamp > mostRecent.timestamp) {
                                mostRecent = { source: service, timestamp, data: db };
                            }
                        }
                    } catch (error) {
                        console.error(`Erro ao ler banco de ${service}:`, error);
                    }
                }
            }

            // Verificar banco local
            if (fs.existsSync(this.localDbPath)) {
                try {
                    const dbRaw = fs.readFileSync(this.localDbPath, 'utf8');
                    const db = JSON.parse(dbRaw);
                    if (db._syncMetadata?.lastSync) {
                        const timestamp = new Date(db._syncMetadata.lastSync);
                        databases.local = { data: db, timestamp };

                        if (!mostRecent.timestamp || timestamp > mostRecent.timestamp) {
                            mostRecent = { source: 'local', timestamp, data: db };
                        }
                    }
                } catch (error) {
                    console.error('Erro ao ler banco local:', error);
                }
            }

            // Se não encontrou nenhum banco válido, criar um novo
            if (!mostRecent.data) {
                console.log('Nenhum banco válido encontrado. Criando novo...');
                mostRecent.data = {
                    tasks: [],
                    projects: [],
                    projectTasks: [],
                    recurringTasks: [],
                    _syncMetadata: {
                        lastSync: new Date().toISOString(),
                        deviceName: os.hostname(),
                        syncVersion: '1.0'
                    }
                };
            }

            // Aplicar a versão mais recente em todos os locais
            console.log(`Resolvendo conflitos: usando versão de ${mostRecent.source || 'novo'} ${mostRecent.timestamp ? `(${mostRecent.timestamp.toLocaleString()})` : ''}`);

            // Garantir que o diretório local existe
            const dataDir = path.dirname(this.localDbPath);
            if (!fs.existsSync(dataDir)) {
                fs.mkdirSync(dataDir, { recursive: true });
            }

            // Atualizar local
            fs.writeFileSync(this.localDbPath, JSON.stringify(mostRecent.data, null, 2));

            // Atualizar nuvens
            for (const service of services) {
                if (this.cloudPaths[service]) {
                    this.syncToCloud(service).catch(err => {
                        console.error(`Erro ao sincronizar ${service} após resolver conflitos:`, err);
                    });
                }
            }

            return true;
        } catch (error) {
            console.error('Erro ao resolver conflitos:', error);
            return false;
        }
    }

    getStatus() {
        return {
            cloudPaths: this.cloudPaths,
            syncEnabled: this.syncEnabled,
            lastSync: this.lastSync,
            hasOneDrive: !!this.cloudPaths.onedrive,
            hasGoogleDrive: !!this.cloudPaths.googledrive,
            conflicts: this.detectConflicts()
        };
    }
}

module.exports = SyncManager;