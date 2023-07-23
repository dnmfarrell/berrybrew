@echo off
mkdir staging
mkdir staging\data

copy dev\data\*.json staging\data

call perl -i.bak -ne "s/berrybrew(?!\\staging)/berrybrew\\\\staging/; print" staging/data/config.json
call perl -i.bak -ne "s/\"run_mode\"\s+:\s+\"prod\"/\"run_mode\"\t\t  : \"staging\"/; print" staging/data/config.json

copy bin\ICSharpCode.SharpZipLib.dll staging\
copy bin\Newtonsoft.Json.dll staging\

echo "compiling Messaging dll"

call mcs^
    src\messaging.cs^
    -lib:staging^
    -t:library^
    -out:staging\bbmessaging.dll

echo "compiling API dll..."

call mcs^
    src\berrybrew.cs^
    -lib:staging^
    -t:library^
    -out:staging\bbapi.dll^
    -r:bbmessaging.dll^
    -r:Newtonsoft.Json.dll^
    -r:ICSharpCode.SharpZipLib.dll

echo "compiling binary..."

call mcs^
    src\bbconsole.cs^
    -lib:staging^
    -out:staging/berrybrew.exe^
    -r:bbapi.dll^
    -r:bbmessaging.dll^
    -win32icon:inc/berrybrew.ico

echo "compiling UI..."

call mcs^
    src\berrybrew-ui.cs^
    -lib:staging^
    -out:staging/berrybrew-ui.exe^
    -r:bbapi.dll^
    -r:System.Drawing^
    -r:System.Windows.Forms^
    -r:Microsoft.VisualBasic.dll^
    -win32icon:inc/berrybrew.ico^
    -win32manifest:berrybrew.manifest

copy staging\berrybrew.exe staging\bb.exe
copy bin\berrybrew-refresh.bat staging\

