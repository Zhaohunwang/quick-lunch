# macOS 打包方案

> 本文档基于 [AvaloniaUI macOS 部署文档](https://docs.avaloniaui.net/docs/deployment/macos)，结合 ProjectHub 项目实际情况，详细说明如何将应用打包为 macOS `.app` 应用包并分发。

---

## 1. 概述

macOS 应用以 `.app` Bundle 形式分发。`.app` 实际上是一个符合特定目录结构的文件夹，macOS 将其视为一个可启动的应用程序。它包含编译后的可执行文件、图标资源和元数据。

ProjectHub 的 `.app` Bundle 结构如下：

```
ProjectHub.app/
└── Contents/
    ├── _CodeSignature/          # 代码签名信息
    │   └── CodeResources
    ├── Info.plist                # 应用元数据（标识符、版本、图标等）
    ├── embedded.provisionprofile # 签名配置文件（App Store 分发时需要）
    ├── MacOS/                   # 可执行文件和 DLL
    │   ├── ProjectHub.Desktop           # 主可执行文件（无扩展名）
    │   ├── ProjectHub.Desktop.dll       # 主程序集
    │   ├── ProjectHub.Core.dll
    │   ├── Avalonia.dll
    │   ├── Avalonia.Desktop.dll
    │   ├── Semi.Avalonia.dll
    │   └── ... (其他依赖 DLL)
    └── Resources/
        └── ProjectHub.icns      # 应用图标
```

---

## 2. 前置准备

### 2.1 csproj 配置

确认 `ProjectHub.Desktop.csproj` 中包含以下配置，确保 macOS 发布时生成 AppHost 可执行文件：

```xml
<PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <UseAppHost>true</UseAppHost>
    <RuntimeIdentifiers>osx-x64;osx-arm64</RuntimeIdentifiers>
</PropertyGroup>
```

> **重要**：`UseAppHost` 必须为 `true`，否则 `dotnet publish` 不会生成 macOS 所需的无扩展名主执行文件（即 `ProjectHub.Desktop`），`.app` Bundle 将无法启动。

### 2.2 图标文件准备

macOS 要求 `.icns` 格式的图标文件。当前项目中的 `Assets/app-icon.ico` 是 Windows 格式，需要转换。

#### 方式一：使用在线工具

将 `app-icon.ico` 上传至 [iConvert Icons](https://iconverticons.com/online/) 或 [ConvertIO](https://convertio.co/) 转换为 `.icns` 格式。

#### 方式二：使用 macOS 命令行

```bash
# 在 macOS 上，将 PNG 转换为 icns
# 先准备不同尺寸的 PNG（16, 32, 64, 128, 256, 512, 1024）
# 然后使用 iconutil

mkdir ProjectHub.iconset
sips -z 16 16     app-icon.png --out ProjectHub.iconset/icon_16x16.png
sips -z 32 32     app-icon.png --out ProjectHub.iconset/icon_16x16@2x.png
sips -z 32 32     app-icon.png --out ProjectHub.iconset/icon_32x32.png
sips -z 64 64     app-icon.png --out ProjectHub.iconset/icon_32x32@2x.png
sips -z 128 128   app-icon.png --out ProjectHub.iconset/icon_128x128.png
sips -z 256 256   app-icon.png --out ProjectHub.iconset/icon_128x128@2x.png
sips -z 256 256   app-icon.png --out ProjectHub.iconset/icon_256x256.png
sips -z 512 512   app-icon.png --out ProjectHub.iconset/icon_256x256@2x.png
sips -z 512 512   app-icon.png --out ProjectHub.iconset/icon_512x512.png
sips -z 1024 1024 app-icon.png --out ProjectHub.iconset/icon_512x512@2x.png
iconutil -c icns ProjectHub.iconset -o ProjectHub.icns
```

#### 方式三：使用 Linux

参考 [Creating macOS Icons (icns) on Linux](https://dentrassi.de/2014/02/25/creating-mac-os-x-icons-icns-on-linux/)，在 Linux 环境下使用 `png2icns` 等工具生成。

### 2.3 .NET SDK 要求

- .NET 8 SDK（与 `TargetFramework` 匹配）

---

## 3. 发布应用

### 3.1 标准发布

```bash
# 发布到 macOS x64（Intel 芯片）
dotnet publish ProjectHub.Desktop/ProjectHub.Desktop.csproj \
    -r osx-x64 \
    --configuration Release \
    -p:UseAppHost=true

# 发布到 macOS arm64（Apple Silicon 芯片）
dotnet publish ProjectHub.Desktop/ProjectHub.Desktop.csproj \
    -r osx-arm64 \
    --configuration Release \
    -p:UseAppHost=true
```

发布输出路径为：

```
ProjectHub.Desktop/bin/Release/net8.0/osx-x64/publish/
```

### 3.2 单文件发布（推荐）

单文件发布将大部分 DLL 合并到主执行文件中，简化签名和公证流程：

```bash
dotnet publish ProjectHub.Desktop/ProjectHub.Desktop.csproj \
    -r osx-arm64 \
    --configuration Release \
    -p:UseAppHost=true \
    -p:PublishSingleFile=true
```

> **注意**：单文件发布要求 .NET 8 运行时在目标机器上可用，或者在 csproj 中配置 `SelfContained`。

### 3.3 自包含发布

自包含发布会将 .NET 运行时一起打包，用户无需安装 .NET：

```bash
dotnet publish ProjectHub.Desktop/ProjectHub.Desktop.csproj \
    -r osx-arm64 \
    --configuration Release \
    --self-contained \
    -p:UseAppHost=true \
    -p:PublishSingleFile=true
```

### 3.4 验证发布产物

发布后检查 `publish/` 目录，确保同时存在以下两个文件：

| 文件 | 说明 |
|------|------|
| `ProjectHub.Desktop` | 无扩展名的可执行文件（AppHost） |
| `ProjectHub.Desktop.dll` | 主程序集 DLL |

如果缺少无扩展名的可执行文件，说明 `UseAppHost` 未生效，请检查 csproj 配置。

---

## 4. 创建 Info.plist

`Info.plist` 是 `.app` Bundle 的核心配置文件，macOS 通过它获取应用的身份、版本、图标等信息。

### 4.1 Info.plist 模板

创建 `Info.plist` 文件，内容如下：

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>ProjectHub.Desktop</string>
    <key>CFBundleName</key>
    <string>ProjectHub</string>
    <key>CFBundleDisplayName</key>
    <string>ProjectHub</string>
    <key>CFBundleIdentifier</key>
    <string>com.projecthub.macos</string>
    <key>CFBundleIconFile</key>
    <string>ProjectHub.icns</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
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
```

### 4.2 关键字段说明

| Key | 值 | 说明 |
|-----|-----|------|
| `CFBundleExecutable` | `ProjectHub.Desktop` | 必须与 `dotnet publish` 输出的可执行文件名（无 `.dll` 扩展名）完全一致 |
| `CFBundleName` | `ProjectHub` | 应用显示名称（菜单栏等处，建议不超过 15 个字符） |
| `CFBundleDisplayName` | `ProjectHub` | Finder 和 Launchpad 中显示的完整名称 |
| `CFBundleIdentifier` | `com.projecthub.macos` | 唯一标识符，采用反向 DNS 格式 |
| `CFBundleIconFile` | `ProjectHub.icns` | 图标文件名，需包含 `.icns` 扩展名 |
| `CFBundleVersion` | `1.0.0` | 内部构建版本号 |
| `CFBundleShortVersionString` | `1.0.0` | 用户可见的版本号 |
| `LSMinimumSystemVersion` | `12.0` | 最低支持的 macOS 版本（macOS Monterey） |
| `NSHighResolutionCapable` | `true` | 启用 Retina 高分辨率显示支持 |
| `LSApplicationCategoryType` | `public.app-category.developer-tools` | App Store 分类（开发工具类） |

> **注意**：`CFBundleVersion` 和 `CFBundleShortVersionString` 应与项目实际版本号保持一致，后续可通过 CI/CD 流水线自动替换。

---

## 5. 创建 .app Bundle

### 5.1 手动创建目录结构

按照以下步骤创建 `.app` 目录结构：

```bash
# 定义变量
APP_NAME="ProjectHub.app"
PUBLISH_DIR="ProjectHub.Desktop/bin/Release/net8.0/osx-arm64/publish"
BUILD_DIR="build/mac"

# 清理旧的构建产物
rm -rf "$BUILD_DIR/$APP_NAME"

# 创建目录结构
mkdir -p "$BUILD_DIR/$APP_NAME/Contents/MacOS"
mkdir -p "$BUILD_DIR/$APP_NAME/Contents/Resources"

# 复制 Info.plist
cp Info.plist "$BUILD_DIR/$APP_NAME/Contents/"

# 复制图标
cp ProjectHub.icns "$BUILD_DIR/$APP_NAME/Contents/Resources/"

# 复制发布产物
cp -a "$PUBLISH_DIR/"* "$BUILD_DIR/$APP_NAME/Contents/MacOS/"
```

### 5.2 自动化打包脚本

创建 `scripts/package-macos.sh` 脚本（需要在 macOS 或 Linux/WSL 上执行）：

```bash
#!/bin/bash
set -e

APP_NAME="ProjectHub.app"
BUNDLE_ID="com.projecthub.macos"
VERSION="1.0.0"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
BUILD_DIR="$PROJECT_DIR/build/mac"
PUBLISH_DIR="$PROJECT_DIR/ProjectHub.Desktop/bin/Release/net8.0/osx-arm64/publish"
INFO_PLIST="$PROJECT_DIR/build/mac/Info.plist"
ICON_FILE="$PROJECT_DIR/build/mac/ProjectHub.icns"

echo "=== ProjectHub macOS 打包脚本 ==="
echo ""

# Step 1: 清理
echo "[1/5] 清理旧的构建产物..."
rm -rf "$BUILD_DIR/$APP_NAME"
mkdir -p "$BUILD_DIR"

# Step 2: 创建目录结构
echo "[2/5] 创建 .app Bundle 目录结构..."
mkdir -p "$BUILD_DIR/$APP_NAME/Contents/MacOS"
mkdir -p "$BUILD_DIR/$APP_NAME/Contents/Resources"

# Step 3: 复制文件
echo "[3/5] 复制 Info.plist 和图标..."
cp "$INFO_PLIST" "$BUILD_DIR/$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$BUILD_DIR/$APP_NAME/Contents/Resources/ProjectHub.icns"

echo "[3/5] 复制发布产物..."
cp -a "$PUBLISH_DIR/"* "$BUILD_DIR/$APP_NAME/Contents/MacOS/"

# Step 4: 设置可执行文件权限
echo "[4/5] 设置可执行文件权限..."
chmod +x "$BUILD_DIR/$APP_NAME/Contents/MacOS/ProjectHub.Desktop"

# Step 5: 验证
echo "[5/5] 验证 .app Bundle 结构..."
if [ -f "$BUILD_DIR/$APP_NAME/Contents/MacOS/ProjectHub.Desktop" ] && \
   [ -f "$BUILD_DIR/$APP_NAME/Contents/MacOS/ProjectHub.Desktop.dll" ] && \
   [ -f "$BUILD_DIR/$APP_NAME/Contents/Info.plist" ] && \
   [ -f "$BUILD_DIR/$APP_NAME/Contents/Resources/ProjectHub.icns" ]; then
    echo ""
    echo "=== 打包成功 ==="
    echo "输出路径: $BUILD_DIR/$APP_NAME"
    echo ""
    echo "文件结构:"
    find "$BUILD_DIR/$APP_NAME" -type f | sort
else
    echo ""
    echo "=== 打包失败：缺少必要文件 ==="
    exit 1
fi
```

### 5.3 跨平台创建 .app（含 Windows）

`.app` 目录结构本质是文件夹，在任何操作系统上都可以创建。但需要注意：

1. 在 **Windows** 上创建的可执行文件缺少 Unix 可执行权限标志
2. 必须在 macOS 或 Linux 上运行 `chmod +x` 修正权限：

```bash
chmod +x ProjectHub.app/Contents/MacOS/ProjectHub.Desktop
```

---

## 6. 代码签名

代码签名是 macOS 安全机制的一部分，是公证（Notarization）的前提条件。签名后 macOS 可以验证应用来源和完整性。

> **注意**：代码签名必须在 macOS 上完成，需要安装 Xcode 命令行工具。

### 6.1 前置条件

- macOS 计算机
- Apple Developer 账号（付费订阅，用于公证和 App Store 分发）
- Xcode 命令行工具：`xcode-select --install`
- Developer ID 证书（在 Keychain Access 中可见）

### 6.2 创建 Entitlements 文件

Hardened Runtime 是 macOS 安全策略，限制应用运行时行为。Avalonia 应用需要以下例外权限：

创建 `ProjectHub.entitlements` 文件：

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.cs.allow-jit</key>
    <true/>
    <key>com.apple.security.automation.apple-events</key>
    <true/>
</dict>
</plist>
```

| 权限 | 说明 |
|------|------|
| `com.apple.security.cs.allow-jit` | **必须**。允许 JIT 编译代码，Avalonia 在 Hardened Runtime 下运行必须启用此权限 |
| `com.apple.security.automation.apple-events` | 允许发送 Apple Events，修复 Console.app 中的报错 |

> **安全提示**：Microsoft 文档列出的其他 Hardened Runtime 例外权限并非 Avalonia 应用必需，添加不必要的例外会增加安全风险。

### 6.3 执行代码签名

签名脚本：

```bash
#!/bin/bash
set -e

APP_NAME="build/mac/ProjectHub.app"
ENTITLEMENTS="build/mac/ProjectHub.entitlements"
SIGNING_IDENTITY="Developer ID Application: YourCompanyName (TEAM_ID)"

echo "=== ProjectHub 代码签名 ==="

# 签名 Contents/MacOS 下的所有文件
find "$APP_NAME/Contents/MacOS/" -type f | while read -r fname; do
    echo "[INFO] 签名: $fname"
    codesign --force \
             --timestamp \
             --options=runtime \
             --entitlements "$ENTITLEMENTS" \
             --sign "$SIGNING_IDENTITY" \
             "$fname"
done

# 签名 .app Bundle 本身
echo "[INFO] 签名: $APP_NAME"
codesign --force \
         --timestamp \
         --options=runtime \
         --entitlements "$ENTITLEMENTS" \
         --sign "$SIGNING_IDENTITY" \
         "$APP_NAME"

echo ""
echo "=== 验证签名 ==="
codesign --verify --verbose "$APP_NAME"
echo ""
echo "=== 签名完成 ==="
```

### 6.4 验证签名

```bash
codesign --verify --verbose ProjectHub.app
```

如果签名正确，输出应包含 `valid on disk` 等验证通过信息，无报错。

---

## 7. 公证（Notarization）

公证是将签名后的应用提交 Apple 服务器进行安全扫描的过程。macOS 10.15 (Catalina) 及更高版本要求所有 App Store 之外分发的应用必须经过公证，否则用户打开时会遇到"无法打开，因为无法验证开发者"的警告。

### 7.1 公证流程

```bash
# Step 1: 将 .app 打包为 .zip（必须使用 ditto，不要使用 zip 命令）
ditto -c -k --sequesterRsrc --keepParent ProjectHub.app ProjectHub.zip

# Step 2: 提交公证
xcrun notarytool submit ProjectHub.zip \
    --apple-id "your-apple-id@example.com" \
    --team-id "YOUR_TEAM_ID" \
    --password "app-specific-password" \
    --wait

# Step 3: 公证成功后，将公证票据钉到 .app
xcrun stapler staple ProjectHub.app

# Step 4: 验证公证
xcrun stapler validate ProjectHub.app
```

> **提示**：可以在 Keychain 中存储密码以避免每次输入：`--password "@keychain:AC_PASSWORD"`，其中 `AC_PASSWORD` 是 Keychain 中的项目名称。

### 7.2 使用 `altool`（旧版方式）

如果 `notarytool` 不可用，可以使用旧版 `altool`：

```bash
# 提交公证
xcrun altool --notarize-app \
    -f ProjectHub.zip \
    --primary-bundle-id com.projecthub.macos \
    -u "your-apple-id@example.com" \
    -p "app-specific-password"

# 查询公证状态（使用返回的 UUID）
xcrun altool --notarization-info <UUID> \
    -u "your-apple-id@example.com" \
    -p "app-specific-password"
```

### 7.3 DMG 分发的公证

如果通过 `.dmg` 文件分发：

```bash
# Step 1: 先对 .app 进行公证和 staple
# （执行上述 7.1 中的 Step 1-4）

# Step 2: 将已公证的 .app 放入 DMG
# （见第 8 节 DMG 创建）

# Step 3: 对 DMG 进行公证
ditto -c -k --sequesterRsrc --keepParent ProjectHub.dmg ProjectHub-dmg.zip
xcrun notarytool submit ProjectHub-dmg.zip \
    --apple-id "your-apple-id@example.com" \
    --team-id "YOUR_TEAM_ID" \
    --password "app-specific-password" \
    --wait

# Step 4: 对 DMG 进行 staple
xcrun stapler staple ProjectHub.dmg
```

---

## 8. 创建 DMG 安装包

DMG（Disk Image）是 macOS 最常见的分发格式，用户双击即可打开并将应用拖入 Applications 文件夹。

### 8.1 创建 DMG

```bash
#!/bin/bash
set -e

APP_NAME="ProjectHub"
DMG_NAME="ProjectHub-1.0.0-macos-arm64.dmg"
VOLUME_NAME="ProjectHub"
BUILD_DIR="build/mac"
TEMP_DIR="$BUILD_DIR/dmg-staging"

echo "=== 创建 DMG 安装包 ==="

# 清理
rm -rf "$TEMP_DIR"
rm -f "$BUILD_DIR/$DMG_NAME"

# 创建临时目录
mkdir -p "$TEMP_DIR"

# 复制 .app
cp -a "$BUILD_DIR/$APP_NAME.app" "$TEMP_DIR/"

# 创建 Applications 软链接（方便用户拖拽安装）
ln -s /Applications "$TEMP_DIR/Applications"

# 创建 DMG
hdiutil create -volname "$VOLUME_NAME" \
    -srcfolder "$TEMP_DIR" \
    -ov \
    -format UDZO \
    "$BUILD_DIR/$DMG_NAME"

# 清理
rm -rf "$TEMP_DIR"

echo "=== DMG 创建完成 ==="
echo "输出路径: $BUILD_DIR/$DMG_NAME"
```

### 8.2 DMG 自定义外观（可选）

使用脚本自动打开 DMG 并设置窗口样式（需要在 macOS 上使用 Finder 脚本或 AppleScript）：

```bash
hdiutil attach "$BUILD_DIR/$DMG_NAME"

# 设置 Finder 窗口外观
osascript <<EOF
tell application "Finder"
    tell disk "$VOLUME_NAME"
        open
        set current view of container window to icon view
        set toolbar visible of container window to false
        set statusbar visible of container window to false
        set the bounds of container window to {100, 100, 640, 400}
        set viewOptions to the icon view options of container window
        set arrangement of viewOptions to not arranged
        set icon size of viewOptions to 80
        set position of item "$APP_NAME.app" of container window to {130, 200}
        set position of item "Applications" of container window to {410, 200}
        close
        open
        update without registering applications
        delay 2
        close
    end tell
end tell
EOF

hdiutil detach "/Volumes/$VOLUME_NAME"
```

---

## 9. csproj 完整修改汇总

以下是 `ProjectHub.Desktop.csproj` 需要添加的 macOS 打包相关配置：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <AvaloniaUseCompiledBindingsByDefault>false</AvaloniaUseCompiledBindingsByDefault>

    <!-- macOS 打包配置 -->
    <UseAppHost>true</UseAppHost>
    <RuntimeIdentifiers>win-x64;osx-x64;osx-arm64;linux-x64</RuntimeIdentifiers>
  </PropertyGroup>

  <!-- ... 其余配置保持不变 ... -->
</Project>
```

---

## 10. 完整打包流程总结

以下是在 Windows 开发环境下完成 macOS 打包的完整步骤：

### 10.1 在 Windows 上准备

```bash
# Step 1: 修改 csproj 添加 UseAppHost 和 RuntimeIdentifiers
# Step 2: 发布应用
dotnet publish ProjectHub.Desktop/ProjectHub.Desktop.csproj \
    -r osx-arm64 \
    --configuration Release \
    -p:UseAppHost=true \
    -p:PublishSingleFile=true

# Step 3: 创建 .app 目录结构（可以在 Windows 上操作文件夹）
```

### 10.2 在 macOS 上完成

```bash
# Step 4: 修正可执行文件权限
chmod +x ProjectHub.app/Contents/MacOS/ProjectHub.Desktop

# Step 5: 代码签名
# （执行第 6 节的签名脚本）

# Step 6: 公证
# （执行第 7 节的公证流程）

# Step 7: 创建 DMG（可选）
# （执行第 8 节的 DMG 创建脚本）
```

### 10.3 完全在 macOS 上完成（使用自动化脚本）

项目提供了两个自动化打包脚本，覆盖从发布到打包的完整流程。

#### build-macos.sh（macOS/Linux 上运行，功能完整）

支持发布、打包、签名、公证、创建 DMG 的一站式脚本：

```bash
# 基础构建（仅发布 + 创建 .app Bundle）
bash scripts/build-macos.sh

# 发布到 Intel 芯片
bash scripts/build-macos.sh -r osx-x64

# 同时发布 Intel 和 Apple Silicon
bash scripts/build-macos.sh -r osx-x64,osx-arm64

# 指定版本号
bash scripts/build-macos.sh -v 1.2.0

# 完整发布流水线（签名 + 公证 + DMG）
bash scripts/build-macos.sh \
    --sign \
    --signing-identity "Developer ID Application: YourName (TEAM_ID)" \
    --notarize \
    --apple-id "your@email.com" \
    --team-id "TEAM_ID" \
    --apple-password "app-specific-password" \
    --dmg

# 框架依赖发布（体积更小，需要用户安装 .NET 8）
bash scripts/build-macos.sh --no-self-contained

# 查看所有选项
bash scripts/build-macos.sh --help
```

#### build-macos.ps1（Windows 上运行，跨平台编译）

在 Windows 下交叉编译 macOS 版本，创建 .app Bundle 目录结构：

```powershell
# 基础构建（osx-arm64）
.\scripts\build-macos.ps1

# 发布到 Intel 芯片
.\scripts\build-macos.ps1 -Runtime osx-x64

# 同时发布两个架构
.\scripts\build-macos.ps1 -Runtime osx-x64,osx-arm64

# 指定版本号
.\scripts\build-macos.ps1 -Version "1.2.0"
```

> **注意**：`build-macos.ps1` 只能完成发布和创建 Bundle 结构。代码签名、公证和 DMG 创建必须在 macOS 上完成。脚本运行后会在 `.app` 目录中生成 `README_SIGNING.txt`，包含后续步骤说明。

---

## 11. 常见问题

### Q1: 发布后缺少无扩展名的可执行文件

**现象**：`publish/` 目录下只有 `ProjectHub.Desktop.dll`，没有 `ProjectHub.Desktop`（无扩展名）。

**原因**：`UseAppHost` 未启用。

**解决**：
- 在 csproj 中添加 `<UseAppHost>true</UseAppHost>`
- 或在 `dotnet publish` 命令中添加 `-p:UseAppHost=true`

### Q2: 资源文件报错 `doesn't have a target for osx-64`

**现象**：构建时报错找不到 `osx-64` 的 target。

**解决**：在 csproj 的 `<PropertyGroup>` 中添加：

```xml
<RuntimeIdentifiers>osx-x64;osx-arm64</RuntimeIdentifiers>
```

### Q3: 在 Windows 上创建的 .app 无法在 macOS 上启动

**原因**：Windows 文件系统不支持 Unix 可执行权限标志。

**解决**：在 macOS 或 Linux 上运行：

```bash
chmod +x ProjectHub.app/Contents/MacOS/ProjectHub.Desktop
```

### Q4: 双击 .app 提示"无法打开，因为无法验证开发者"

**原因**：应用未经过公证。

**解决**：执行第 7 节的公证流程，或在 macOS 系统设置中临时允许打开（不推荐用于分发）。

### Q5: 公证提交失败，提示 "The archive is invalid"

**原因**：使用了 `zip` 命令打包 `.app`。

**解决**：使用 `ditto` 替代：

```bash
ditto -c -k --sequesterRsrc --keepParent ProjectHub.app ProjectHub.zip
```

### Q6: 单文件发布后应用体积很大

**原因**：自包含发布会打包完整的 .NET 运行时。

**解决**：使用框架依赖发布（去掉 `--self-contained`），要求用户安装 .NET 8 运行时：

```bash
dotnet publish -r osx-arm64 --configuration Release \
    -p:UseAppHost=true -p:PublishSingleFile=true
```

---

## 12. 版本更新与自动化

### 12.1 版本号管理

在 CI/CD 流水线中，可以通过以下方式自动更新版本号：

```bash
# 通过命令行参数设置版本
dotnet publish -r osx-arm64 --configuration Release \
    -p:UseAppHost=true \
    -p:Version=1.2.0 \
    -p:AssemblyVersion=1.2.0 \
    -p:FileVersion=1.2.0
```

同时需要更新 `Info.plist` 中的 `CFBundleVersion` 和 `CFBundleShortVersionString`。

### 12.2 CI/CD 集成建议

在 GitHub Actions 或 Azure DevOps 中添加 macOS 打包 Job：

```yaml
# GitHub Actions 示例
macos-build:
  runs-on: macos-latest
  steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    - name: Publish
      run: |
        dotnet publish ProjectHub.Desktop/ProjectHub.Desktop.csproj \
          -r osx-arm64 \
          --configuration Release \
          -p:UseAppHost=true \
          -p:PublishSingleFile=true \
          -p:Version=${{ github.ref_name }}
    - name: Package
      run: bash scripts/package-macos.sh
    - name: Sign
      run: bash scripts/sign-macos.sh
      env:
        SIGNING_IDENTITY: ${{ secrets.MACOS_SIGNING_IDENTITY }}
        CERTIFICATE_PASSWORD: ${{ secrets.MACOS_CERTIFICATE_PASSWORD }}
    - name: Notarize
      run: |
        ditto -c -k --sequesterRsrc --keepParent build/mac/ProjectHub.app ProjectHub.zip
        xcrun notarytool submit ProjectHub.zip \
          --apple-id ${{ secrets.APPLE_ID }} \
          --team-id ${{ secrets.TEAM_ID }} \
          --password ${{ secrets.APPLE_APP_PASSWORD }} \
          --wait
        xcrun stapler staple build/mac/ProjectHub.app
    - name: Create DMG
      run: bash scripts/create-dmg.sh
```

---

## 13. 参考链接

- [AvaloniaUI macOS 部署文档](https://docs.avaloniaui.net/docs/deployment/macos)
- [AvaloniaUI Parcel 打包工具](https://docs.avaloniaui.net/tools/parcel/packaging-for-macos)
- [.NET macOS 公证问题](https://docs.microsoft.com/en-us/dotnet/core/install/macos-notarization-issues)
- [Apple Information Property List 参考](https://developer.apple.com/documentation/bundleresources/information_property_list/bundle_configuration)
- [Apple 公证文档](https://developer.apple.com/documentation/xcode/notarizing-your-app-before-distribution)
- [.NET Runtime Identifiers (RID)](https://learn.microsoft.com/dotnet/core/rid-catalog)
