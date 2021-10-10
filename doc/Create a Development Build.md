# Create a Development Build

## Sections

- [Full Build Except Installer](#create-the-new-development-build)
- [Full Build With Installer](#installer)
- [UI Only](#ui-only)
- [Binary Only](#berrybrew-binary-only)
- [API Only](#api-only)

During development, it's handy to be able to ensure the code builds and works
correctly without overwriting the currently-installed production installation.

This is a must for testing out new features to ensure they work correctly prior
to running the [unit test](Unit%20Testing.md) suite.

#### Create the new development build:

- Run the `dev\build.bat` script, which compiles the binary, library and UI and
places the new build within a newly-created `build` directory within your
repository directory

Use the new development build:

- Simply run `berrybrew` out of the new build directory, eg:

    `build\berrybrew.exe <command> [options]`

- If modifying the config file, do a `berrybrew options-update-force` for the
updated directives to be pushed up into the registry

#### UI only

- Run the `dev\build_ui.bat` script, which compiles the UI binary. If not done
previously, you need to run `dev\build.bat` to build the API library first.

- Run the `build\berrybrew-ui.exe` to start the UI. Note that when using the
dev build script for the UI directly, the UI will run out of the command line
window as opposed to a GUI app so that you can see the debugging output

#### berrybrew binary only

- Run the `dev\build_bb.bat` script

#### API only

- Run the `dev\build_api.bat` script

#### Installer

- Run `perl dev\build_installer.pl`. This will run the complete `dev\build.bat` script
mentioned above, and then create an installer with a minimized installation, and
place it into the repo's `build/` directory. The installer will install into 
the `%PROGRAM_FILES%/berrybrew/build` directory. We use the `dev\create_build_installer.nsi`
NSIS installer script.

**NOTE**: When running under the development build, Perl installations and
`berrybrew`'s temporary directory are stored within a newly created `build`
directory underneath of `berrybrew`'s default directory (defaults to 
`C:\berrybrew`) 