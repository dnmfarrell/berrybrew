@echo off
mkdir test
mkdir test\data

copy dev\data\*.json test\data
copy bin\env.exe test

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

echo "compiling UI..."

call csc^
    -lib:build^
    -r:bbapi.dll^
    -r:System.Drawing.dll^
    -r:System.Windows.Forms.dll^
    -win32icon:inc/berrybrew.ico^
    -win32manifest:berrybrew.manifest^
    -t:winexe^
    -out:test/berrybrew-ui.exe^
    src\berrybrew-ui.cs

copy bin\berrybrew-refresh.bat test\
copy bin\ICSharpCode.SharpZipLib.dll test\
copy bin\Newtonsoft.Json.dll test\