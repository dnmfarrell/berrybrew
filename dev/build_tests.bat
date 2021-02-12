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

rem echo "compiling UI..."
rem call csc^
rem    -lib:build^
rem    -r:bbapi.dll^
rem    -r:System.Drawing.dll^
rem    -r:System.Windows.Forms.dll^
rem    -win32icon:inc/berrybrew.ico^
rem    -win32manifest:berrybrew.manifest^
rem    -t:winexe^
rem    -out:test/berrybrew-ui.exe^
rem    src\berrybrew-ui.cs

copy test\berrybrew.exe test\bb.exe
copy bin\berrybrew-refresh.bat test\
copy bin\ICSharpCode.SharpZipLib.dll test\
copy bin\Newtonsoft.Json.dll test\