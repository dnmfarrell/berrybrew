@echo off
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

echo "compiling binary..."

call mcs^
    -lib:build^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:build/berrybrew.exe^
    src\bbconsole.cs

echo "compiling UI..."

call mcs^
    -lib:build^
    -r:bbapi.dll^
    -r:System.Drawing^
    -r:System.Windows.Forms^
    -win32icon:inc/berrybrew.ico^
    -out:build/berrybrew-ui.exe^
    src\berrybrew-ui.cs

copy build\berrybrew.exe build\bb.exe    
copy bin\berrybrew-refresh.bat build\
copy bin\ICSharpCode.SharpZipLib.dll build\
copy bin\Newtonsoft.Json.dll build\
