@echo off

set BB_RUN_MODE=testing

mkdir testing
mkdir testing\data

copy dev\data\*.json testing\data

call perl -i.bak -ne "my $bs=chr(92); s/berrybrew(?!\\+testing)/berrybrew${bs}${bs}testing/; print" testing/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"testing\"/; print" testing/data/config.json

copy bin\ICSharpCode.SharpZipLib.dll testing\
copy bin\Newtonsoft.Json.dll testing\

echo "compiling dll..."

call mcs^
    src\berrybrew.cs^
    src\pathoperations.cs^
    src\perlinstance.cs^
    src\perloperations.cs^
    src\messaging.cs^
    -lib:testing^
    -t:library^
    -out:testing\bbapi.dll^
    -r:Newtonsoft.Json.dll^
    -r:ICSharpCode.SharpZipLib.dll

echo "compiling binary..."

call mcs^
    src\bbconsole.cs^
    -lib:testing^
    -out:testing/berrybrew.exe^
    -win32icon:inc/berrybrew.ico^
    -r:bbapi.dll

copy testing\berrybrew.exe testing\bb.exe
copy bin\berrybrew-refresh.bat testing\