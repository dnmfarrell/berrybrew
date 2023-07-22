@echo off
mkdir staging
mkdir staging\data

copy dev\data\*.json staging\data

call perl -i.bak -ne "s/berrybrew(?!\\staging)/berrybrew\\\\staging/; print" staging/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"staging\"/; print" staging/data/config.json

echo "compiling dll..."

call mcs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:staging\bbapi.dll^
    src\berrybrew.cs

echo "compiling binary..."

call mcs^
    -lib:staging^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:staging/berrybrew.exe^
    src\bbconsole.cs

echo "compiling UI..."

call mcs^
    -lib:staging^
    -r:bbapi.dll^
    -r:System.Drawing^
    -r:System.Windows.Forms^
    -r:Microsoft.VisualBasic.dll^
    -win32icon:inc/berrybrew.ico^
    -win32manifest:berrybrew.manifest^
    -out:staging/berrybrew-ui.exe^
    src\berrybrew-ui.cs

copy staging\berrybrew.exe staging\bb.exe
copy bin\berrybrew-refresh.bat staging\
copy bin\ICSharpCode.SharpZipLib.dll staging\
copy bin\Newtonsoft.Json.dll staging\
