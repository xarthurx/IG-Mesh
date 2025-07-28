param(
    [string]$TargetDir,
    [string]$SourceDir,
    [string]$Configuration
)

# Create cppPrebuild directory if it doesn't exist
$cppPrebuildDir = Join-Path $SourceDir $Configuration

# Check if we found any DLL
$targetDll = Join-Path $cppPrebuildDir "GeoSharPlusCPP.dll"
$targetPDB = Join-Path $cppPrebuildDir "GeoSharPlusCPP.pdb"

if (Test-Path $targetDll) {
    # Copy to the target directory as well
    Write-Host "Copying to project's target directory: $TargetDir"
    Copy-Item -Path $targetDll -Destination $TargetDir -Force
    if (Test-Path $targetPDB) {
		Copy-Item -Path $targetPDB -Destination $TargetDir -Force
    }
    
    if (Test-Path ($targetDll)) {
        Write-Host "Successfully copied GeoSharPlusCPP.dll and PDB file to $TargetDir"
    } else {
        Write-Host "Warning: Failed to copy GeoSharPlusCPP.dll PDB file to $TargetDir"
    }
} else {
    Write-Host "Warning: Could not find GeoSharPlusCPP.dll in any expected location"
}
