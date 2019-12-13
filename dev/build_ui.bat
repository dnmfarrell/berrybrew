@echo off
mkdir build
mkdir build\data

copy dev\data\*.json build\data

call perl -i.bak -ne "s/berrybrew(?!\\\\build)/berrybrew\\\\build/; print" build/data/config.json

echo "compiling UI..."

call mcs^
    -lib:build^
    -r:bbapi.dll^
    -r:System.Drawing^
    -r:System.Windows.Forms^
    -win32icon:inc/berrybrew.ico^
    -out:build/berrybrew-ui.exe^
    src\berrybrew-ui.cs
