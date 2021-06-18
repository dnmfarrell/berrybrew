@echo off

set BB_RUN_MODE=test

mkdir test
mkdir test\data

copy dev\data\*.json test\data

call perl -i.bak -ne "my $bs=chr(92); s/berrybrew(?!\\+test)/berrybrew${bs}${bs}test/; print" test/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"test\"/; print" test/data/config.json

echo "compiling dll..."
call mcs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:test\bbapi.dll^
    src\berrybrew.cs

echo "compiling binary..."
call mcs^
    -lib:test^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:test/berrybrew.exe^
    src\bbconsole.cs

copy test\berrybrew.exe test\bb.exe
copy bin\berrybrew-refresh.bat test\
copy bin\ICSharpCode.SharpZipLib.dll test\
copy bin\Newtonsoft.Json.dll test\