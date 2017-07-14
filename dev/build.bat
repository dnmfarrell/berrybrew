@echo off
mkdir build
mkdir build\data
copy dev\data\*.json build\data                 & rem [pryrt] for Contributing.md::`build\berrybrew <cmd> [opts]` to work, build\data\config.json _must_ exist

echo "compiling dll..."

call mcs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:build\bbapi.dll^
    src\berrybrew.cs

echo "compiling binary..."

call mcs^
    -lib:build^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:build/berrybrew.exe^
    src\bbconsole.cs

rem copy bin\Ionic.Zip.dll build\               & rem [pryrt] Ionic* no longer exists
copy bin\ICSharpCode.SharpZipLib.dll build\     & rem [pryrt] but ICSharpCode* _is_ needed
copy bin\Newtonsoft.Json.dll build\
