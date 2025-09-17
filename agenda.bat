@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul 2>&1
title Agenda - Sistema Completo
color 0A

set "INSTALL_DIR=%PROGRAMFILES%\SistemaAgenda"
set "FIRST_RUN_FILE=%LOCALAPPDATA%\SistemaAgenda\installed.txt"
set "LOCAL_DIR=%LOCALAPPDATA%\SistemaAgenda"
set "CURRENT_DIR=%~dp0"
set "ERROR_LOG=%TEMP%\agenda_error.log"

:: Limpa log de erros anterior
if exist "%ERROR_LOG%" del "%ERROR_LOG%"

:: Verifica se é primeira execução ou se precisa instalar
if not exist "%FIRST_RUN_FILE%" goto :AUTO_INSTALL
if not exist "%INSTALL_DIR%\package.json" goto :AUTO_INSTALL

:: Verifica se o Node.js está instalado
where node >nul 2>nul
if %errorlevel% neq 0 (
    call :ShowError "Node.js não está instalado no sistema" "Instale o Node.js de https://nodejs.org/"
    pause
    exit /b 1
)

:: Se já está instalado, pula para execução normal
goto :CHECK_DEPENDENCIES

:AUTO_INSTALL
:: INSTALAÇÃO AUTOMÁTICA COM BARRA DE PROGRESSO
cls
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║         PRIMEIRA EXECUÇÃO DETECTADA - INSTALAÇÃO AUTOMÁTICA       ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
timeout /t 2 >nul

:: Cria diretórios necessários
if not exist "%LOCAL_DIR%" (
    echo [→] Criando diretório local: %LOCAL_DIR%
    mkdir "%LOCAL_DIR%" 2>>"%ERROR_LOG%"
    if !errorlevel! neq 0 (
        call :ShowError "Falha ao criar diretório local" "Verifique as permissões em %LOCALAPPDATA%"
        pause
        exit /b 1
    )
)

:: Verifica permissões administrativas
net session >nul 2>&1
if errorlevel 1 (
    cls
    call :DrawProgress 5 "Solicitando permissões administrativas..."
    echo.
    echo [!] Reiniciando com privilégios de administrador...
    powershell -Command "Start-Process '%~f0' -Verb RunAs" 2>>"%ERROR_LOG%"
    if !errorlevel! neq 0 (
        call :ShowError "Falha ao obter permissões administrativas" "Execute o arquivo como Administrador manualmente"
        pause
    )
    exit
)

:: Inicia processo de instalação com barra de progresso
set /a step=0
set "total_steps=10"

:: ETAPA 1: Criando estrutura
cls
call :DrawProgressWithDetails 10 "Criando estrutura de diretórios..." 1 %total_steps%
echo.
echo [→] Criando: %INSTALL_DIR%
if not exist "%INSTALL_DIR%" (
    mkdir "%INSTALL_DIR%" 2>>"%ERROR_LOG%"
    if !errorlevel! neq 0 (
        call :ShowError "Falha ao criar diretório de instalação" "Verifique permissões em %PROGRAMFILES%"
        pause
        exit /b 1
    )
)
echo [√] Diretório principal criado

echo [→] Criando: %INSTALL_DIR%\data
if not exist "%INSTALL_DIR%\data" mkdir "%INSTALL_DIR%\data" 2>>"%ERROR_LOG%"
echo [√] Diretório de dados criado

echo [→] Criando: %INSTALL_DIR%\electron
if not exist "%INSTALL_DIR%\electron" mkdir "%INSTALL_DIR%\electron" 2>>"%ERROR_LOG%"
echo [√] Diretório do Electron criado
timeout /t 1 >nul

:: ETAPA 2: Verificando arquivos necessários
cls
call :DrawProgressWithDetails 15 "Verificando arquivos necessários..." 2 %total_steps%
echo.
set "required_files=package.json main.js index.html style.css script.js"
set "missing_files="
for %%f in (%required_files%) do (
    if not exist "%CURRENT_DIR%%%f" (
        echo [X] Arquivo faltando: %%f
        set "missing_files=!missing_files! %%f"
    ) else (
        echo [√] Arquivo encontrado: %%f
    )
)

if not "!missing_files!"=="" (
    call :ShowError "Arquivos necessários não encontrados:" "Faltando:!missing_files!"
    pause
    exit /b 1
)
timeout /t 1 >nul

