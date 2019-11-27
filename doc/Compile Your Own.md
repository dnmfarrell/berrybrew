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

    bin\berrybrew.exe config
