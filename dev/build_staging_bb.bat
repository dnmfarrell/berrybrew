@echo off

set BB_RUN_MODE=staging

mkdir staging
mkdir staging\data

copy dev\data\*.json staging\data

call perl -i.bak -ne "s/berrybrew(?!\\staging)/berrybrew\\\\staging/; print" staging/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"staging\"/; print" staging/data/config.json

echo "compiling berrybrew binary..."

call mcs^
    src\bbconsole.cs
    -lib:staging^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:staging/berrybrew.exe^

copy staging\berrybrew.exe staging\bb.exe
copy bin\berrybrew-refresh.bat staging\
copy bin\ICSharpCode.SharpZipLib.dll staging\
copy bin\Newtonsoft.Json.dll staging\
