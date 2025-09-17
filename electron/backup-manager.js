const fs = require('fs').promises;
const fsSync = require('fs');
const path = require('path');
const { execSync } = require('child_process');

class BackupManager {
    constructor(baseDir) {
        this.baseDir = baseDir;
        this.backupDir = path.join(baseDir, 'backups');
        this.dataBackupDir = path.join(this.backupDir, 'data');
        this.versionBackupDir = path.join(this.backupDir, 'versions');
        this.programBackupDir = path.join(this.backupDir, 'program');

        // Arquivos de dados para backup
        this.dataFiles = [
            'tasks.json',
            'meetings.json',
            'projects.json',
            'project-tasks.json'
        ];

        // Informações de versionamento
        this.currentVersion = {
            major: 1,
            minor: 0,
            patch: 0,
            stage: 'stable', // 'alpha', 'beta', 'stable'
            name: 'Electro V1',
            timestamp: new Date().toISOString()
        };

        this.ensureDirectories();
    }

    async ensureDirectories() {
        const dirs = [this.backupDir, this.dataBackupDir, this.versionBackupDir, this.programBackupDir];
        for (const dir of dirs) {
            if (!fsSync.existsSync(dir)) {
                await fs.mkdir(dir, { recursive: true });
            }
        }
    }

    // ========================
    // BACKUP DE DADOS
    // ========================

    async createDataBackup(label = null) {
        try {
            const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
            const backupLabel = label || `auto-${timestamp}`;
            const backupFolder = path.join(this.dataBackupDir, backupLabel);

            await fs.mkdir(backupFolder, { recursive: true });

            const backupInfo = {
                label: backupLabel,
                timestamp: new Date().toISOString(),
                type: 'data',
                files: [],
                size: 0
            };

            // Fazer backup de cada arquivo de dados
            for (const fileName of this.dataFiles) {
                const sourceFile = path.join(this.baseDir, fileName);
                const targetFile = path.join(backupFolder, fileName);

                if (fsSync.existsSync(sourceFile)) {
                    await fs.copyFile(sourceFile, targetFile);
                    const stats = await fs.stat(targetFile);
                    backupInfo.files.push({
                        name: fileName,
                        size: stats.size,
                        modified: stats.mtime
                    });
                    backupInfo.size += stats.size;
                }
            }

            // Salvar metadados do backup
            const metadataFile = path.join(backupFolder, 'backup-info.json');
            await fs.writeFile(metadataFile, JSON.stringify(backupInfo, null, 2));

            console.log(`✅ Data backup criado: ${backupLabel}`);
            return { success: true, backup: backupInfo };

        } catch (error) {
            console.error('❌ Erro ao criar backup de dados:', error);
            return { success: false, error: error.message };
        }
    }

    async restoreDataBackup(backupLabel) {
        try {
            const backupFolder = path.join(this.dataBackupDir, backupLabel);
            const metadataFile = path.join(backupFolder, 'backup-info.json');

            if (!fsSync.existsSync(metadataFile)) {
                throw new Error('Backup não encontrado');
            }

            const backupInfo = JSON.parse(await fs.readFile(metadataFile, 'utf8'));

            // Criar backup atual antes de restaurar
            await this.createDataBackup(`pre-restore-${Date.now()}`);

            // Restaurar cada arquivo
            for (const fileInfo of backupInfo.files) {
                const sourceFile = path.join(backupFolder, fileInfo.name);
                const targetFile = path.join(this.baseDir, fileInfo.name);

                if (fsSync.existsSync(sourceFile)) {
                    await fs.copyFile(sourceFile, targetFile);
                }
            }

            console.log(`✅ Dados restaurados do backup: ${backupLabel}`);
            return { success: true, restored: backupInfo };

        } catch (error) {
            console.error('❌ Erro ao restaurar backup:', error);
            return { success: false, error: error.message };
        }
    }

