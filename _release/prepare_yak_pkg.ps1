# Description: Prepare the package for Yak
#
$scriptPath = $PSScriptRoot
if (!$scriptPath) {
    # Fallback for older PowerShell versions or when invoked differently
    $scriptPath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}

$currentFolder = $scriptPath
# $currentFolder = Get-Location
$targetFolder = (Get-Item $currentFolder).Parent.FullName
$binFolder = Join-Path -Path $targetFolder -ChildPath "bin"

echo "========================="
echo "current folder:"
echo $currentFolder
echo "target folder:"
echo $targetFolder
echo "bin folder path:"
echo $binFolder
echo "========================="

# Download Yak.exe if not already present
curl.exe -fSLo yak.exe https://files.mcneel.com/yak/tools/0.13.0/yak.exe

#--------------------------------------
# Rhino 8, net7.0
#--------------------------------------
if (Test-Path "releaseRH8"){
  Remove-Item releaseRH8 -Recurse
}
New-Item -Path "releaseRH8" -ItemType Directory -Force

Push-Location ".\releaseRH8"; 

if (Test-Path "manifest.yml")
{
  Remove-Item manifest.yml
}

# Copy files and icon - use the determined bin folder path 
Copy-Item -Path "${binFolder}\Release\*" -Destination "." -Recurse -ErrorAction SilentlyContinue
Copy-Item -Path "${currentFolder}\icon_new.png" -Destination "." -Recurse

./../yak.exe spec; 
Add-Content manifest.yml "`nicon: icon_new.png"
Add-Content manifest.yml "`nkeywords: `n - mesh `n - geometry `n - high-performance"

Write-Host "======================================="
Write-Host "Modified Manifest File for NetCore 7, Rhino 8"
Write-Host "======================================="
Get-Content manifest.yml

./../yak.exe build

Pop-Location
# then yak push xx.yak in the cmd line
