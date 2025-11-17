# Description: Prepare the package for Yak
#
$scriptPath = $PSScriptRoot
if (!$scriptPath) {
    # Fallback for older PowerShell versions or when invoked differently
    $scriptPath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  YAK PACKAGE PREPARATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$currentFolder = $scriptPath
$targetFolder = (Get-Item $currentFolder).Parent.FullName
$binFolder = Join-Path -Path $targetFolder -ChildPath "bin"

Write-Host "[PATHS]" -ForegroundColor Green
Write-Host "  - Script Directory   : $currentFolder" -ForegroundColor Gray
Write-Host "  - Target Directory   : $targetFolder" -ForegroundColor Gray
Write-Host "  - Bin Directory      : $binFolder" -ForegroundColor Gray

#--------------------------------------
# Download Yak.exe
#--------------------------------------
Write-Host ""
Write-Host "[YAK TOOL]" -ForegroundColor Green

$yakPath = Join-Path $currentFolder "yak.exe"
if (Test-Path $yakPath) {
    Write-Host "  - Yak.exe already exists" -ForegroundColor Yellow
} else {
    Write-Host "  - Downloading Yak.exe..." -ForegroundColor Yellow
    try {
        curl.exe -fSLo yak.exe https://files.mcneel.com/yak/tools/latest/yak.exe
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  - Downloaded successfully" -ForegroundColor Green
        } else {
            Write-Host "  - Download failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "  - Error downloading Yak.exe: $_" -ForegroundColor Red
        exit 1
    }
}

#--------------------------------------
# Rhino 8, .NET 8
#--------------------------------------
Write-Host ""
Write-Host "[PACKAGE PREPARATION]" -ForegroundColor Green
Write-Host "  - Target: Rhino 8 (.NET 8)" -ForegroundColor Yellow

$releaseFolder = "releaseRH8"

if (Test-Path $releaseFolder) {
    Write-Host "  - Cleaning existing release folder..." -ForegroundColor Gray
    Remove-Item $releaseFolder -Recurse -Force
    Write-Host "  - Removed: $releaseFolder" -ForegroundColor Green
}

Write-Host "  - Creating release folder..." -ForegroundColor Gray
New-Item -Path $releaseFolder -ItemType Directory -Force | Out-Null
Write-Host "  - Created: $releaseFolder" -ForegroundColor Green

Push-Location ".\$releaseFolder"

Write-Host ""
Write-Host "[COPYING FILES]" -ForegroundColor Green

# Remove existing manifest if present
if (Test-Path "manifest.yml") {
    Remove-Item manifest.yml -Force
    Write-Host "  - Removed existing manifest.yml" -ForegroundColor Gray
}

# Copy build outputs
$releaseBinPath = Join-Path $binFolder "Release\*"
if (Test-Path (Split-Path $releaseBinPath -Parent)) {
    Copy-Item -Path $releaseBinPath -Destination "." -Recurse -ErrorAction SilentlyContinue
    $copiedFiles = (Get-ChildItem -Path "." -Recurse -File).Count
    Write-Host "  - Copied $copiedFiles file(s) from bin\Release" -ForegroundColor Green
} else {
    Write-Host "  - [WARNING] Release bin folder not found: $releaseBinPath" -ForegroundColor Red
}

# Copy plugin icon
$iconPath = Join-Path $currentFolder "pluginIcon.png"
if (Test-Path $iconPath) {
    Copy-Item -Path $iconPath -Destination "." -Force
    Write-Host "  - Copied plugin icon" -ForegroundColor Green
} else {
    Write-Host "  - [WARNING] Plugin icon not found: $iconPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "[GENERATING MANIFEST]" -ForegroundColor Green

# Generate manifest using yak spec
try {
    ./../yak.exe spec
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  - Generated manifest.yml" -ForegroundColor Green
    } else {
        Write-Host "  - [ERROR] Failed to generate manifest" -ForegroundColor Red
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "  - [ERROR] Exception during manifest generation: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}

# Append additional manifest entries
Write-Host "  - Adding metadata..." -ForegroundColor Gray
Add-Content manifest.yml "`nicon: pluginIcon.png"
Add-Content manifest.yml "`nkeywords: `n - mesh `n - geometry `n - high-performance"
Write-Host "  - Added icon and keywords" -ForegroundColor Green

Write-Host ""
Write-Host "[MANIFEST CONTENT]" -ForegroundColor Green
Write-Host "--------------------------------------" -ForegroundColor Gray
Get-Content manifest.yml | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
Write-Host "--------------------------------------" -ForegroundColor Gray

Write-Host ""
Write-Host "[BUILDING PACKAGE]" -ForegroundColor Green

# Build the Yak package
try {
    ./../yak.exe build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  - Package built successfully" -ForegroundColor Green
        
        # Display generated .yak file
        $yakFiles = Get-ChildItem -Path "." -Filter "*.yak"
        if ($yakFiles.Count -gt 0) {
            Write-Host ""
            Write-Host "[GENERATED PACKAGE]" -ForegroundColor Green
            foreach ($yakFile in $yakFiles) {
                Write-Host "  - $($yakFile.Name) ($('{0:N2}' -f ($yakFile.Length / 1MB)) MB)" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "  - [ERROR] Failed to build package" -ForegroundColor Red
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "  - [ERROR] Exception during package build: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "[NEXT STEPS]" -ForegroundColor Green
Write-Host "  - To publish: cd $releaseFolder" -ForegroundColor Gray
Write-Host "  - Then run: yak push <package-name>.yak" -ForegroundColor Gray

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PACKAGE PREPARATION COMPLETED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
