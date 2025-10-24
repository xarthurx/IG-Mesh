
if (Test-Path "manifest.yml")
{
  Remove-Item manifest.yml
}

Copy-Item -Path "..\..\bin\*.dll" -Destination ".\" -Recurse
Copy-Item -Path "..\..\bin\*.gha" -Destination ".\" -Recurse

&'C:\Program Files\Rhino 8\System\Yak.exe' spec

# Add-Content manifest.yml "`nicon: `"https://i.imgur.com/WABE4LN.png`""
Add-Content manifest.yml "`nicon: packageManagerIcon.png"
Add-Content manifest.yml "`nkeywords: `n - geometry-processing `n - mesh-processing `n - mesh"

Write-Host "========================="
Write-Host "Modified Manifest File:"
Write-Host "========================="

Get-Content manifest.yml


echo "========================="
echo "Build Package:"
echo "========================="
&'C:\Program Files\Rhino 7\System\Yak.exe' build

echo "========================="
echo "Build Package:"
echo "========================="
&'C:\Program Files\Rhino 7\System\Yak.exe' login


# then yak push xx.yak in the cmd line
