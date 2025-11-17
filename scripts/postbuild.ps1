param(  
   [string]$TargetDir,  
   [string]$SolutionDir,  
   [string]$Configuration  
)  

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  POST-BUILD SCRIPT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Extract the leaf of the TargetDir  
$targetDirLeaf = Split-Path -Leaf $TargetDir  
Write-Host "[INFO] Target directory leaf: $targetDirLeaf" -ForegroundColor Yellow

# Display input parameters
Write-Host ""
Write-Host "[PARAMETERS]" -ForegroundColor Green
Write-Host "  - Target Directory : $TargetDir" -ForegroundColor Gray
Write-Host "  - Solution Directory : $SolutionDir" -ForegroundColor Gray
Write-Host "  - Configuration : $Configuration" -ForegroundColor Gray

# Set output directory  
$outputDir = Join-Path (Join-Path $SolutionDir "bin") $Configuration  
$outputDir = Join-Path $outputDir $targetDirLeaf  # adding the `net8.0-windows` part 

Write-Host ""
Write-Host "[OUTPUT DIRECTORY]" -ForegroundColor Green
Write-Host "  - Creating: $outputDir" -ForegroundColor Gray

# Create output directory  
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null  
Write-Host "  - Directory created successfully" -ForegroundColor Green

Write-Host ""
Write-Host "[COPYING FILES]" -ForegroundColor Green
Write-Host "  - Source: $TargetDir" -ForegroundColor Gray
Write-Host "  - Destination: $outputDir" -ForegroundColor Gray

# Copy build outputs  
$dllFiles = Join-Path $TargetDir "*.dll"
$ghaFiles = Join-Path $TargetDir "*.gha"

if (Test-Path $dllFiles) {
    Copy-Item -Path $dllFiles -Destination $outputDir -Force
    $copiedDlls = (Get-ChildItem -Path $dllFiles).Count
    Write-Host "  - Copied $copiedDlls DLL file(s)" -ForegroundColor Green
} else {
    Write-Host "  - No DLL files found to copy" -ForegroundColor Yellow
}

if (Test-Path $ghaFiles) {
    Copy-Item -Path $ghaFiles -Destination $outputDir -Force
    $copiedGhas = (Get-ChildItem -Path $ghaFiles).Count
    Write-Host "  - Copied $copiedGhas GHA file(s)" -ForegroundColor Green
} else {
    Write-Host "  - No GHA files found to copy" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  POST-BUILD COMPLETED SUCCESSFULLY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
