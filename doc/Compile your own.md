## Compile Your Own 

### Get the distribution

    git clone https://github.com/stevieb9/berrybrew
    cd berrybrew
  
### Compile the API library

    mcs^
        src\berrybrew.cs^
        src\messaging.cs^
        src\pathoperations.cs^
        src\perlinstance.cs^
        src\perloperations.cs^
        -lib:bin^
        -t:library^
        -out:bin\bbapi.dll^
        -r:Newtonsoft.Json.dll^
        -r:ICSharpCode.SharpZipLib.dll^ 

### Compile the berrybrew.exe binary

    mcs ^
        src/bbconsole.cs^
        -lib:bin^
        -out:bin/berrybrew.exe^
        -win32icon:inc/berrybrew.ico^
        -r:bbapi.dll

### Compile the UI

    mcs^
        **src\berrybrew-ui.cs^
        src\perloperations.cs^
        -lib:bin^
        -t:winexe^
        -out:bin/berrybrew-ui.exe^
        -r:bbapi.dll^
        -r:System.Drawing^
        -r:System.Windows.Forms^
        -r:Microsoft.Visualbasic.dll^
        -win32icon:inc/berrybrew.ico^
        -win32manifest:berrybrew.manifest

### Copy/Paste Entire Process

    mcs^
        src\berrybrew.cs^
        src\messaging.cs^
        src\pathoperations.cs^
        src\perlinstance.cs^
        src\perloperations.cs^
        -lib:bin^
        -t:library^
        -out:bin\bbapi.dll^
        -r:Newtonsoft.Json.dll^
        -r:ICSharpCode.SharpZipLib.dll

    mcs ^
        src/bbconsole.cs^
        -lib:bin^
        -out:bin/berrybrew.exe^
        -win32icon:inc/berrybrew.ico^
        -r:bbapi.dll

    mcs^
        **src\berrybrew-ui.cs^
        src\perloperations.cs^
        -lib:bin^
        -t:winexe^
        -out:bin/berrybrew-ui.exe^
        -r:bbapi.dll^
        -r:System.Drawing^
        -r:System.Windows.Forms^
        -r:Microsoft.Visualbasic.dll^
        -win32icon:inc/berrybrew.ico^
        -win32manifest:berrybrew.manifest

&copy; 2016-2023 by Steve Bertrand