:: ETAPA 3: Copiando arquivos principais
cls
call :DrawProgressWithDetails 25 "Copiando arquivos do sistema..." 3 %total_steps%
echo.
set "files=package.json main.js index.html style.css script.js server.js sync-manager.js"
set /a file_count=0
set /a file_total=8

for %%f in (%files%) do (
    set /a file_count+=1
    if exist "%CURRENT_DIR%%%f" (
        echo [!file_count!/!file_total!] Copiando: %%f
        copy "%CURRENT_DIR%%%f" "%INSTALL_DIR%\" >nul 2>>"%ERROR_LOG%"
        if !errorlevel! neq 0 (
            echo [X] Erro ao copiar %%f
            type "%ERROR_LOG%"
        ) else (
            echo [√] Copiado com sucesso
        )
    )
)
timeout /t 1 >nul

:: ETAPA 4: Copiando pasta electron e recursos
cls
call :DrawProgressWithDetails 35 "Copiando recursos do Electron..." 4 %total_steps%
echo.
if exist "%CURRENT_DIR%electron" (
    echo [→] Copiando pasta electron...
    echo [!] Este processo pode demorar alguns segundos...
    xcopy "%CURRENT_DIR%electron" "%INSTALL_DIR%\electron" /E /I /Y >nul 2>>"%ERROR_LOG%"
    if !errorlevel! neq 0 (
        call :ShowError "Falha ao copiar recursos do Electron" "Verifique o espaço em disco"
        pause
        exit /b 1
    )
    echo [√] Recursos copiados com sucesso
)

:: Copia arquivos ARIA se existirem
if exist "%CURRENT_DIR%simple-aria-server.js" (
    echo [→] Copiando servidor ARIA...
    copy "%CURRENT_DIR%simple-aria-server.js" "%INSTALL_DIR%\" >nul 2>>"%ERROR_LOG%"
    echo [√] Servidor ARIA copiado
)
timeout /t 1 >nul

:: ETAPA 5: Verificando Node.js e npm
cls
call :DrawProgressWithDetails 45 "Verificando ambiente Node.js..." 5 %total_steps%
echo.
where node >nul 2>nul
if %errorlevel% neq 0 (
    echo [X] Node.js não encontrado
    echo.
    echo ╔════════════════════════════════════════════════════════════════════╗
    echo ║                       AÇÃO NECESSÁRIA                             ║
    echo ╚════════════════════════════════════════════════════════════════════╝
    echo.
    echo   O Node.js é necessário para executar este sistema.
    echo   Por favor, baixe e instale em: https://nodejs.org/
    echo.
    echo   Após instalar, execute este arquivo novamente.
    echo.
    pause
    exit /b 1
) else (
    echo [√] Node.js encontrado
    for /f "tokens=*" %%i in ('node -v') do echo     Versão: %%i
)

where npm >nul 2>nul
if %errorlevel% neq 0 (
    echo [X] npm não encontrado
    call :ShowError "npm não está disponível" "Reinstale o Node.js"
    pause
    exit /b 1
) else (
    echo [√] npm encontrado
    for /f "tokens=*" %%i in ('npm -v') do echo     Versão: %%i
)
timeout /t 1 >nul

:: ETAPA 6: Instalando dependências npm
cls
call :DrawProgressWithDetails 55 "Instalando dependências do sistema..." 6 %total_steps%
echo.
cd /d "%INSTALL_DIR%"
echo [→] Executando: npm install
echo [!] Este processo pode demorar vários minutos...
echo.

:: Mostra progresso do npm em tempo real
set "npm_start_time=%time%"
echo Iniciando instalação às %npm_start_time%
echo.
echo ┌────────────────────────────────────────────────────┐
echo │ Instalando pacotes:                               │
echo │   • express (servidor web)                        │
echo │   • electron (interface gráfica)                  │
echo │   • cors (comunicação entre serviços)             │
echo │   • body-parser (processamento de dados)          │
echo │   • dependências adicionais...                    │
echo └────────────────────────────────────────────────────┘
echo.

