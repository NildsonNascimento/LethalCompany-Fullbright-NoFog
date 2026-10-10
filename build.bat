@echo off
REM Requer o .NET SDK (https://dotnet.microsoft.com/download). Nao precisa editar nenhum caminho.
REM Se o jogo estiver fora do local padrao: build.bat "D:\Caminho\Lethal Company"
if "%~1"=="" (
  dotnet build NildoFullbrightLite.csproj -c Release
) else (
  dotnet build NildoFullbrightLite.csproj -c Release -p:GameDir="%~1"
)
if errorlevel 1 (echo. & echo ERRO no build & pause & exit /b 1)
echo.
echo OK! Pacote pronto em dist\  (importe no r2modman: Settings ^> Import local mod)
pause
