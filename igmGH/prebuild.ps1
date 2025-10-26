param(
    [string]$TargetDir,
    [string]$SourceDir,
    [string]$Configuration
)

Write-Host "PreBuild: TargetDir = $TargetDir"
Write-Host "PreBuild: SourceDir = $SourceDir"
Write-Host "PreBuild: Configuration = $Configuration"

# Determine the solution directory (go up from SourceDir if needed)
$solutionDir = if ($SourceDir -match "GeoSharPlusCPP") {
    # If SourceDir points to GeoSharPlusCPP\build\, get solution root
    Split-Path (Split-Path (Split-Path $SourceDir -Parent) -Parent) -Parent
} else {
    # Assume SourceDir is already the solution root or close to it
    $SourceDir
}

Write-Host "PreBuild: Determined solution directory = $solutionDir"

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

$copied = $false

foreach ($location in $locations) {
    Write-Host "Checking location: $($location.name)"
    
    # Try to copy Windows DLL
    if ($location.dll -and (Test-Path $location.dll)) {
        Write-Host "Found GeoSharPlusCPP.dll at: $($location.dll)"
        Copy-Item -Path $location.dll -Destination $TargetDir -Force
        Write-Host "Successfully copied GeoSharPlusCPP.dll to $TargetDir"
        $copied = $true
        
        # Copy PDB if available
        if ($location.pdb -and (Test-Path $location.pdb)) {
            Copy-Item -Path $location.pdb -Destination $TargetDir -Force
            Write-Host "Successfully copied GeoSharPlusCPP.pdb to $TargetDir"
        }
    }
    
    # Try to copy macOS dylib
    if ($location.dylib -and (Test-Path $location.dylib)) {
        Write-Host "Found libGeoSharPlusCPP.dylib at: $($location.dylib)"
        Copy-Item -Path $location.dylib -Destination $TargetDir -Force
        Write-Host "Successfully copied libGeoSharPlusCPP.dylib to $TargetDir"
        $copied = $true
    }
    
    # If we found something in this location, stop searching
    if ($copied) {
        break
    }
}

if (-not $copied) {
    Write-Host "Warning: Could not find GeoSharPlusCPP.dll or libGeoSharPlusCPP.dylib in any expected location"
    Write-Host "Searched locations:"
    foreach ($location in $locations) {
        Write-Host "  - $($location.name): $($location.dll), $($location.dylib)"
    }
}
