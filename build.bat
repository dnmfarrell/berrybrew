mkdir build
mkdir build\data

echo "compiling dll..."

call mcs -lib:bin -t:library -r:ICSharpCode.SharpZipLib.dll,Newtonsoft.Json.dll^
 -out:build\bbapi.dll src\berrybrew.cs

echo "compiling binary..."

call mcs src\bbconsole.cs -lib:build -r:bbapi.dll^ -out:build\berrybrew.exe 

copy bin\ICSharpCode.SharpZipLib.dll build\
copy bin\Newtonsoft.Json.dll build\
copy data\* build\data\
