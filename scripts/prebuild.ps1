param(
    [string]$TargetDir,
    [string]$SourceDir,
    [string]$Configuration
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PRE-BUILD SCRIPT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[PARAMETERS]" -ForegroundColor Green
Write-Host "  - Target Directory : $TargetDir" -ForegroundColor Gray
Write-Host "  - Source Directory : $SourceDir" -ForegroundColor Gray
Write-Host "  - Configuration : $Configuration" -ForegroundColor Gray

# Determine the solution directory (go up from SourceDir if needed)
$solutionDir = if ($SourceDir -match "GeoSharPlusCPP") {
    # If SourceDir points to GeoSharPlusCPP\build\, get solution root
    # From C:\...\igMesh\GeoSharPlusCPP\build\ -> C:\...\igMesh\
    Split-Path (Split-Path $SourceDir -Parent) -Parent
} else {
    # Assume SourceDir is already the solution root or close to it
    $SourceDir
}

Write-Host ""
Write-Host "[SOLUTION DIRECTORY]" -ForegroundColor Green
Write-Host "  - Determined path: $solutionDir" -ForegroundColor Gray

# Define possible locations for C++ libraries
$locations = @(
    # Local build (original logic)
    @{
        dll = Join-Path (Join-Path $SourceDir $Configuration) "GeoSharPlusCPP.dll"
        dylib = $null
        pdb = Join-Path (Join-Path $SourceDir $Configuration) "GeoSharPlusCPP.pdb"
        name = "Local build directory"
    },
    # CI build location (cppPrebuild folder in solution root)
    @{
        dll = Join-Path $solutionDir "cppPrebuild\GeoSharPlusCPP.dll"
        dylib = Join-Path $solutionDir "cppPrebuild\libGeoSharPlusCPP.dylib"
        pdb = $null
        name = "CI cppPrebuild directory"
    }
)

Write-Host ""
Write-Host "[SEARCHING FOR C++ LIBRARIES]" -ForegroundColor Green

$copied = $false
$locationIndex = 1

foreach ($location in $locations) {
    Write-Host ""
    Write-Host "  Location $locationIndex : $($location.name)" -ForegroundColor Yellow
    
    # Try to copy Windows DLL
    if ($location.dll -and (Test-Path $location.dll)) {
        Write-Host "    [FOUND] GeoSharPlusCPP.dll" -ForegroundColor Green
        Write-Host "    - Path: $($location.dll)" -ForegroundColor Gray
        Copy-Item -Path $location.dll -Destination $TargetDir -Force
        Write-Host "    - Copied to: $TargetDir" -ForegroundColor Green
        $copied = $true
        
        # Copy PDB if available
        if ($location.pdb -and (Test-Path $location.pdb)) {
            Write-Host "    [FOUND] GeoSharPlusCPP.pdb" -ForegroundColor Green
            Copy-Item -Path $location.pdb -Destination $TargetDir -Force
            Write-Host "    - Copied to: $TargetDir" -ForegroundColor Green
        }
    } else {
        if ($location.dll) {
            Write-Host "    [NOT FOUND] GeoSharPlusCPP.dll at: $($location.dll)" -ForegroundColor DarkGray
        }
    }
    
    # Try to copy macOS dylib
    if ($location.dylib -and (Test-Path $location.dylib)) {
        Write-Host "    [FOUND] libGeoSharPlusCPP.dylib" -ForegroundColor Green
        Write-Host "    - Path: $($location.dylib)" -ForegroundColor Gray
        Copy-Item -Path $location.dylib -Destination $TargetDir -Force
        Write-Host "    - Copied to: $TargetDir" -ForegroundColor Green
        $copied = $true
    } else {
        if ($location.dylib) {
            Write-Host "    [NOT FOUND] libGeoSharPlusCPP.dylib at: $($location.dylib)" -ForegroundColor DarkGray
        }
    }
    
    # If we found something in this location, stop searching
    if ($copied) {
        Write-Host ""
        Write-Host "[RESULT] Successfully found and copied C++ libraries" -ForegroundColor Green
        break
    }
    
    $locationIndex++
}

if (-not $copied) {
    Write-Host ""
    Write-Host "[WARNING] Could not find C++ libraries in any expected location" -ForegroundColor Red
    Write-Host ""
    Write-Host "Searched locations:" -ForegroundColor Yellow
    foreach ($location in $locations) {
        Write-Host "  - $($location.name)" -ForegroundColor Gray
        if ($location.dll) {
            Write-Host "    DLL: $($location.dll)" -ForegroundColor Gray
        }
        if ($location.dylib) {
            Write-Host "    DYLIB: $($location.dylib)" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PRE-BUILD COMPLETED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
