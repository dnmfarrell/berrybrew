@echo off
mkdir test
mkdir test\data

copy dev\data\*.json test\data

call perl -i.bak -ne "s/berrybrew(?!\\\\test)/berrybrew\\\\test/; print" test/data/config.json

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

copy bin\ICSharpCode.SharpZipLib.dll test\
copy bin\Newtonsoft.Json.dll test\
