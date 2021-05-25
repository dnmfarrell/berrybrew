@echo off

set BB_RUN_MODE=build

mkdir build
mkdir build\data

copy dev\data\*.json build\data

call perl -i.bak -ne "s/berrybrew(?!\\\\build)/berrybrew\\\\\\\\build/; print" build/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"build\"/; print" build/data/config.json

echo "compiling berrybrew binary..."

call mcs^
    -lib:build^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:build/berrybrew.exe^
    src\bbconsole.cs

copy build\berrybrew.exe build\bb.exe
copy bin\berrybrew-refresh.bat build\
copy bin\ICSharpCode.SharpZipLib.dll build\
copy bin\Newtonsoft.Json.dll build\
