## Create a Development Build

During development, it's handy to be able to ensure the code builds and works
correctly without overwriting the currently-installed production installation.

This is a must for testing out new features to ensure they work correctly prior
to running the [unit test](https://github.com/stevieb9/berrybrew/blob/master/doc/Unit%20Testing.md)
suite.

Create the new development build:

- run the `dev\build.bat` script, which compiles the binary and library, and
places the new build within a newly-created `build` directory within your
repository directory

Use the new development build:

- simply run `berrybrew` out of the new build directory, eg:

    build\berrybrew.exe command [options]