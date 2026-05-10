#Requires -Version 5.1
<#
.SYNOPSIS
    ProjectHub Windows 打包脚本

.DESCRIPTION
    将 ProjectHub Desktop 应用发布为 Windows 可执行文件，并可选地创建安装包。

.PARAMETER Runtime
    目标运行时架构。默认为 win-x64。
    可选值: win-x64, win-x86, win-arm64

.PARAMETER SelfContained
    是否自包含发布（包含 .NET 运行时）。默认为 $true。

.PARAMETER SingleFile
    是否发布为单文件。默认为 $true。

.PARAMETER Version
    应用版本号。默认从 csproj 读取。

.PARAMETER Configuration
    构建配置。默认为 Release。

.PARAMETER CreateInstaller
    是否创建 Inno Setup 安装包。默认为 $false。

.PARAMETER Clean
    是否在发布前清理构建产物。默认为 $true。

.EXAMPLE
    .\scripts\build-windows.ps1

.EXAMPLE
    .\scripts\build-windows.ps1 -Runtime win-x64 -Version "1.2.0" -CreateInstaller

.EXAMPLE
    .\scripts\build-windows.ps1 -Runtime win-x64,win-arm64 -SelfContained $true
#>
param(
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string[]]$Runtime = @("win-x64"),

    [bool]$SelfContained = $true,

    [bool]$SingleFile = $true,

    [string]$Version = "",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [bool]$CreateInstaller = $false,

    [bool]$Clean = $true
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$ProjectFile = Join-Path $ProjectDir "ProjectHub.Desktop\ProjectHub.Desktop.csproj"
$BuildRoot = Join-Path $ProjectDir "build\windows"
$InstallerDir = Join-Path $ScriptDir "installer"

$AppName = "ProjectHub"
$ExeName = "ProjectHub.Desktop"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "=== $Message ===" -ForegroundColor Cyan
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Gray
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-ErrMsg {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Test-DotNetSdk {
    try {
        $dotnetVersion = & dotnet --version 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet not found"
        }
        Write-Info ".NET SDK version: $dotnetVersion"
        return $true
    }
    catch {
        Write-ErrMsg ".NET SDK not found. Please install .NET 8 SDK."
        Write-ErrMsg "Download: https://dotnet.microsoft.com/download/dotnet/8.0"
        return $false
    }
}

function Get-AppVersion {
    if ($Version -ne "") {
        return $Version
    }

    $csprojContent = Get-Content $ProjectFile -Raw
    $versionMatch = [regex]::Match($csprojContent, '<Version>(.*?)</Version>')
    if ($versionMatch.Success) {
        return $versionMatch.Groups[1].Value
    }

    $versionMatch = [regex]::Match($csprojContent, '<AssemblyVersion>(.*?)</AssemblyVersion>')
    if ($versionMatch.Success) {
        return $versionMatch.Groups[1].Value
    }

    return "1.0.0"
}

function Invoke-Clean {
    param([string]$Rid)

    $outputDir = Join-Path $BuildRoot $Rid
    if (Test-Path $outputDir) {
        Write-Info "Cleaning: $outputDir"
        Remove-Item -Recurse -Force $outputDir
    }

    $objDir = Join-Path $ProjectDir "ProjectHub.Desktop\obj"
    $binDir = Join-Path $ProjectDir "ProjectHub.Desktop\bin"
    foreach ($dir in @($objDir, $binDir)) {
        if (Test-Path $dir) {
            Write-Info "Cleaning: $dir"
            Remove-Item -Recurse -Force $dir
        }
    }
}

function Invoke-Publish {
    param(
        [string]$Rid,
        [string]$AppVersion
    )

    $publishDir = Join-Path $BuildRoot "$Rid\publish"

    $publishArgs = @(
        "publish"
        $ProjectFile
        "-r", $Rid
        "-c", $Configuration
        "-o", $publishDir
        "-p:UseAppHost=true"
        "-p:Version=$AppVersion"
        "-p:AssemblyVersion=$AppVersion"
        "-p:FileVersion=$AppVersion"
    )

    if ($SelfContained) {
        $publishArgs += "--self-contained"
    }

    if ($SingleFile) {
        $publishArgs += "-p:PublishSingleFile=true"
    }

    Write-Info "Command: dotnet $($publishArgs -join ' ')"
    Write-Host ""

    $null = & dotnet @publishArgs 2>&1 | Out-Host

    if ($LASTEXITCODE -ne 0) {
        Write-ErrMsg "dotnet publish failed for $Rid"
        exit 1
    }

    return $publishDir
}

