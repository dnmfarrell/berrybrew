@echo off

set BB_RUN_MODE=staging

mkdir staging
mkdir staging\data

copy dev\data\*.json staging\data

call perl -i.bak -ne "s/berrybrew(?!\\staging)/berrybrew\\\\staging/; print" staging/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"staging\"/; print" staging/data/config.json

echo "compiling UI..."

call csc^
    src\berrybrew-ui.cs^
    src\perloperations.cs^
    -lib:staging^
    -r:bbapi.dll^
    -r:System.Drawing.dll^
    -r:System.Windows.Forms.dll^
    -r:Microsoft.VisualBasic.dll^
    -win32icon:inc/berrybrew.ico^
    -win32manifest:berrybrew.manifest^
    -out:staging/berrybrew-ui.exe^