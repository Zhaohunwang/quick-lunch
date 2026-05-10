#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
PROJECT_FILE="$PROJECT_DIR/ProjectHub.Desktop/ProjectHub.Desktop.csproj"
BUILD_ROOT="$PROJECT_DIR/build/mac"

APP_NAME="ProjectHub"
BUNDLE_ID="com.projecthub.macos"
EXE_NAME="ProjectHub.Desktop"
ICON_NAME="ProjectHub.icns"

RUNTIME="osx-arm64"
CONFIGURATION="Release"
SELF_CONTAINED=true
SINGLE_FILE=true
VERSION=""
CLEAN=true
SIGN_APP=false
NOTARIZE=false
CREATE_DMG=false

SIGNING_IDENTITY=""
APPLE_ID=""
TEAM_ID=""
APPLE_PASSWORD=""

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
NC='\033[0m'

usage() {
    echo ""
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  -r, --runtime RUNTIME      Target runtime (default: osx-arm64)"
    echo "                              Options: osx-x64, osx-arm64"
    echo "  -v, --version VERSION      App version (default: read from csproj)"
    echo "  -c, --config CONFIG        Build configuration (default: Release)"
    echo "  --no-self-contained        Framework-dependent publish"
    echo "  --no-single-file           Multi-file publish"
    echo "  --no-clean                 Skip cleaning before build"
    echo "  --sign                     Code sign the app"
    echo "  --notarize                 Notarize the signed app"
    echo "  --dmg                      Create DMG installer"
    echo "  --signing-identity ID      Code signing identity"
    echo "  --apple-id ID              Apple ID for notarization"
    echo "  --team-id ID               Team ID for notarization"
    echo "  --apple-password PWD       App-specific password for notarization"
    echo "  -h, --help                 Show this help"
    echo ""
    echo "Examples:"
    echo "  $0                                          # Basic build (osx-arm64)"
    echo "  $0 -r osx-x64                               # Build for Intel"
    echo "  $0 -r osx-x64,osx-arm64 --dmg               # Build both + DMG"
    echo "  $0 --sign --notarize --dmg                   # Full release pipeline"
    echo "  $0 --sign --signing-identity \"Developer ID Application: MyCompany\""
    echo ""
}

while [[ $# -gt 0 ]]; do
    case $1 in
        -r|--runtime) RUNTIME="$2"; shift 2 ;;
        -v|--version) VERSION="$2"; shift 2 ;;
        -c|--config) CONFIGURATION="$2"; shift 2 ;;
        --no-self-contained) SELF_CONTAINED=false; shift ;;
        --no-single-file) SINGLE_FILE=false; shift ;;
        --no-clean) CLEAN=false; shift ;;
        --sign) SIGN_APP=true; shift ;;
        --notarize) NOTARIZE=true; shift ;;
        --dmg) CREATE_DMG=true; shift ;;
        --signing-identity) SIGNING_IDENTITY="$2"; shift 2 ;;
        --apple-id) APPLE_ID="$2"; shift 2 ;;
        --team-id) TEAM_ID="$2"; shift 2 ;;
        --apple-password) APPLE_PASSWORD="$2"; shift 2 ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Unknown option: $1"; usage; exit 1 ;;
    esac
done

print_step() {
    echo ""
    printf "${CYAN}=== %s ===${NC}\n" "$1"
}

print_info() {
    printf "[INFO] %s\n" "$1"
}

print_ok() {
    printf "${GREEN}[OK] %s${NC}\n" "$1"
}

print_warn() {
    printf "${YELLOW}[WARN] %s${NC}\n" "$1"
}

print_err() {
    printf "${RED}[ERROR] %s${NC}\n" "$1"
}

