#Requires -Version 5.1
<#
.SYNOPSIS
    ProjectHub macOS 跨平台编译脚本（从 Windows 编译 macOS 应用）

.DESCRIPTION
    在 Windows 环境下交叉编译 ProjectHub Desktop 的 macOS 版本，
    创建 .app Bundle 目录结构。签名、公证和 DMG 创建需要在 macOS 上完成。

.PARAMETER Runtime
    目标运行时架构。默认为 osx-x64,osx-arm64（同时编译两种架构）。
    可选值: osx-x64, osx-arm64

.PARAMETER SelfContained
    是否自包含发布（包含 .NET 运行时）。默认为 $true。

.PARAMETER SingleFile
    是否发布为单文件。默认为 $true。

.PARAMETER Version
    应用版本号。默认从 csproj 读取。

.PARAMETER Configuration
    构建配置。默认为 Release。

.PARAMETER Clean
    是否在发布前清理构建产物。默认为 $true。

.EXAMPLE
    .\scripts\build-macos.ps1

.EXAMPLE
    .\scripts\build-macos.ps1 -Runtime osx-x64,osx-arm64 -Version "1.2.0"

.EXAMPLE
    .\scripts\build-macos.ps1 -SelfContained $false -SingleFile $false
#>
param(
    [ValidateSet("osx-x64", "osx-arm64")]
    [string[]]$Runtime = @("osx-x64", "osx-arm64"),

    [bool]$SelfContained = $true,

    [bool]$SingleFile = $true,

    [string]$Version = "",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [bool]$Clean = $true
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$ProjectFile = Join-Path $ProjectDir "ProjectHub.Desktop\ProjectHub.Desktop.csproj"
$BuildRoot = Join-Path $ProjectDir "build\mac"

$AppName = "ProjectHub"
$BundleId = "com.projecthub.macos"
$ExeName = "ProjectHub.Desktop"
$IconName = "ProjectHub.icns"

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

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
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

    $exePath = Join-Path $PublishDir $ExeName
    $dllPath = Join-Path $PublishDir "$ExeName.dll"

    if (-not (Test-Path $exePath)) {
        Write-ErrMsg "Missing executable: $exePath"
        Write-ErrMsg "Ensure UseAppHost=true is set in csproj or via -p:UseAppHost=true"
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

function Get-IconPath {
    $iconCandidates = @(
        (Join-Path $BuildRoot $IconName),
        (Join-Path $ProjectDir "build\$IconName"),
        (Join-Path $ProjectDir $IconName),
        (Join-Path $ProjectDir "ProjectHub.Desktop\Assets\Icons\$IconName"),
        (Join-Path $ProjectDir "ProjectHub.Desktop\Assets\app-icon.icns")
    )

    foreach ($icon in $iconCandidates) {
        if (Test-Path $icon) {
            return $icon
        }
    }

    Write-Warn "No .icns icon file found."
    Write-Warn "Searching for .ico files to provide guidance..."

    $icoCandidates = @(
        (Join-Path $BuildRoot "app-icon.ico"),
        (Join-Path $ProjectDir "build\app-icon.ico"),
        (Join-Path $ProjectDir "ProjectHub.Desktop\Assets\app-icon.ico")
    )

    foreach ($ico in $icoCandidates) {
        if (Test-Path $ico) {
            Write-Info "Found .ico file: $ico"
            Write-Warn "Windows cannot convert .ico to .icns directly."
            Write-Warn "To fix the macOS app icon, do ONE of the following:"
            Write-Warn "  Option A: On a Mac, run: ./scripts/build-macos.sh (auto-converts .ico to .icns)"
            Write-Warn "  Option B: Convert $ico to .icns using an online tool"
            Write-Warn "           e.g. https://convertio.co/ or https://iconverticons.com/"
            Write-Warn "  Option C: Place the generated $IconName in $BuildRoot\"
            return $null
        }
    }

    Write-Warn "The app will have a default icon."
    Write-Warn "Place $IconName in $BuildRoot\ to include your app icon."
    Write-Warn "Conversion guide: https://docs.avaloniaui.net/docs/deployment/macos#creating-icon-files"
    return $null
}

function New-InfoPlist {
    param(
        [string]$PlistPath,
        [string]$AppVersion
    )

    $plistContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$ExeName</string>
    <key>CFBundleName</key>
    <string>$AppName</string>
    <key>CFBundleDisplayName</key>
    <string>$AppName</string>
    <key>CFBundleIdentifier</key>
    <string>$BundleId</string>
    <key>CFBundleIconFile</key>
    <string>$IconName</string>
    <key>CFBundleVersion</key>
    <string>$AppVersion</string>
    <key>CFBundleShortVersionString</key>
    <string>$AppVersion</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.developer-tools</string>
</dict>
</plist>
"@

    Set-Content -Path $PlistPath -Value $plistContent -Encoding UTF8
    Write-Success "Info.plist generated (version: $AppVersion)"
}

function New-AppBundle {
    param(
        [string]$PublishDir,
        [string]$AppVersion,
        [string]$Rid
    )

    $bundleDir = Join-Path $BuildRoot "$Rid\$AppName.app"

    if (Test-Path $bundleDir) {
        Write-Info "Removing existing bundle: $bundleDir"
        Remove-Item -Recurse -Force $bundleDir
    }

    Write-Info "Creating bundle: $bundleDir"
    New-Item -ItemType Directory -Path "$bundleDir\Contents\MacOS" -Force | Out-Null
    New-Item -ItemType Directory -Path "$bundleDir\Contents\Resources" -Force | Out-Null

    $plistPath = Join-Path $bundleDir "Contents\Info.plist"
    New-InfoPlist -PlistPath $plistPath -AppVersion $AppVersion

    $iconPath = Get-IconPath
    if ($null -ne $iconPath -and (Test-Path $iconPath)) {
        Copy-Item $iconPath -Destination "$bundleDir\Contents\Resources\$IconName"
        Write-Success "Icon: $iconPath"
    }

    Write-Info "Copying publish output to bundle..."
    Copy-Item -Path (Join-Path $PublishDir "*") -Destination "$bundleDir\Contents\MacOS" -Recurse

    $exeInBundle = Join-Path $bundleDir "Contents\MacOS\$ExeName"
    $dllInBundle = Join-Path $bundleDir "Contents\MacOS\$ExeName.dll"
    $plistExists = Test-Path $plistPath

    if ((Test-Path $exeInBundle) -and $plistExists) {
        Write-Success "Bundle created: $bundleDir"
    }
    else {
        Write-ErrMsg "Bundle creation failed: missing required files"
        if (-not (Test-Path $exeInBundle)) {
            Write-ErrMsg "  Missing: $exeInBundle"
        }
        if (-not $plistExists) {
            Write-ErrMsg "  Missing: $plistPath"
        }
        return $null
    }

    $noteFile = Join-Path $bundleDir "Contents\MacOS\README_SIGNING.txt"
    $noteContent = @"
NOTE: This .app bundle was cross-compiled on Windows.

To complete macOS packaging, you MUST do the following on a Mac:

1. Set executable permissions:
   chmod +x "$AppName.app/Contents/MacOS/$ExeName"

2. Code sign (requires Apple Developer ID certificate):
   codesign --force --timestamp --options=runtime \
       --entitlements ProjectHub.entitlements \
       --sign "Developer ID Application: YourName (TEAM_ID)" \
       "$AppName.app/Contents/MacOS/$ExeName"

   codesign --force --timestamp --options=runtime \
       --entitlements ProjectHub.entitlements \
       --sign "Developer ID Application: YourName (TEAM_ID)" \
       "$AppName.app"

3. Notarize (requires Apple Developer account):
   ditto -c -k --sequesterRsrc --keepParent "$AppName.app" notarize.zip
   xcrun notarytool submit notarize.zip \
       --apple-id "your@email.com" \
       --team-id "TEAM_ID" \
       --password "app-specific-password" \
       --wait
   xcrun stapler staple "$AppName.app"

4. Create DMG (optional):
   hdiutil create -volname "$AppName" -srcfolder "." -ov -format UDZO "$AppName.dmg"

For details, see: docs/14_macos_packaging.md
"@
    Set-Content -Path $noteFile -Value $noteContent -Encoding UTF8

    return $bundleDir
}

function Show-Summary {
    param(
        [string]$AppVersion,
        [array]$Results
    )

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ProjectHub macOS Cross-Build Complete" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Version:   $AppVersion" -ForegroundColor White
    Write-Host "  Config:    $Configuration" -ForegroundColor White
    Write-Host "  SelfContained: $SelfContained" -ForegroundColor White
    Write-Host "  SingleFile:   $SingleFile" -ForegroundColor White
    Write-Host ""

    foreach ($result in $Results) {
        Write-Host "  [$($result.Rid)]" -ForegroundColor Yellow
        Write-Host "    Publish: $($result.PublishDir)" -ForegroundColor White
        Write-Host "    Bundle:  $($result.BundleDir)" -ForegroundColor White
    }

    Write-Host ""
    Write-Host "  NEXT STEPS:" -ForegroundColor Yellow
    Write-Host "    1. Copy build/mac/<rid>/<AppName>.app to a Mac" -ForegroundColor White
    Write-Host "    2. Run chmod +x on the executable" -ForegroundColor White
    Write-Host "    3. Code sign with a Developer ID certificate" -ForegroundColor White
    Write-Host "    4. Notarize with Apple" -ForegroundColor White
    Write-Host "    (See build/mac/<rid>/<AppName>.app/Contents/MacOS/README_SIGNING.txt)" -ForegroundColor White
    Write-Host ""
    Write-Host "  Or use build-macos.sh on macOS for the full pipeline." -ForegroundColor White
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
}

# ============================================================
# Main
# ============================================================

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  ProjectHub macOS Cross-Build Script" -ForegroundColor Magenta
Write-Host "  (Windows -> macOS compilation)" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta

Write-Step "Step 1/4: Pre-flight checks"

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
Write-Host ""
Write-Warn "NOTE: This script cross-compiles from Windows."
Write-Warn "Code signing and notarization must be done on a Mac."

$Results = @()

foreach ($rid in $Runtime) {

    Write-Step "Step 2/4: Publish for $rid"

    if ($Clean) {
        Invoke-Clean -Rid $rid
    }

    $publishDir = Invoke-Publish -Rid $rid -AppVersion $appVersion

    Write-Step "Step 3/4: Verify publish output ($rid)"

    $valid = Test-PublishOutput -PublishDir $publishDir
    if (-not $valid) {
        Write-ErrMsg "Publish output validation failed for $rid"
        exit 1
    }

    Write-Step "Step 4/4: Create .app Bundle ($rid)"

    $bundleDir = New-AppBundle -PublishDir $publishDir -AppVersion $appVersion -Rid $rid

    if ($null -eq $bundleDir) {
        Write-ErrMsg "Bundle creation failed for $rid"
        exit 1
    }

    $Results += @{
        Rid        = $rid
        PublishDir = $publishDir
        BundleDir  = $bundleDir
    }
}

Show-Summary -AppVersion $appVersion -Results $Results
