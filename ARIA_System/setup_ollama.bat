@echo off
title ARIA Premium System - Ollama Setup
color 0A

echo ===============================================
echo     ARIA Premium System - Ollama Setup
echo ===============================================
echo.

echo [1/5] Verificando se Ollama esta instalado...
where ollama >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: Ollama nao encontrado!
    echo.
    echo Por favor, instale o Ollama primeiro:
    echo 1. Va para: https://ollama.ai/download
    echo 2. Baixe e instale o Ollama para Windows
    echo 3. Execute este script novamente
    echo.
    pause
    exit /b 1
)
echo OK: Ollama encontrado

echo.
echo [2/5] Verificando se servico Ollama esta rodando...
curl -s http://localhost:11434/api/tags >nul 2>&1
if %errorlevel% neq 0 (
    echo Iniciando servico Ollama...
    start "" ollama serve
    timeout /t 5 >nul
) else (
    echo OK: Servico Ollama ja esta rodando
)

echo.
echo [3/5] Instalando modelo llama3.1:8b (principal)...
echo Isso pode demorar alguns minutos...
ollama pull llama3.1:8b
if %errorlevel% neq 0 (
    echo ERRO: Falha ao baixar llama3.1:8b
    goto :error
)
echo OK: llama3.1:8b instalado

echo.
echo [4/5] Instalando modelo mistral:7b (backup)...
ollama pull mistral:7b
if %errorlevel% neq 0 (
    echo AVISO: Falha ao baixar mistral:7b (continuando...)
) else (
    echo OK: mistral:7b instalado
)

echo.
echo [5/5] Testando modelos instalados...
echo Testando llama3.1:8b...
echo {"model": "llama3.1:8b", "prompt": "Responda apenas OK", "stream": false} | curl -s -X POST http://localhost:11434/api/generate -d @- | findstr "OK" >nul
if %errorlevel% neq 0 (
    echo AVISO: Teste do llama3.1:8b falhou
) else (
    echo OK: llama3.1:8b funcionando
)

echo.
echo ===============================================
echo           CONFIGURACAO CONCLUIDA!
echo ===============================================
echo.
echo Modelos Ollama instalados com sucesso!
echo.
echo Para usar o ARIA com Ollama:
echo 1. O servico Ollama sera iniciado automaticamente
echo 2. O ARIA usara Ollama quando estiver offline
echo 3. Verifique as configuracoes no painel do ARIA
echo.
echo Comandos uteis:
echo - ollama list          : Lista modelos instalados
echo - ollama serve         : Inicia servico manualmente
echo - ollama pull [modelo] : Instala novos modelos
echo.
pause
exit /b 0

:error
echo.
echo ===============================================
echo              ERRO NA CONFIGURACAO
echo ===============================================
echo.
echo Algo deu errado durante a instalacao.
echo Verifique sua conexao com internet e tente novamente.
echo.
pause
exit /b 1