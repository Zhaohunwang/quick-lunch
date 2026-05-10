@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion

echo.
echo ============================================
echo   ProjectHub Quick Build
echo ============================================
echo.

set SCRIPT_DIR=%~dp0
for %%i in ("%SCRIPT_DIR%..") do set PROJECT_DIR=%%~fi

cd /d "%PROJECT_DIR%"

set PUBLISH_DIR=%PROJECT_DIR%\build\windows\win-x64\publish
set DIST_DIR=%PROJECT_DIR%\build\windows\dist

if not exist "%DIST_DIR%" mkdir "%DIST_DIR%"

echo [1/2] Publishing for win-x64...
echo.

dotnet publish ProjectHub.Desktop\ProjectHub.Desktop.csproj ^
    -r win-x64 ^
    --configuration Release ^
    --self-contained ^
    -o "%PUBLISH_DIR%" ^
    -p:UseAppHost=true ^
    -p:PublishSingleFile=true

if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] dotnet publish failed.
    pause
    exit /b 1
)

for /f "tokens=2 delims=<>" %%a in ('findstr /r "<Version>.*</Version>" ProjectHub.Desktop\ProjectHub.Desktop.csproj 2^>nul') do set APP_VERSION=%%a
if "!APP_VERSION!"=="" set APP_VERSION=1.0.0

set ZIP_NAME=ProjectHub-!APP_VERSION!-win-x64.zip

if exist "%DIST_DIR%\!ZIP_NAME!" del "%DIST_DIR%\!ZIP_NAME!"

powershell -NoProfile -Command "Compress-Archive -Path '%PUBLISH_DIR%\*' -DestinationPath '%DIST_DIR%\!ZIP_NAME!' -CompressionLevel Optimal"

echo.
echo ============================================
echo   Build Complete!
echo   Version: !APP_VERSION!
echo   Output:  %DIST_DIR%\!ZIP_NAME!
echo ============================================
echo.

pause
