@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul 2>&1
color 09
title Verificador de Instalação - Sistema de Agenda

set "INSTALL_DIR=%PROGRAMFILES%\SistemaAgenda"
set "FIRST_RUN_FILE=%LOCALAPPDATA%\SistemaAgenda\first_run.txt"
set "LOCAL_DIR=%LOCALAPPDATA%\SistemaAgenda"

if not exist "%LOCAL_DIR%" mkdir "%LOCAL_DIR%"

if exist "%FIRST_RUN_FILE%" (
    start "" "%INSTALL_DIR%\agenda.bat" 2>nul
    if errorlevel 1 (
        echo Sistema não encontrado. Executando instalação...
        call instalar.bat
    )
) else (
    cls
    echo.
    echo ╔════════════════════════════════════════════════════════════════════╗
    echo ║              PRIMEIRA EXECUÇÃO DETECTADA                          ║
    echo ╚════════════════════════════════════════════════════════════════════╝
    echo.
    echo Sistema não instalado. Iniciando instalação automática...
    echo.
    timeout /t 3 >nul
    echo primeira_execucao_%date%_%time% > "%FIRST_RUN_FILE%"
    call instalar.bat
)
exit