call npm install 2>>"%ERROR_LOG%"
if !errorlevel! neq 0 (
    echo.
    echo [X] Erro durante a instalação de dependências
    echo.
    echo Detalhes do erro:
    echo ────────────────────────────────────────────────────
    type "%ERROR_LOG%" | findstr /i "error warn"
    echo ────────────────────────────────────────────────────
    echo.
    echo Possíveis soluções:
    echo   1. Verifique sua conexão com a internet
    echo   2. Tente executar como Administrador
    echo   3. Limpe o cache do npm: npm cache clean --force
    echo.
    pause
    exit /b 1
)
echo [√] Todas as dependências instaladas com sucesso
timeout /t 1 >nul

:: ETAPA 7: Configurando permissões
cls
call :DrawProgressWithDetails 70 "Configurando permissões de acesso..." 7 %total_steps%
echo.
echo [→] Aplicando permissões em: %INSTALL_DIR%
icacls "%INSTALL_DIR%" /grant "%USERNAME%":F /T >nul 2>>"%ERROR_LOG%"
if !errorlevel! neq 0 (
    echo [!] Aviso: Não foi possível configurar todas as permissões
    echo     O sistema pode solicitar permissões ao executar
) else (
    echo [√] Permissões configuradas com sucesso
)
timeout /t 1 >nul

:: ETAPA 8: Criando arquivo de execução
cls
call :DrawProgressWithDetails 80 "Criando executáveis e scripts..." 8 %total_steps%
echo.
echo [→] Criando: %INSTALL_DIR%\iniciar.bat
(
echo @echo off
echo title Agenda de Tarefas
echo cd /d "%INSTALL_DIR%"
echo :: Inicia servidor ARIA se existir
echo if exist "simple-aria-server.js" (
echo     start /b node simple-aria-server.js
echo )
echo :: Inicia aplicação principal
echo start /b "" npm start
echo exit
) > "%INSTALL_DIR%\iniciar.bat"
if !errorlevel! neq 0 (
    echo [X] Erro ao criar executável
) else (
    echo [√] Executável criado com sucesso
)
timeout /t 1 >nul

:: ETAPA 9: Criando atalhos
cls
call :DrawProgressWithDetails 90 "Criando atalhos do sistema..." 9 %total_steps%
echo.

:: Atalho Desktop
set "DESKTOP=%USERPROFILE%\Desktop"
echo [→] Criando atalho na Área de Trabalho...
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%DESKTOP%\Agenda de Tarefas.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\iniciar.bat'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.IconLocation = '%SystemRoot%\System32\shell32.dll,13'; $Shortcut.Description = 'Sistema de Agenda'; $Shortcut.Save()" >nul 2>>"%ERROR_LOG%"
if !errorlevel! neq 0 (
    echo [!] Aviso: Não foi possível criar atalho no Desktop
) else (
    echo [√] Atalho criado no Desktop
)

:: Menu Iniciar
set "STARTMENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Agenda de Tarefas"
echo [→] Adicionando ao Menu Iniciar...
if not exist "%STARTMENU%" mkdir "%STARTMENU%" 2>>"%ERROR_LOG%"
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%STARTMENU%\Agenda de Tarefas.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\iniciar.bat'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.IconLocation = '%SystemRoot%\System32\shell32.dll,13'; $Shortcut.Description = 'Sistema de Agenda'; $Shortcut.Save()" >nul 2>>"%ERROR_LOG%"
if !errorlevel! neq 0 (
    echo [!] Aviso: Não foi possível adicionar ao Menu Iniciar
) else (
    echo [√] Adicionado ao Menu Iniciar
)
timeout /t 1 >nul

:: ETAPA 10: Finalizando instalação
cls
call :DrawProgressWithDetails 95 "Registrando instalação no sistema..." 10 %total_steps%
echo.
echo [→] Salvando registro de instalação
(
echo Instalado em: %date% %time%
echo Versão: 1.0
echo Local: %INSTALL_DIR%
echo Usuario: %USERNAME%
echo Node:
node -v 2>>"%ERROR_LOG%"
echo npm:
npm -v 2>>"%ERROR_LOG%"
) > "%FIRST_RUN_FILE%"

if !errorlevel! neq 0 (
    echo [!] Aviso: Não foi possível salvar registro completo
) else (
    echo [√] Instalação registrada com sucesso
)
timeout /t 1 >nul

:: ETAPA FINAL
cls
call :DrawProgressWithDetails 100 "Instalação concluída!" %total_steps% %total_steps%
echo.
echo [√] Sistema instalado com sucesso
timeout /t 2 >nul

