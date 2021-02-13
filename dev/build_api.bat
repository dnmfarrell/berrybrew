@echo off

set BB_RUN_MODE=build

mkdir build
mkdir build\data

copy dev\data\*.json build\data

call perl -i.bak -ne "s/berrybrew(?!\\\\build)/berrybrew\\\\\\\\build/; print" build/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"build\"/; print" build/data/config.json

echo "compiling dll..."

call mcs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:build\bbapi.dll^
    src\berrybrew.cs

copy bin\berrybrew-refresh.bat build\
copy bin\ICSharpCode.SharpZipLib.dll build\
copy bin\Newtonsoft.Json.dll build\
