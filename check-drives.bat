@echo off
echo ========================================
echo    VERIFICANDO DRIVES E PASTAS
echo ========================================
echo.

echo [1] Verificando unidades disponiveis:
wmic logicaldisk get name,description
echo.

echo [2] Verificando drive G:
if exist "G:\" (
    echo Drive G: encontrado
    echo.
    echo Conteudo de G:\
    dir "G:\" /b 2>nul
    echo.

    if exist "G:\Meu Drive" (
        echo [OK] Pasta "Meu Drive" encontrada em G:\
    ) else if exist "G:\My Drive" (
        echo [OK] Pasta "My Drive" encontrada em G:\
    ) else if exist "G:\Minha unidade" (
        echo [OK] Pasta "Minha unidade" encontrada em G:\
    ) else (
        echo [!] Nenhuma pasta padrao do Google Drive encontrada
        echo Tentando criar AgendaDB diretamente em G:\
    )
) else (
    echo [X] Drive G: nao encontrado
)

echo.
echo [3] Verificando OneDrive:
if exist "%USERPROFILE%\OneDrive" (
    echo [OK] OneDrive encontrado em: %USERPROFILE%\OneDrive
) else (
    echo [X] OneDrive nao encontrado
)

echo.
echo [4] Verificando Google Drive em outros locais:
if exist "%USERPROFILE%\Google Drive" (
    echo [OK] Google Drive encontrado em: %USERPROFILE%\Google Drive
) else if exist "%USERPROFILE%\GoogleDrive" (
    echo [OK] Google Drive encontrado em: %USERPROFILE%\GoogleDrive
) else (
    echo [X] Google Drive nao encontrado na pasta do usuario
)

echo.
pause