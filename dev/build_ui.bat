@echo off
mkdir build
mkdir build\data

copy dev\data\*.json build\data

call perl -i.bak -ne "s/berrybrew(?!\\\\build)/berrybrew\\\\build/; print" build/data/config.json

echo "compiling UI..."

call mcs^
    -lib:build^
    -r:bbapi.dll^
    -r:System.Drawing.dll^
    -r:System.Windows.Forms.dll^
    -win32icon:inc/berrybrew.ico^
    -t:winexe^
    -out:build/berrybrew-ui.exe^
    src\berrybrew-ui.cs
