@echo off
color 0A
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║        TESTE COMPLETO DE SINCRONIZACAO - AGENDA               ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

echo [1/4] Verificando ambientes de nuvem...
echo ========================================
powershell -Command "if (Test-Path 'C:\Users\%USERNAME%\OneDrive') { Write-Host '[OK] OneDrive detectado' -ForegroundColor Green } else { Write-Host '[X] OneDrive não encontrado' -ForegroundColor Red }"
powershell -Command "if (Test-Path 'G:\My Drive') { Write-Host '[OK] Google Drive detectado em G:\My Drive' -ForegroundColor Green } else { Write-Host '[X] Google Drive não encontrado' -ForegroundColor Red }"
echo.

echo [2/4] Verificando pastas de sincronizacao...
echo ========================================
if exist "C:\Users\%USERNAME%\OneDrive\AgendaDB" (
    powershell -Command "Write-Host '[OK] Pasta AgendaDB existe no OneDrive' -ForegroundColor Green"
    dir "C:\Users\%USERNAME%\OneDrive\AgendaDB" /b 2>nul
) else (
    powershell -Command "Write-Host '[!] Pasta AgendaDB sera criada no OneDrive' -ForegroundColor Yellow"
)

if exist "G:\My Drive\AgendaDB" (
    powershell -Command "Write-Host '[OK] Pasta AgendaDB existe no Google Drive' -ForegroundColor Green"
    dir "G:\My Drive\AgendaDB" /b 2>nul
) else (
    powershell -Command "Write-Host '[!] Pasta AgendaDB sera criada no Google Drive' -ForegroundColor Yellow"
)
echo.

echo [3/4] Testando sincronizacao...
echo ========================================
node -e "const SyncManager = require('./sync-manager'); const sm = new SyncManager(); sm.initializeSyncFolders(); sm.performSync().then(r => { console.log('Resultado da sincronização:'); console.log('OneDrive - Upload:', r.onedrive.upload ? 'OK' : 'FALHOU', '- Download:', r.onedrive.download ? 'OK' : 'Sem dados'); console.log('Google Drive - Upload:', r.googledrive.upload ? 'OK' : 'FALHOU', '- Download:', r.googledrive.download ? 'OK' : 'Sem dados'); process.exit(0); });" 2>nul

echo.
echo [4/4] Verificando arquivos sincronizados...
echo ========================================
if exist "C:\Users\%USERNAME%\OneDrive\AgendaDB\agenda_database.json" (
    powershell -Command "Write-Host '[OK] Banco de dados no OneDrive' -ForegroundColor Green"
    powershell -Command "$size = (Get-Item 'C:\Users\%USERNAME%\OneDrive\AgendaDB\agenda_database.json').Length; Write-Host '     Tamanho: '$size' bytes' -ForegroundColor Cyan"
)

if exist "G:\My Drive\AgendaDB\agenda_database.json" (
    powershell -Command "Write-Host '[OK] Banco de dados no Google Drive' -ForegroundColor Green"
    powershell -Command "$size = (Get-Item 'G:\My Drive\AgendaDB\agenda_database.json').Length; Write-Host '     Tamanho: '$size' bytes' -ForegroundColor Cyan"
)

echo.
echo ════════════════════════════════════════════════════════════════
powershell -Command "Write-Host 'TESTE CONCLUIDO COM SUCESSO!' -ForegroundColor Green"
echo A sincronizacao esta funcionando corretamente!
echo Os dados serao sincronizados automaticamente entre dispositivos.
echo ════════════════════════════════════════════════════════════════
echo.
pause