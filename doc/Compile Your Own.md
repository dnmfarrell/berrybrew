## Compile Your Own 

### Get the distribution

    git clone https://github.com/stevieb9/berrybrew
    cd berrybrew
    
### Compile the API library

    mcs \
        -lib:bin \
        -t:library \
        -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll \ 
        -out:bin/bbapi.dll \
        src/berrybrew.cs

### Compile the berrybrew.exe binary

    mcs \
        src/bbconsole.cs
        -lib:bin \
        -r:bbapi.dll \
        -out:bin/berrybrew.exe \
        -win32icon:inc/berrybrew.ico

### Compile the UI

    csc \
        -lib:build \
        -r:bbapi.dll \
        -r:System.Drawing \
        -r:System.Windows.Forms \
        -win32icon:inc/berrybrew.ico \
        -win32manifest:berrybrew.manifest \
        -t:winexe \
        -out:bin/berrybrew-ui.exe \
        src\berrybrew-ui.cs
