@echo off
mkdir build
mkdir build\data

copy dev\data\*.json build\data

call perl -i.bak -ne "s/berrybrew(?!\\\\test)/berrybrew\\\\test/; print" build/data/config.json

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

copy bin\ICSharpCode.SharpZipLib.dll build\
copy bin\Newtonsoft.Json.dll build\