cls
color 0A
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║                  INSTALAÇÃO CONCLUÍDA COM SUCESSO!                ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo   ┌─ Informações da Instalação ───────────────────────────────────┐
echo   │                                                               │
echo   │  Local: %INSTALL_DIR%
echo   │  Atalhos: Desktop e Menu Iniciar                            │
echo   │  Status: Pronto para uso                                    │
echo   │                                                               │
echo   └───────────────────────────────────────────────────────────────┘
echo.
echo   O sistema será iniciado automaticamente em 3 segundos...
echo.
timeout /t 3 >nul

:: Muda para o diretório de instalação e continua
cd /d "%INSTALL_DIR%"

:CHECK_DEPENDENCIES
:: Garante que está no diretório correto
echo [→] Verificando diretório de trabalho atual: %CD%

:: Determina o diretório correto para executar
if exist "%INSTALL_DIR%\package.json" (
    set "WORK_DIR=%INSTALL_DIR%"
    echo [→] Usando diretório de instalação: %INSTALL_DIR%
) else if exist "%CURRENT_DIR%package.json" (
    set "WORK_DIR=%CURRENT_DIR%"
    echo [→] Usando diretório do projeto: %CURRENT_DIR%
) else (
    call :ShowError "package.json não encontrado" "Verifique se os arquivos do projeto estão presentes"
    pause
    exit /b 1
)

:: Muda para o diretório correto
echo [→] Mudando para diretório: %WORK_DIR%
cd /d "%WORK_DIR%" 2>>"%ERROR_LOG%"
if !errorlevel! neq 0 (
    call :ShowError "Falha ao acessar diretório" "Não foi possível acessar: %WORK_DIR%"
    pause
    exit /b 1
)

echo [√] Diretório configurado: %CD%

:: Verifica se as dependências estão instaladas
if not exist "node_modules" (
    cls
    echo ╔════════════════════════════════════════════════════════════════════╗
    echo ║                    INSTALANDO DEPENDÊNCIAS                        ║
    echo ╚════════════════════════════════════════════════════════════════════╝
    echo.
    echo [→] Diretório de trabalho: %CD%
    echo [→] Instalando pacotes necessários...
    echo     Este processo será executado apenas uma vez
    echo.

    :: Verifica se package.json existe no diretório atual
    if not exist "package.json" (
        echo [X] package.json não encontrado no diretório atual
        echo     Diretório: %CD%
        echo     Listando arquivos:
        dir /b
        echo.
        pause
        exit /b 1
    )

    echo [→] package.json encontrado, iniciando instalação...
    call npm install 2>>"%ERROR_LOG%"
    if !errorlevel! neq 0 (
        echo.
        echo [X] Erro na instalação de dependências
        echo.
        echo [!] Diretório atual: %CD%
        echo [!] Conteúdo do diretório:
        dir /b
        echo.
        echo Erro encontrado:
        if exist "%ERROR_LOG%" (
            type "%ERROR_LOG%" | findstr /i "error npm" | head -10
        )
        echo.
        echo Tentando soluções alternativas:
        echo [→] Limpando cache do npm...
        npm cache clean --force 2>>nul
        echo [→] Tentando instalação novamente...
        call npm install 2>>"%ERROR_LOG%"
        if !errorlevel! neq 0 (
            echo [X] Falha na segunda tentativa
            pause
            exit /b 1
        )
    )

    echo.
    echo [√] Dependências instaladas com sucesso!
    echo.
    timeout /t 2 >nul
)

:: Verifica se existe o executável já compilado
if exist "dist\Agenda de Tarefas*.exe" (
    cls
    echo ╔════════════════════════════════════════════════════════════════════╗
    echo ║                   INICIANDO AGENDA INSTALADA                      ║
    echo ╚════════════════════════════════════════════════════════════════════╝
    echo.
    echo [→] Iniciando aplicação compilada...
    cd dist
    for %%i in ("Agenda de Tarefas*.exe") do (
        start "" "%%i" 2>>"%ERROR_LOG%"
        if !errorlevel! neq 0 (
            echo [X] Erro ao iniciar aplicação
            type "%ERROR_LOG%"
            pause
            exit /b 1
        )
        goto fim
    )
)

:: Se não existe executável, inicia versão de desenvolvimento
cls
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║                        AGENDA DE TAREFAS                          ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.

