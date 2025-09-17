@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul 2>&1
color 0A
title Instalador - Sistema de Agenda

set "INSTALL_DIR=%PROGRAMFILES%\SistemaAgenda"
set "DESKTOP=%USERPROFILE%\Desktop"
set "STARTMENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs"

cls
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║          INSTALADOR DO SISTEMA DE AGENDA v1.0                     ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo Local de instalação: %INSTALL_DIR%
echo.
timeout /t 3 >nul

set /a progress=0
set /a total=10

:STEP1
cls
call :DrawProgress 10 "Criando diretório principal..."
echo.
echo [√] Criando pasta: %INSTALL_DIR%
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
timeout /t 1 >nul

:STEP2
cls
call :DrawProgress 20 "Copiando arquivos do sistema..."
echo.
echo [√] Copiando arquivo: index.html
if exist "index.html" copy "index.html" "%INSTALL_DIR%\" >nul
timeout /t 1 >nul

:STEP3
cls
call :DrawProgress 30 "Copiando estilos..."
echo.
echo [√] Copiando arquivo: styles.css
if exist "styles.css" copy "styles.css" "%INSTALL_DIR%\" >nul
timeout /t 1 >nul

:STEP4
cls
call :DrawProgress 40 "Copiando scripts..."
echo.
echo [√] Copiando arquivo: script.js
if exist "script.js" copy "script.js" "%INSTALL_DIR%\" >nul
timeout /t 1 >nul

:STEP5
cls
call :DrawProgress 50 "Criando pasta de dados..."
echo.
echo [√] Criando pasta: %INSTALL_DIR%\data
if not exist "%INSTALL_DIR%\data" mkdir "%INSTALL_DIR%\data"
timeout /t 1 >nul

:STEP6
cls
call :DrawProgress 60 "Configurando permissões..."
echo.
echo [√] Aplicando permissões na pasta: %INSTALL_DIR%
icacls "%INSTALL_DIR%" /grant "%USERNAME%":F >nul 2>&1
timeout /t 1 >nul

:STEP7
cls
call :DrawProgress 70 "Criando arquivo de execução..."
echo.
echo [√] Criando launcher: %INSTALL_DIR%\agenda.bat
(
echo @echo off
echo start "" "%INSTALL_DIR%\index.html"
) > "%INSTALL_DIR%\agenda.bat"
timeout /t 1 >nul

:STEP8
cls
call :DrawProgress 80 "Criando atalho na área de trabalho..."
echo.
echo [√] Criando atalho em: %DESKTOP%
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%DESKTOP%\Sistema Agenda.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\agenda.bat'; $Shortcut.IconLocation = 'shell32.dll,1'; $Shortcut.Save()" >nul 2>&1
timeout /t 1 >nul

:STEP9
cls
call :DrawProgress 90 "Registrando no menu iniciar..."
echo.
echo [√] Adicionando ao Menu Iniciar: %STARTMENU%
if not exist "%STARTMENU%\Sistema Agenda" mkdir "%STARTMENU%\Sistema Agenda"
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%STARTMENU%\Sistema Agenda\Sistema Agenda.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\agenda.bat'; $Shortcut.IconLocation = 'shell32.dll,1'; $Shortcut.Save()" >nul 2>&1
timeout /t 1 >nul

:STEP10
cls
call :DrawProgress 100 "Finalizando instalação..."
echo.
echo [√] Salvando configurações
echo %date% %time% > "%INSTALL_DIR%\install.log"
timeout /t 2 >nul

cls
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║                    INSTALAÇÃO CONCLUÍDA!                          ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo [√] Sistema instalado com sucesso em: %INSTALL_DIR%
echo [√] Atalho criado na área de trabalho
echo [√] Adicionado ao menu iniciar
echo.
echo Pressione qualquer tecla para abrir o sistema...
pause >nul
start "" "%INSTALL_DIR%\agenda.bat"
exit

:DrawProgress
set /a filled=%1/5
set /a empty=20-%filled%
set "bar="
for /l %%i in (1,1,%filled%) do set "bar=!bar!█"
for /l %%i in (1,1,%empty%) do set "bar=!bar!░"

echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║          INSTALADOR DO SISTEMA DE AGENDA v1.0                     ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo  Progresso: [!bar!] %1%%
echo.
echo  Status: %~2
echo.
goto :eof