mkdir build
mkdir build\data

call mcs^
    -lib:bin^
    -t:library^
    -r:ICSharpCode.SharpZipLib.dll,Newtonsoft.Json.dll^
    -out:build\bbapi.dll^
    src\berrybrew.cs

call mcs^
    src\bbconsole.cs^
    -lib:build^
    -r:bbapi.dll^
    -win32icon:berrybrew.ico^
    -out:build\berrybrew.exe 

copy bin\ICSharpCode.SharpZipLib.dll build\
copy bin\Newtonsoft.Json.dll build\
copy data\* build\data\