get_version() {
    if [[ -n "$VERSION" ]]; then
        echo "$VERSION"
        return
    fi

    local ver
    ver=$(sed -n 's/.*<Version>\([^<]*\)<\/Version>.*/\1/p' "$PROJECT_FILE" 2>/dev/null | head -1)
    if [[ -n "$ver" ]]; then
        echo "$ver"
        return
    fi

    ver=$(sed -n 's/.*<AssemblyVersion>\([^<]*\)<\/AssemblyVersion>.*/\1/p' "$PROJECT_FILE" 2>/dev/null | head -1)
    if [[ -n "$ver" ]]; then
        echo "$ver"
        return
    fi

    echo "1.0.0"
}

check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        print_err ".NET SDK not found. Please install .NET 8 SDK."
        print_err "Download: https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    fi
    local ver
    ver=$(dotnet --version)
    print_info ".NET SDK version: $ver"
}

check_codesign() {
    if ! command -v codesign &> /dev/null; then
        print_err "codesign not found. Please install Xcode command line tools."
        print_err "Run: xcode-select --install"
        exit 1
    fi
}

check_notarytool() {
    if ! command -v xcrun &> /dev/null || ! xcrun notarytool --help &> /dev/null 2>&1; then
        print_err "notarytool not found. Please install Xcode."
        exit 1
    fi
}

clean_build() {
    if [[ "$CLEAN" != true ]]; then
        print_info "Skipping clean"
        return
    fi

    local output_dir="$BUILD_ROOT/$RUNTIME"
    if [[ -d "$output_dir" ]]; then
        print_info "Cleaning: $output_dir"
        rm -rf "$output_dir"
    fi

    local obj_dir="$PROJECT_DIR/ProjectHub.Desktop/obj"
    local bin_dir="$PROJECT_DIR/ProjectHub.Desktop/bin"
    for dir in "$obj_dir" "$bin_dir"; do
        if [[ -d "$dir" ]]; then
            print_info "Cleaning: $dir"
            rm -rf "$dir"
        fi
    done
}

publish_app() {
    local app_version="$1"
    local publish_dir="$BUILD_ROOT/$RUNTIME/publish"

    local publish_args=(
        "publish"
        "$PROJECT_FILE"
        "-r" "$RUNTIME"
        "-c" "$CONFIGURATION"
        "-o" "$publish_dir"
        "-p:UseAppHost=true"
        "-p:Version=$app_version"
        "-p:AssemblyVersion=$app_version"
        "-p:FileVersion=$app_version"
    )

    if [[ "$SELF_CONTAINED" == true ]]; then
        publish_args+=("--self-contained")
    fi

    if [[ "$SINGLE_FILE" == true ]]; then
        publish_args+=("-p:PublishSingleFile=true")
    fi

    print_info "Command: dotnet ${publish_args[*]}"
    echo ""

    dotnet "${publish_args[@]}"

    echo "$publish_dir"
}

verify_publish() {
    local publish_dir="$1"

    local exe_path="$publish_dir/$EXE_NAME"
    local dll_path="$publish_dir/$EXE_NAME.dll"

    if [[ ! -f "$exe_path" ]]; then
        print_err "Missing executable: $exe_path"
        return 1
    fi

    if [[ "$SINGLE_FILE" == true ]]; then
        print_ok "Executable: $exe_path"
    else
        if [[ ! -f "$dll_path" ]]; then
            print_err "Missing DLL: $dll_path"
            return 1
        fi
        print_ok "Executable: $exe_path"
        print_ok "Assembly:   $dll_path"
    fi

    local file_count
    file_count=$(find "$publish_dir" -maxdepth 1 -type f | wc -l | tr -d ' ')
    print_info "Total files: $file_count"

    local total_size
    total_size=$(du -sh "$publish_dir" | cut -f1)
    print_info "Total size: $total_size"
}