    async listDataBackups() {
        try {
            const backups = [];
            const items = await fs.readdir(this.dataBackupDir);

            for (const item of items) {
                const backupPath = path.join(this.dataBackupDir, item);
                const metadataPath = path.join(backupPath, 'backup-info.json');

                if (fsSync.existsSync(metadataPath)) {
                    const metadata = JSON.parse(await fs.readFile(metadataPath, 'utf8'));
                    backups.push(metadata);
                }
            }

            return backups.sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp));

        } catch (error) {
            console.error('❌ Erro ao listar backups:', error);
            return [];
        }
    }

    // ========================
    // BACKUP DO PROGRAMA
    // ========================

    async createProgramBackup(versionInfo = null) {
        try {
            const version = versionInfo || this.currentVersion;
            const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
            const backupLabel = `${version.name.replace(/\s+/g, '-')}-${version.stage}-v${version.major}.${version.minor}.${version.patch}-${timestamp}`;
            const backupFolder = path.join(this.programBackupDir, backupLabel);

            await fs.mkdir(backupFolder, { recursive: true });

            // Arquivos e pastas para incluir no backup
            const includeItems = [
                'index.html',
                'package.json',
                'electron/',
                'projeto/',
                'README.md',
                'AGENDA.bat'
            ];

            // Arquivos para excluir
            const excludeItems = [
                'node_modules',
                'dist',
                'backups',
                '.git',
                '*.log',
                'tasks.json',
                'meetings.json',
                'projects.json',
                'project-tasks.json'
            ];

            const backupInfo = {
                label: backupLabel,
                timestamp: new Date().toISOString(),
                type: 'program',
                version: version,
                files: [],
                size: 0
            };

            // Copiar arquivos e pastas
            for (const item of includeItems) {
                const sourcePath = path.join(this.baseDir, item);
                const targetPath = path.join(backupFolder, item);

                if (fsSync.existsSync(sourcePath)) {
                    const stats = await fs.stat(sourcePath);

                    if (stats.isDirectory()) {
                        await this.copyDirectory(sourcePath, targetPath, excludeItems);
                    } else {
                        await fs.copyFile(sourcePath, targetPath);
                        backupInfo.files.push({
                            name: item,
                            size: stats.size,
                            type: 'file'
                        });
                        backupInfo.size += stats.size;
                    }
                }
            }

            // Salvar metadados
            const metadataFile = path.join(backupFolder, 'backup-info.json');
            await fs.writeFile(metadataFile, JSON.stringify(backupInfo, null, 2));

            // Atualizar histórico de versões
            await this.updateVersionHistory(version, backupLabel);

            console.log(`✅ Program backup criado: ${backupLabel}`);
            return { success: true, backup: backupInfo };

        } catch (error) {
            console.error('❌ Erro ao criar backup do programa:', error);
            return { success: false, error: error.message };
        }
    }

    async copyDirectory(source, target, excludeItems = []) {
        await fs.mkdir(target, { recursive: true });
        const items = await fs.readdir(source);

        for (const item of items) {
            // Verificar se deve excluir
            if (excludeItems.some(exclude => item.includes(exclude))) {
                continue;
            }

            const sourcePath = path.join(source, item);
            const targetPath = path.join(target, item);
            const stats = await fs.stat(sourcePath);

            if (stats.isDirectory()) {
                await this.copyDirectory(sourcePath, targetPath, excludeItems);
            } else {
                await fs.copyFile(sourcePath, targetPath);
            }
        }
    }

    // ========================
    // SISTEMA DE VERSIONAMENTO
    // ========================

    async updateVersionHistory(version, backupLabel) {
        const historyFile = path.join(this.versionBackupDir, 'version-history.json');
        let history = [];

        if (fsSync.existsSync(historyFile)) {
            history = JSON.parse(await fs.readFile(historyFile, 'utf8'));
        }

        history.unshift({
            ...version,
            backupLabel,
            created: new Date().toISOString()
        });

        // Manter apenas últimas 50 versões
        if (history.length > 50) {
            history = history.slice(0, 50);
        }

        await fs.writeFile(historyFile, JSON.stringify(history, null, 2));
    }

    async getVersionHistory() {
        const historyFile = path.join(this.versionBackupDir, 'version-history.json');

        if (fsSync.existsSync(historyFile)) {
            return JSON.parse(await fs.readFile(historyFile, 'utf8'));
        }

        return [];
    }

    async promoteVersion(stage) {
        const stages = ['alpha', 'beta', 'stable'];
        const currentStageIndex = stages.indexOf(this.currentVersion.stage);
        const newStageIndex = stages.indexOf(stage);

        if (newStageIndex <= currentStageIndex) {
            throw new Error('Não é possível retroceder a versão');
        }

        const newVersion = {
            ...this.currentVersion,
            stage: stage,
            timestamp: new Date().toISOString()
        };

        if (stage === 'stable') {
            newVersion.name = `Electro V${newVersion.major}`;
        } else if (stage === 'beta') {
            newVersion.name = 'Electro Jarvis';
        }

        // Criar backup da versão promovida
        const result = await this.createProgramBackup(newVersion);

        if (result.success) {
            this.currentVersion = newVersion;
            await this.saveCurrentVersion();
        }

        return result;
    }

    async saveCurrentVersion() {
        const versionFile = path.join(this.baseDir, 'version.json');
        await fs.writeFile(versionFile, JSON.stringify(this.currentVersion, null, 2));
    }

    async loadCurrentVersion() {
        const versionFile = path.join(this.baseDir, 'version.json');

        if (fsSync.existsSync(versionFile)) {
            this.currentVersion = JSON.parse(await fs.readFile(versionFile, 'utf8'));
        }
    }

    // ========================
    // BACKUP AUTOMÁTICO
    // ========================

    async setupAutoBackup() {
        // Backup automático a cada 1 hora
        setInterval(async () => {
            await this.createDataBackup();
            await this.cleanOldBackups();
        }, 60 * 60 * 1000); // 1 hora

        console.log('🔄 Sistema de backup automático iniciado');
    }

    async cleanOldBackups() {
        try {
            const backups = await this.listDataBackups();
            const maxBackups = 24; // Manter últimas 24 horas

            if (backups.length > maxBackups) {
                const toDelete = backups.slice(maxBackups);

                for (const backup of toDelete) {
                    const backupPath = path.join(this.dataBackupDir, backup.label);
                    await fs.rmdir(backupPath, { recursive: true });
                }

                console.log(`🧹 Removidos ${toDelete.length} backups antigos`);
            }
        } catch (error) {
            console.error('❌ Erro ao limpar backups antigos:', error);
        }
    }

    // ========================
    // RELATÓRIOS
    // ========================

    async getBackupStatus() {
        const dataBackups = await this.listDataBackups();
        const versionHistory = await this.getVersionHistory();

        return {
            dataBackups: {
                count: dataBackups.length,
                latest: dataBackups[0] || null,
                totalSize: dataBackups.reduce((sum, b) => sum + (b.size || 0), 0)
            },
            versionHistory: {
                count: versionHistory.length,
                current: this.currentVersion,
                latest: versionHistory[0] || null
            },
            autoBackup: {
                enabled: true,
                interval: '1 hora',
                retention: '24 backups'
            }
        };
    }
}

module.exports = BackupManager;