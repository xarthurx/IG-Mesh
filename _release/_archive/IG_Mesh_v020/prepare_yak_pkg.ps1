
Remove-Item manifest.yml
&'C:\Program Files\Rhino 7\System\Yak.exe' spec

Add-Content manifest.yml "`nicon_url: https://i.imgur.com/9eJy5Bs.png `n"
Add-Content manifest.yml "keywords `n - drawing `n - climate `n - soil `n - language"

echo "========================="
echo "Modified Manifest File:"
echo "========================="
Get-Content manifest.yml


echo "========================="
echo "Build Package:"
echo "========================="
&'C:\Program Files\Rhino 7\System\Yak.exe' build

echo "========================="
echo "Build Package:"
echo "========================="
&'C:\Program Files\Rhino 7\System\Yak.exe' login