convert_ico_to_icns() {
    local ico_path="$1"
    local output_icns="$2"

    if ! command -v sips &> /dev/null || ! command -v iconutil &> /dev/null; then
        print_warn "sips or iconutil not available, cannot convert .ico to .icns"
        return 1
    fi

    local iconset_dir="$BUILD_ROOT/iconset_temp"
    rm -rf "$iconset_dir"
    mkdir -p "$iconset_dir"

    local base_png="$iconset_dir/base.png"
    if ! sips -s format png "$ico_path" --out "$base_png" &> /dev/null; then
        print_err "Failed to convert .ico to PNG using sips"
        rm -rf "$iconset_dir"
        return 1
    fi

    local names=("icon_16x16.png" "icon_16x16@2x.png" "icon_32x32.png" "icon_32x32@2x.png" "icon_128x128.png" "icon_128x128@2x.png" "icon_256x256.png" "icon_256x256@2x.png" "icon_512x512.png" "icon_512x512@2x.png")
    local icon_sizes=("16 16" "32 32" "32 32" "64 64" "128 128" "256 256" "256 256" "512 512" "512 512" "1024 1024")

    for i in "${!names[@]}"; do
        read -r w h <<< "${icon_sizes[$i]}"
        sips -z "$h" "$w" "$base_png" --out "$iconset_dir/${names[$i]}" &> /dev/null
    done

    if iconutil -c icns "$iconset_dir" -o "$output_icns" 2>&1; then
        print_ok "Converted .ico to .icns: $output_icns"
        rm -rf "$iconset_dir"
        return 0
    else
        print_err "iconutil failed to create .icns"
        rm -rf "$iconset_dir"
        return 1
    fi
}

get_icon_path() {
    local icon_candidates=(
        "$BUILD_ROOT/$ICON_NAME"
        "$PROJECT_DIR/build/$ICON_NAME"
        "$PROJECT_DIR/$ICON_NAME"
        "$PROJECT_DIR/ProjectHub.Desktop/Assets/Icons/$ICON_NAME"
        "$PROJECT_DIR/ProjectHub.Desktop/Assets/app-icon.icns"
    )

    for icon in "${icon_candidates[@]}"; do
        if [[ -f "$icon" ]]; then
            echo "$icon"
            return
        fi
    done

    local ico_candidates=(
        "$BUILD_ROOT/app-icon.ico"
        "$PROJECT_DIR/build/app-icon.ico"
        "$PROJECT_DIR/ProjectHub.Desktop/Assets/app-icon.ico"
    )

    for ico in "${ico_candidates[@]}"; do
        if [[ -f "$ico" ]]; then
            print_info "Found .ico file: $ico"
            local converted="$BUILD_ROOT/$ICON_NAME"
            if convert_ico_to_icns "$ico" "$converted"; then
                echo "$converted"
                return
            fi
        fi
    done

    print_warn "No .icns icon file found. The app will have a default icon."
    print_warn "Place $ICON_NAME in $BUILD_ROOT/ or convert from PNG:"
    print_warn "  See: https://docs.avaloniaui.net/docs/deployment/macos#creating-icon-files"
    echo ""
}

create_bundle() {
    local publish_dir="$1"
    local app_version="$2"

    local bundle_dir="$BUILD_ROOT/$RUNTIME/$APP_NAME.app"
    local icon_path
    icon_path=$(get_icon_path)

    if [[ -d "$bundle_dir" ]]; then
        print_info "Removing existing bundle: $bundle_dir"
        rm -rf "$bundle_dir"
    fi

    print_info "Creating bundle: $bundle_dir"
    mkdir -p "$bundle_dir/Contents/MacOS"
    mkdir -p "$bundle_dir/Contents/Resources"

    generate_info_plist "$bundle_dir/Contents/Info.plist" "$app_version"

    if [[ -n "$icon_path" && -f "$icon_path" ]]; then
        cp "$icon_path" "$bundle_dir/Contents/Resources/$ICON_NAME"
        print_ok "Icon: $icon_path"
    fi

    cp -a "$publish_dir/"* "$bundle_dir/Contents/MacOS/"

    chmod +x "$bundle_dir/Contents/MacOS/$EXE_NAME"

    if [[ -f "$bundle_dir/Contents/MacOS/$EXE_NAME" && -f "$bundle_dir/Contents/Info.plist" ]]; then
        print_ok "Bundle created: $bundle_dir"
    else
        print_err "Bundle creation failed: missing required files"
        return 1
    fi

    echo "$bundle_dir"
}

