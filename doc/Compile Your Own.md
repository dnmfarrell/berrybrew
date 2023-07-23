## Compile Your Own 

### Get the distribution

    git clone https://github.com/stevieb9/berrybrew
    cd berrybrew
   
### Compile the Messaging library

    mcs^
        src\messaging.cs^
        -lib:bin^
        -t:library^
        -out:bin\bbmessaging.dll^

### Compile the API library

    mcs^
        src\berrybrew.cs^
        -lib:bin^
        -t:library^
        -out:bin\bbapi.dll^
        -r:bbmessaging.dll^
        -r:Newtonsoft.Json.dll^
        -r:ICSharpCode.SharpZipLib.dll^ 

### Compile the berrybrew.exe binary

    mcs ^
        src/bbconsole.cs^
        -lib:bin ^
        -out:bin/berrybrew.exe ^
        -r:bbapi.dll ^
        -r:bbmessaging.dll ^
        -win32icon:inc/berrybrew.ico ^

### Compile the UI

    mcs^
        src\berrybrew-ui.cs^
        -lib:bin^
        -t:winexe^
        -out:bin/berrybrew-ui.exe^
        -r:bbapi.dll^
        -r:bbmessaging^
        -r:System.Drawing^
        -r:System.Windows.Forms^
        -r:Microsoft.Visualbasic.dll^
        -win32icon:inc/berrybrew.ico^
        -win32manifest:berrybrew.manifest^