:: Garante que está no diretório correto antes de iniciar
echo [→] Verificando diretório atual: %CD%
if not exist "package.json" (
    echo [!] package.json não encontrado no diretório atual
    echo [→] Tentando acessar diretório do projeto...

    if exist "%CURRENT_DIR%package.json" (
        echo [→] Mudando para: %CURRENT_DIR%
        cd /d "%CURRENT_DIR%" 2>>"%ERROR_LOG%"
    ) else if exist "%INSTALL_DIR%\package.json" (
        echo [→] Mudando para: %INSTALL_DIR%
        cd /d "%INSTALL_DIR%" 2>>"%ERROR_LOG%"
    ) else (
        call :ShowError "Projeto não encontrado" "package.json não está disponível em nenhum diretório"
        pause
        exit /b 1
    )
)

echo [√] Diretório correto configurado: %CD%

:: Verifica se já está rodando
echo [→] Verificando se o sistema já está em execução...
tasklist /fi "imagename eq electron.exe" 2>nul | find /i "electron.exe" >nul
if %errorlevel% equ 0 (
    echo.
    echo ╔════════════════════════════════════════════════════════════════════╗
    echo ║                   APLICATIVO JÁ ESTÁ RODANDO!                     ║
    echo ╚════════════════════════════════════════════════════════════════════╝
    echo.
    echo   A Agenda já está aberta na bandeja do sistema.
    echo   Procure o ícone na área de notificação (próximo ao relógio).
    echo.
    echo   Fechando em 3 segundos...
    timeout /t 3 >nul
    exit
)

echo [→] Iniciando sistema...
echo     Diretório de trabalho: %CD%
echo.

:: Inicia servidor ARIA se existir
if exist "simple-aria-server.js" (
    echo [→] Iniciando servidor ARIA (porta 3002)...
    start /b node simple-aria-server.js 2>>"%ERROR_LOG%"
    if !errorlevel! neq 0 (
        echo [!] Aviso: Servidor ARIA não pôde ser iniciado
        echo     Integração de voz não estará disponível
        echo     Erro salvo em: %ERROR_LOG%
    ) else (
        echo [√] Servidor ARIA iniciado
    )
) else (
    echo [!] simple-aria-server.js não encontrado
    echo     Integração ARIA não estará disponível
)

:: Verifica se package.json existe antes de iniciar
if not exist "package.json" (
    call :ShowError "package.json não encontrado no diretório atual" "Verifique se está no diretório correto do projeto"
    pause
    exit /b 1
)

:: Inicia o Electron em background
echo [→] Iniciando interface gráfica...
echo     Executando: npm start
start /b "" npm start 2>>"%ERROR_LOG%" 1>nul

:: Aguarda o servidor iniciar com indicador visual
echo.
echo [→] Aguardando inicialização do sistema
set /a wait_count=0
:wait_loop
set /a wait_count+=1
if !wait_count! leq 10 (
    <nul set /p "=."
    ping -n 2 127.0.0.1 >nul 2>&1

    :: Verifica se iniciou
    tasklist /fi "imagename eq electron.exe" 2>nul | find /i "electron.exe" >nul
    if !errorlevel! equ 0 goto :startup_success

    goto :wait_loop
)

