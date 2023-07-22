@echo off

set BB_RUN_MODE=testing

mkdir testing
mkdir testing\data

copy dev\data\*.json testing\data

call perl -i.bak -ne "my $bs=chr(92); s/berrybrew(?!\\+testing)/berrybrew${bs}${bs}testing/; print" testing/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"testing\"/; print" testing/data/config.json

echo "compiling dll..."
call mcs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:testing\bbapi.dll^
    src\berrybrew.cs

echo "compiling binary..."
call mcs^
    -lib:testing^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:testing/berrybrew.exe^
    src\bbconsole.cs

copy testing\berrybrew.exe testing\bb.exe
copy bin\berrybrew-refresh.bat testing\
copy bin\ICSharpCode.SharpZipLib.dll testing\
copy bin\Newtonsoft.Json.dll testing\
