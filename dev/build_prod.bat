@echo off

copy dev\data\*.json data

copy bin\ICSharpCode.SharpZipLib.dll staging\
copy bin\Newtonsoft.Json.dll staging\

echo "compiling API dll..."

call mcs^
    src\berrybrew.cs^
    src\pathoperations.cs^
    src\perlinstance.cs^
    src\perloperations.cs^
    src\messaging.cs^
    -lib:bin^
    -t:library^
    -out:bin\bbapi.dll^
    -r:Newtonsoft.Json.dll^
    -r:ICSharpCode.SharpZipLib.dll

echo "compiling binary..."

call mcs^
    src\bbconsole.cs^
    -lib:bin^
    -out:bin/berrybrew.exe^
    -win32icon:inc/berrybrew.ico^
    -r:bbapi.dll

echo "compiling UI..."

call mcs^
    src\berrybrew-ui.cs^
    src\perloperations.cs^
    -lib:bin^
    -t:winexe^
    -out:bin/berrybrew-ui.exe^
    -r:bbapi.dll^
    -r:Microsoft.VisualBasic.dll^
    -r:Newtonsoft.Json.dll^
    -r:System.Drawing^
    -r:System.Windows.Forms^
    -win32icon:inc/berrybrew.ico^
    -win32manifest:berrybrew.manifest

copy bin\berrybrew.exe bin\bb.exe

