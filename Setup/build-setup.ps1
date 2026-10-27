
$out = "payload-tmp"
Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item payload.zip -Force -ErrorAction SilentlyContinue

dotnet publish ..\Desktop -c Release -o $out
dotnet publish ..\StartupResident -c Release -o $out

Compress-Archive -Path "$out\*" -DestinationPath payload.zip

dotnet publish ..\Setup -c Release -o "release"

Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item payload.zip -Force -ErrorAction SilentlyContinue