generate_info_plist() {
    local plist_path="$1"
    local app_version="$2"

    cat > "$plist_path" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$EXE_NAME</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleIconFile</key>
    <string>$ICON_NAME</string>
    <key>CFBundleVersion</key>
    <string>$app_version</string>
    <key>CFBundleShortVersionString</key>
    <string>$app_version</string>
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
PLIST
    print_ok "Info.plist generated (version: $app_version)"
}

create_entitlements() {
    local entitlements_path="$BUILD_ROOT/$RUNTIME/ProjectHub.entitlements"

    cat > "$entitlements_path" << PLIST
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
PLIST

    echo "$entitlements_path"
}

sign_app() {
    local bundle_dir="$1"
    local app_version="$2"

    check_codesign

    if [[ -z "$SIGNING_IDENTITY" ]]; then
        print_err "Signing identity not specified. Use --signing-identity."
        exit 1
    fi

    local entitlements
    entitlements=$(create_entitlements)

    print_info "Signing Contents/MacOS/ files..."
    find "$bundle_dir/Contents/MacOS/" -type f | while IFS= read -r fname; do
        print_info "  Signing: $(basename "$fname")"
        codesign --force \
                 --timestamp \
                 --options=runtime \
                 --entitlements "$entitlements" \
                 --sign "$SIGNING_IDENTITY" \
                 "$fname"
    done

    print_info "Signing bundle: $bundle_dir"
    codesign --force \
             --timestamp \
             --options=runtime \
             --entitlements "$entitlements" \
             --sign "$SIGNING_IDENTITY" \
             "$bundle_dir"

    print_info "Verifying signature..."
    if codesign --verify --verbose "$bundle_dir" 2>&1; then
        print_ok "Code signing complete and verified"
    else
        print_err "Signature verification failed"
        exit 1
    fi
}

notarize_app() {
    local bundle_dir="$1"

    check_notarytool

    if [[ -z "$APPLE_ID" || -z "$TEAM_ID" || -z "$APPLE_PASSWORD" ]]; then
        print_err "Notarization requires --apple-id, --team-id, and --apple-password."
        exit 1
    fi

    local zip_path="$BUILD_ROOT/$RUNTIME/notarize.zip"

    print_info "Creating notarization archive..."
    if [[ -f "$zip_path" ]]; then
        rm -f "$zip_path"
    fi
    ditto -c -k --sequesterRsrc --keepParent "$bundle_dir" "$zip_path"

    print_info "Submitting for notarization..."
    xcrun notarytool submit "$zip_path" \
        --apple-id "$APPLE_ID" \
        --team-id "$TEAM_ID" \
        --password "$APPLE_PASSWORD" \
        --wait

    print_info "Stapling notarization ticket..."
    xcrun stapler staple "$bundle_dir"

    print_info "Validating stapled bundle..."
    xcrun stapler validate "$bundle_dir"

    rm -f "$zip_path"
    print_ok "Notarization complete"
}

create_dmg() {
    local bundle_dir="$1"
    local app_version="$2"
    local rid="$3"

    local dmg_name="$APP_NAME-$app_version-macos-${rid/osx-/}.dmg"
    local dist_dir="$BUILD_ROOT/dist"
    local staging_dir="$BUILD_ROOT/$rid/dmg-staging"

    mkdir -p "$dist_dir"
    rm -rf "$staging_dir"
    rm -f "$dist_dir/$dmg_name"

    mkdir -p "$staging_dir"
    cp -a "$bundle_dir" "$staging_dir/"
    ln -s /Applications "$staging_dir/Applications"

    print_info "Creating DMG: $dist_dir/$dmg_name"
    hdiutil create -volname "$APP_NAME" \
        -srcfolder "$staging_dir" \
        -ov \
        -format UDZO \
        "$dist_dir/$dmg_name"

    rm -rf "$staging_dir"

    if [[ -f "$dist_dir/$dmg_name" ]]; then
        local dmg_size
        dmg_size=$(du -h "$dist_dir/$dmg_name" | cut -f1)
        print_ok "DMG created: $dist_dir/$dmg_name ($dmg_size)"
    else
        print_err "DMG creation failed"
        return 1
    fi
}

