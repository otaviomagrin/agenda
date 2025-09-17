@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul 2>&1
color 0C
title Desinstalador - Sistema de Agenda

set "INSTALL_DIR=%PROGRAMFILES%\SistemaAgenda"
set "DESKTOP=%USERPROFILE%\Desktop"
set "STARTMENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs"

cls
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║         DESINSTALADOR DO SISTEMA DE AGENDA v1.0                   ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo ATENÇÃO: Isso removerá completamente o Sistema de Agenda!
echo.
echo Deseja continuar? (S/N)
choice /c SN /n
if errorlevel 2 exit

:STEP1
cls
call :DrawProgress 20 "Removendo atalhos..."
echo.
echo [×] Removendo atalho da área de trabalho
if exist "%DESKTOP%\Sistema Agenda.lnk" del "%DESKTOP%\Sistema Agenda.lnk"
timeout /t 1 >nul

:STEP2
cls
call :DrawProgress 40 "Removendo do menu iniciar..."
echo.
echo [×] Removendo pasta: %STARTMENU%\Sistema Agenda
if exist "%STARTMENU%\Sistema Agenda" rmdir /s /q "%STARTMENU%\Sistema Agenda"
timeout /t 1 >nul

:STEP3
cls
call :DrawProgress 60 "Removendo arquivos do sistema..."
echo.
echo [×] Removendo pasta de dados: %INSTALL_DIR%\data
if exist "%INSTALL_DIR%\data" rmdir /s /q "%INSTALL_DIR%\data"
timeout /t 1 >nul

:STEP4
cls
call :DrawProgress 80 "Removendo pasta principal..."
echo.
echo [×] Removendo pasta: %INSTALL_DIR%
if exist "%INSTALL_DIR%" rmdir /s /q "%INSTALL_DIR%"
timeout /t 1 >nul

:STEP5
cls
call :DrawProgress 100 "Finalizando desinstalação..."
echo.
echo [×] Limpeza concluída
timeout /t 2 >nul

cls
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║                   DESINSTALAÇÃO CONCLUÍDA!                        ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo [√] Sistema removido com sucesso!
echo.
echo Pressione qualquer tecla para sair...
pause >nul
exit

:DrawProgress
set /a filled=%1/5
set /a empty=20-%filled%
set "bar="
for /l %%i in (1,1,%filled%) do set "bar=!bar!█"
for /l %%i in (1,1,%empty%) do set "bar=!bar!░"

echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║         DESINSTALADOR DO SISTEMA DE AGENDA v1.0                   ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo  Progresso: [!bar!] %1%%
echo.
echo  Status: %~2
echo.
goto :eof