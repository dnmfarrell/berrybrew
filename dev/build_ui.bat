@echo off

set BB_RUN_MODE=build

mkdir build
mkdir build\data

copy dev\data\*.json build\data

call perl -i.bak -ne "s/berrybrew(?!\\build)/berrybrew\\\\build/; print" build/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"build\"/; print" build/data/config.json

echo "compiling UI..."

call csc^
    -lib:build^
    -r:bbapi.dll^
    -r:System.Drawing.dll^
    -r:System.Windows.Forms.dll^
    -r:Microsoft.VisualBasic.dll^
    -win32icon:inc/berrybrew.ico^
    -win32manifest:berrybrew.manifest^
    -out:build/berrybrew-ui.exe^
    src\berrybrew-ui.cs