function Test-PublishOutput {
    param([string]$PublishDir)

    $exePath = Join-Path $PublishDir "$ExeName.exe"
    $dllPath = Join-Path $PublishDir "$ExeName.dll"

    if (-not (Test-Path $exePath)) {
        Write-ErrMsg "Missing executable: $exePath"
        return $false
    }

    if ($SingleFile) {
        Write-Success "Executable: $exePath"
    }
    else {
        if (-not (Test-Path $dllPath)) {
            Write-ErrMsg "Missing DLL: $dllPath"
            return $false
        }
        Write-Success "Executable: $exePath"
        Write-Success "Assembly:   $dllPath"
    }

    $fileCount = (Get-ChildItem $PublishDir -File).Count
    Write-Info "Total files in publish directory: $fileCount"

    $totalSize = (Get-ChildItem $PublishDir -File | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [math]::Round($totalSize / 1MB, 2)
    Write-Info "Total size: ${sizeMB} MB"

    return $true
}

function New-DistributionPackage {
    param(
        [string]$PublishDir,
        [string]$Rid,
        [string]$AppVersion
    )

    $distDir = Join-Path $BuildRoot "dist"
    if (-not (Test-Path $distDir)) {
        New-Item -ItemType Directory -Path $distDir -Force | Out-Null
    }

    $zipName = "$AppName-$AppVersion-$Rid.zip"
    $zipPath = Join-Path $distDir $zipName

    if (Test-Path $zipPath) {
        Remove-Item -Force $zipPath
    }

    Write-Info "Creating zip: $zipPath"
    Compress-Archive -Path (Join-Path $PublishDir "*") -DestinationPath $zipPath -CompressionLevel Optimal

    $zipSize = (Get-Item $zipPath).Length
    $sizeMB = [math]::Round($zipSize / 1MB, 2)
    Write-Success "Distribution package: $zipPath (${sizeMB} MB)"

    return $zipPath
}

function Invoke-InnoSetupBuild {
    param(
        [string]$PublishDir,
        [string]$AppVersion,
        [string]$Rid
    )

    $isccPath = $null
    $possiblePaths = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
    )

    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }

    if ($null -eq $isccPath) {
        $isccInPath = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
        if ($null -ne $isccInPath) {
            $isccPath = $isccInPath.Source
        }
    }

    if ($null -eq $isccPath) {
        Write-ErrMsg "Inno Setup not found. Please install Inno Setup 6."
        Write-ErrMsg "Download: https://jrsoftware.org/isinfo.php"
        Write-ErrMsg "Skipping installer creation."
        return $null
    }

    Write-Info "Using Inno Setup: $isccPath"

    $issFile = Join-Path $ScriptDir "installer.iss"
    if (-not (Test-Path $issFile)) {
        Write-ErrMsg "Installer script not found: $issFile"
        Write-ErrMsg "Skipping installer creation."
        return $null
    }

    $distDir = Join-Path $BuildRoot "dist"
    if (-not (Test-Path $distDir)) {
        New-Item -ItemType Directory -Path $distDir -Force | Out-Null
    }

    $isccArgs = @(
        "/DAppVersion=$AppVersion"
        "/DPublishDir=$PublishDir"
        "/DOutputDir=$distDir"
        "/DRuntimeId=$Rid"
        $issFile
    )

    Write-Info "Building installer..."
    & $isccPath @isccArgs

    if ($LASTEXITCODE -ne 0) {
        Write-ErrMsg "Inno Setup build failed"
        return $null
    }

    $installerPattern = Join-Path $distDir "$AppName-Setup-*.exe"
    $installerFile = Get-ChildItem -Path $distDir -Filter "$AppName-Setup-*.exe" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -ne $installerFile) {
        $sizeMB = [math]::Round($installerFile.Length / 1MB, 2)
        Write-Success "Installer: $($installerFile.FullName) (${sizeMB} MB)"
        return $installerFile.FullName
    }

    return $null
}

function Show-Summary {
    param(
        [string]$AppVersion,
        [array]$Rids,
        [hashtable]$Results
    )

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ProjectHub Windows Build Complete" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Version:   $AppVersion" -ForegroundColor White
    Write-Host "  Config:    $Configuration" -ForegroundColor White
    Write-Host "  Runtime:   $($Rids -join ', ')" -ForegroundColor White
    Write-Host "  SelfContained: $SelfContained" -ForegroundColor White
    Write-Host "  SingleFile:   $SingleFile" -ForegroundColor White
    Write-Host ""

    foreach ($rid in $Rids) {
        $result = $Results[$rid]
        Write-Host "  [$rid]" -ForegroundColor Yellow
        Write-Host "    Publish:   $($result.PublishDir)" -ForegroundColor White
        if ($result.ZipPath) {
            Write-Host "    Zip:       $($result.ZipPath)" -ForegroundColor White
        }
        if ($result.InstallerPath) {
            Write-Host "    Installer: $($result.InstallerPath)" -ForegroundColor White
        }
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
}

# ============================================================
# Main
# ============================================================

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  ProjectHub Windows Build Script" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta

Write-Step "Step 1/5: Pre-flight checks"

if (-not (Test-DotNetSdk)) {
    exit 1
}

if (-not (Test-Path $ProjectFile)) {
    Write-ErrMsg "Project file not found: $ProjectFile"
    exit 1
}

$appVersion = Get-AppVersion
Write-Info "Application version: $appVersion"
Write-Info "Target runtime(s): $($Runtime -join ', ')"
Write-Info "Self-contained: $SelfContained"
Write-Info "Single file: $SingleFile"
Write-Info "Create installer: $CreateInstaller"

$Results = @{}

foreach ($rid in $Runtime) {

    Write-Step "Step 2/5: Publish for $rid"

    if ($Clean) {
        Invoke-Clean -Rid $rid
    }

    $publishDir = Invoke-Publish -Rid $rid -AppVersion $appVersion

    Write-Step "Step 3/5: Verify publish output"

    $valid = Test-PublishOutput -PublishDir $publishDir
    if (-not $valid) {
        Write-ErrMsg "Publish output validation failed for $rid"
        exit 1
    }

    Write-Step "Step 4/5: Create distribution package"

    $zipPath = New-DistributionPackage -PublishDir $publishDir -Rid $rid -AppVersion $appVersion

    $installerPath = $null
    if ($CreateInstaller -and $rid -eq "win-x64") {
        Write-Step "Step 5/5: Create installer"

        $installerPath = Invoke-InnoSetupBuild -PublishDir $publishDir -AppVersion $appVersion -Rid $rid
    }

    $Results[$rid] = @{
        PublishDir      = $publishDir
        ZipPath         = $zipPath
        InstallerPath   = $installerPath
    }
}

Show-Summary -AppVersion $appVersion -Rids $Runtime -Results $Results
