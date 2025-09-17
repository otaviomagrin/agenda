@echo off
echo ========================================
echo     TESTE DE SINCRONIZACAO
echo ========================================
echo.

echo [1] Verificando Node.js...
node -v
if errorlevel 1 (
    echo [X] Node.js nao encontrado!
    pause
    exit
)

echo.
echo [2] Criando pasta de dados se necessario...
if not exist "data" mkdir data

echo.
echo [3] Testando modulo de sincronizacao...
node -e "const SyncManager = require('./sync-manager'); const sm = new SyncManager(); console.log('Status:', sm.getStatus()); sm.initializeSyncFolders(); console.log('Pastas inicializadas');"

echo.
echo [4] Testando servidor com sincronizacao...
echo.
echo Pressione Ctrl+C para parar o servidor de teste
echo.
timeout /t 3 >nul
node server.js

pause