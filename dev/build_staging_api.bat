@echo off

set BB_RUN_MODE=staging

mkdir staging
mkdir staging\data

copy dev\data\*.json staging\data

call perl -i.bak -ne "s/berrybrew/berrybrew-staging/; print" staging/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"staging\"/; print" staging/data/config.json

echo "compiling dll..."

call mcs^
    src\berrybrew.cs^
    src\pathoperations.cs^
    src\perlinstance.cs^
    src\perloperations.cs^
    src\messaging.cs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:staging\bbapi.dll^

copy bin\berrybrew-refresh.bat staging\
copy bin\ICSharpCode.SharpZipLib.dll staging\
copy bin\Newtonsoft.Json.dll staging\
