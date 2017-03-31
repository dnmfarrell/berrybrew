mkdir build
mkdir build\data

echo "compiling dll..."

call mcs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:build\bbapi.dll^
    src\berrybrew.cs

echo "compiling binary..."

call mcs^
    src\bbconsole.cs^
    -lib:build^
    -r:bbapi.dll^
    -win32icon:inc/berrybrew.ico^
    -out:build/berrybrew.exe

copy bin\Ionic.Zip.dll build\
copy bin\Newtonsoft.Json.dll build\