:: Se não iniciou após 10 tentativas
echo.
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║                         ERRO AO INICIAR                           ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo   O sistema não pôde ser iniciado após várias tentativas.
echo.
echo   📊 DIAGNÓSTICO DETALHADO:
echo   ───────────────────────────────────────────────────────────────────
echo   • Diretório de trabalho: %CD%
echo   • Node.js disponível:
node -v 2>nul || echo     [X] Node.js não encontrado
echo   • npm disponível:
npm -v 2>nul || echo     [X] npm não encontrado
echo   • package.json existe:
if exist "package.json" (echo     [√] Sim) else (echo     [X] Não)
echo   • node_modules existe:
if exist "node_modules" (echo     [√] Sim) else (echo     [X] Não)
echo.
echo   🔍 POSSÍVEIS CAUSAS:
echo     • Porta 3001 ou 3002 já em uso por outro programa
echo     • Antivírus bloqueando a execução do Node.js/Electron
echo     • Falta de memória RAM disponível
echo     • Dependências corrompidas (node_modules)
echo     • Firewall bloqueando conexões locais
echo     • Processo anterior ainda executando em background
echo.
echo   📋 VERIFICANDO LOG DE ERROS...
if exist "%ERROR_LOG%" (
    echo   ───────────────────────────────────────────────────────────────────
    echo   Últimos erros registrados:
    type "%ERROR_LOG%" | tail -10
    echo   ───────────────────────────────────────────────────────────────────
) else (
    echo   [!] Nenhum log de erro encontrado em %ERROR_LOG%
)
echo.
echo   🔧 SOLUÇÕES SUGERIDAS:
echo     1. Feche outros programas para liberar memória
echo     2. Verifique se outro Electron/Node está rodando (Task Manager)
echo     3. Tente executar como Administrador
echo     4. Reinstale as dependências (delete node_modules e tente novamente)
echo     5. Temporariamente desative o antivírus
echo.
echo   ⚡ OPÇÕES:
echo     [1] Tentar novamente
echo     [2] Reinstalar dependências
echo     [3] Sair
echo.
choice /c 123 /n /m "Escolha uma opcao (1, 2 ou 3): "
if errorlevel 3 exit
if errorlevel 2 (
    echo.
    echo [→] Removendo node_modules...
    if exist "node_modules" rmdir /s /q "node_modules"
    echo [→] Limpando cache npm...
    npm cache clean --force 2>nul
    goto :CHECK_DEPENDENCIES
)
if errorlevel 1 goto :CHECK_DEPENDENCIES

:startup_success
echo.
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║                   SISTEMA INICIADO COM SUCESSO!                   ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo   ✓ Aplicativo rodando na bandeja do sistema
echo   ✓ Procure o ícone próximo ao relógio
echo.
if exist "simple-aria-server.js" (
    echo   ✓ Servidor ARIA ativo na porta 3002
    echo   ✓ Comandos de voz disponíveis
    echo.
)
echo   Esta janela será fechada em 3 segundos...
timeout /t 3 >nul
exit

:fim
exit

:: ════════════════════════════════════════════════════════════════════
:: FUNÇÕES AUXILIARES
:: ════════════════════════════════════════════════════════════════════

:DrawProgress
set /a filled=%1/5
set /a empty=20-%filled%
set "bar="
for /l %%i in (1,1,%filled%) do set "bar=!bar!█"
for /l %%i in (1,1,%empty%) do set "bar=!bar!░"

echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║              INSTALAÇÃO AUTOMÁTICA - SISTEMA AGENDA               ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo  Progresso: [!bar!] %1%%
echo.
echo  Status: %~2
echo  Local: %INSTALL_DIR%
echo.
goto :eof

:DrawProgressWithDetails
set /a filled=%1/5
set /a empty=20-%filled%
set "bar="
for /l %%i in (1,1,%filled%) do set "bar=!bar!█"
for /l %%i in (1,1,%empty%) do set "bar=!bar!░"

echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║              INSTALAÇÃO AUTOMÁTICA - SISTEMA AGENDA               ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo  Progresso Total: [!bar!] %1%%
echo.
echo  Etapa %3 de %4: %~2
echo  ────────────────────────────────────────────────────────────────────
goto :eof

:ShowError
cls
echo.
echo ╔════════════════════════════════════════════════════════════════════╗
echo ║                            ERRO DETECTADO                         ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.
echo  ⚠ ERRO: %~1
echo.
echo  📋 DESCRIÇÃO: %~2
echo.
echo  📂 CONTEXTO DO SISTEMA:
echo     • Diretório atual: %CD%
echo     • Usuário: %USERNAME%
echo     • Data/Hora: %DATE% %TIME%
echo     • Versão Windows: %OS%
echo.
if exist "%ERROR_LOG%" (
    echo  📄 LOG DE ERRO DETALHADO:
    echo     Arquivo: %ERROR_LOG%
    echo.
    echo  📊 ÚLTIMOS ERROS REGISTRADOS:
    echo  ────────────────────────────────────────────────────────────────────
    type "%ERROR_LOG%" | tail -5
    echo  ────────────────────────────────────────────────────────────────────
) else (
    echo  📄 LOG DE ERRO: Nenhum log disponível
)
echo.
echo  🔧 AÇÕES SUGERIDAS:
echo     1. Verifique as permissões do diretório
echo     2. Execute como Administrador se necessário
echo     3. Verifique se há espaço em disco suficiente
echo     4. Temporariamente desative o antivírus
echo.
echo  ⏸ Pressione qualquer tecla para continuar...
goto :eof