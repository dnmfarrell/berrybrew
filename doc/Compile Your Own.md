## Compile Your Own 

    git clone https://github.com/stevieb9/berrybrew
    cd berrybrew
    
    # compile the API library

    mcs \
        -lib:bin \
        -t:library \
        -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll \ 
        -out:bin/bbapi.dll \
        src/berrybrew.cs

    # compile the berrybrew.exe binary

    mcs \
        src/bbconsole.cs
        -lib:bin \
        -r:bbapi.dll \
        -out:bin/berrybrew.exe \
        -win32icon:inc/berrybrew.ico

    mcs \
        -lib:build \
        -r:bbapi.dll \
        -r:System.Drawing \
        -r:System.Windows.Forms \
        -win32icon:inc/berrybrew.ico \
        -out:bin/berrybrew-ui.exe \
        src\berrybrew-ui.cs
        
    bin\berrybrew.exe config