show_summary() {
    local app_version="$1"
    shift
    local results=("$@")

    echo ""
    printf "${GREEN}========================================${NC}\n"
    printf "${GREEN}  ProjectHub macOS Build Complete${NC}\n"
    printf "${GREEN}========================================${NC}\n"
    echo ""
    echo "  Version:    $app_version"
    echo "  Config:     $CONFIGURATION"
    echo "  SelfContained: $SELF_CONTAINED"
    echo "  SingleFile: $SINGLE_FILE"
    echo ""

    for result in "${results[@]}"; do
        IFS='|' read -r rid bundle_path dmg_path <<< "$result"
        printf "  ${YELLOW}[%s]${NC}\n" "$rid"
        echo "    Bundle: $bundle_path"
        if [[ -n "$dmg_path" ]]; then
            echo "    DMG:    $dmg_path"
        fi
    done

    echo ""
    printf "${GREEN}========================================${NC}\n"
}

# ============================================================
# Main
# ============================================================

echo ""
printf "${CYAN}============================================${NC}\n"
printf "${CYAN}  ProjectHub macOS Build Script${NC}\n"
printf "${CYAN}============================================${NC}\n"

IFS=',' read -ra RIDS <<< "$RUNTIME"
app_version=$(get_version)

print_step "Step 1/6: Pre-flight checks"

check_dotnet

if [[ ! -f "$PROJECT_FILE" ]]; then
    print_err "Project file not found: $PROJECT_FILE"
    exit 1
fi

print_info "Application version: $app_version"
print_info "Target runtime(s): ${RIDS[*]}"
print_info "Configuration: $CONFIGURATION"
print_info "Self-contained: $SELF_CONTAINED"
print_info "Single file: $SINGLE_FILE"
print_info "Sign: $SIGN_APP"
print_info "Notarize: $NOTARIZE"
print_info "Create DMG: $CREATE_DMG"

RESULTS=()

for rid in "${RIDS[@]}"; do

    RUNTIME="$rid"

    print_step "Step 2/6: Clean & Publish ($rid)"

    clean_build

    publish_dir=$(publish_app "$app_version")
    publish_dir=$(echo "$publish_dir" | tail -n 1)

    print_step "Step 3/6: Verify publish output ($rid)"

    verify_publish "$publish_dir"

    print_step "Step 4/6: Create .app Bundle ($rid)"

    bundle_dir=$(create_bundle "$publish_dir" "$app_version")
    bundle_dir=$(echo "$bundle_dir" | tail -n 1)

    print_step "Step 5/6: Code Sign ($rid)"

    if [[ "$SIGN_APP" == true ]]; then
        sign_app "$bundle_dir" "$app_version"
    else
        print_info "Skipping code signing (use --sign to enable)"
    fi

    if [[ "$NOTARIZE" == true ]]; then
        print_step "Step 5.5/6: Notarize ($rid)"
        notarize_app "$bundle_dir"
    fi

    dmg_path=""
    if [[ "$CREATE_DMG" == true ]]; then
        print_step "Step 6/6: Create DMG ($rid)"
        create_dmg "$bundle_dir" "$app_version" "$rid"
        dmg_path="$BUILD_ROOT/dist/$APP_NAME-$app_version-macos-${rid/osx-/}.dmg"
    else
        print_step "Step 6/6: Skip DMG ($rid)"
        print_info "Use --dmg to create a DMG installer"
    fi

    RESULTS+=("$rid|$bundle_dir|$dmg_path")
done

show_summary "$app_version" "${RESULTS[@]}"
