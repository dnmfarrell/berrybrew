## Create a Development Build

During development, it's handy to be able to ensure the code builds and works
correctly without overwriting the currently-installed production installation.

This is a must for testing out new features to ensure they work correctly prior
to running the [unit test](https://github.com/stevieb9/berrybrew/blob/master/doc/Unit%20Testing.md)
suite.

**IMPORTANT**: You **must** set the `BERRYBREW_ENV` environment variable to
`build` for the proper configuration to be loaded.

Create the new development build:

- run the `dev\build.bat` script, which compiles the binary and library, and
places the new build within a newly-created `build` directory within your
repository directory

Use the new development build:

- simply run `berrybrew` out of the new build directory, eg:

    `build\berrybrew.exe command [options]`

#### UI only

- run the `dev\build_ui.bat` script, which compiles the UI binary. If not done
previously, you need to run `dev\build.bat` to build the API library first.

- run the `build\berrybrew-ui.exe` to start the UI. Note that when using the
dev build script for the UI directly, the UI will run out of the command line
window as opposed to a GUI app so that you can see the debugging output

#### berrybrew binary only

- run the `dev\build_bb.bat` script

#### API only

- run the `dev\build_api.bat` script

**NOTE**: When running under the development build, Perl installations and
`berrybrew`'s temporary directory are stored within a newly created `build`
directory underneath of `berrybrew`'s default directory (defaults to 
`C:\berrybrew`)    