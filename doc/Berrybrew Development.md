# Berrybrew Development 

## Sections

- [Development Directory Items](#development-directory-items)
- [Compile Full Build (Except Installer)](#development-environment-build)
- [Compile Full Build (With Installer)](#development-installer-build)
- [Compile Binary Only](#berrybrew-binary-only)
- [Compile API Only](#api-only)
- [Compile User Interface](#user-interface)
- [Manually Compile Your Own](Compile%20Your%20Own.md)
- [Production Build for Testing](#production-build)
- [Adding to and Modifying the Codebase](#adding-to-and-modifying-the-codebase)
 
During development, it's handy to be able to ensure the code builds and works
correctly without overwriting the currently-installed production installation.

This is a must for testing out new features to ensure they work correctly prior
to running the [unit test](Unit%20Testing.md) suite.

#### Development directory items

These are the files and tools in the `dev\` directory, and their purposes:

| File                                   | Description                                                                                                                                                                                 |
|----------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **_build_prod_installer_helper.pl**    | Builds production installer. Runs manifest checks, builds, and puts the binary into `download`                                                                                              |
| **_build_staging_installer_helper.pl** | Builds staging installer. Runs manifest checks, builds and puts the binary into `staging\`                                                                                                  |
| **build_prod.bat**                     | Builds the production API, binary and UI (into `bin\`). Copies `dev\data\*` to `data\`                                                                                                      | 
| **build_prod_installer.bat**           | Wrapper for `_build_prod_installer_helper.pl`                                                                                                                                               |
| **build_staging.pl**                   | Performs all tasks within the `build_staging_api.bat`, `build_staging_bb.bat` and `build_staging_ui.bat` scripts                                                                            |
| **build_staging_api.bat**              | Builds the staging `bbapi.dll` library file. Puts it into `staging\`                                                                                                                        |
| **build_staging_bb.bat**               | Builds the staging `berrybrew.exe` binary, puts it into `staging\`                                                                                                                          |
| **build_staging_installer.bat**        | Wrapper script for `build_staging_installer_helper.pl`                                                                                                                                      |
| **build_staging_ui.bat**               | Builds the `berrybrew-ui.exe` UI binary. Puts it into `staging\`                                                                                                                            |
| **build_testing.bat**                  | Sets up and builds the entire unit testing environment. It's located in `testing\`                                                                                                          |
| **create_prod_installer.nsi**          | Production installer configuration script                                                                                                                                                   |
| **create_staging_installer.nsi**       | Staging installer configuration script                                                                                                                                                      |
| **generate_github_releases.pl**        | Script that creates the `releases.json` file until Strawberry site is back online                                                                                                           |
| **NSIS.zip**                           | The installer builder software. If not installed on your `berrybrew` dev platform you can install from here                                                                                 |
| **release.pl**                         | Creates a Berrybrew release. See [Create a Release](Create%20a%20Release.md)                                                                                                                |
| **release_cycle.pl**                   | After a release, this script cycles the repository in preparation for the next version. See [Prepare for next version](Create%20a%20Release.md#prepare-a-branch-for-the-next-release-cycle) |
| **release_post.pl**                    | After a release, restores any backed up configuration files (very rarely used)                                                                                                              |

#### Development environment build

- Run the `dev\build_staging.bat` script, which compiles the binary, library and UI and
places the new build within a newly-created `build` directory within your
repository directory

Use the new development build:

- Simply run `berrybrew` out of the new build directory, eg:

    `build\berrybrew.exe <command> [options]`

- If modifying the config file, do a `berrybrew options-update-force` for the
updated directives to be pushed up into the registry

#### Development installer build 

- Run `dev\build_staging_installer.bat`. This will run the complete
`dev\build_staging.bat` script mentioned above, and then create an installer
with a minimized installation, and place it into the repo's `staging` directory. 
 
The installer will install into the `%PROGRAM_FILES%/berrybrew/staging`
directory. We use the `dev\create_staging_installer.nsi` NSIS installer script
to configure the actual installation binary.

**NOTE**: When running under the development/staging build, Perl installations
and `berrybrew`'s temporary directory are stored in a new root level directory,
`C:\berrybrew-staging`. Temporary files will be in `C:\berrybrew-staging\temp`,
and perl instances in `C:\berrybrew-staging\instance`.

#### berrybrew binary only

- Run the `dev\build_staging_bb.bat` script

#### API only

- Run the `dev\build_staging_api.bat` script

#### User Interface

- Run the `dev\build_staging_ui.bat` script, which runs `dev\build_staging.bat`
compiling the API and the `berrybrew` binary, followed by the UI binary itself.

- Run the `staging\berrybrew-ui.exe` to start the UI. Note that the staging UI
build will execute out of a command line window, so that you can see the debugging
output.

#### Production Build

To perform testing with the production aspects of `berrybrew` without creating
a full blown release, run the `dev\build_prod.bat` script. The production build
operates out of the `bin\` directory (ie. `bin\berrybrew.exe`).

The production installer can be built using the `dev\build_prod_installer.bat`
script. The resulting installer binary will be located in
`download\berrybrewInstaller.exe`.

#### Adding to and Modifying the Codebase

This section lists the main typical changes that occur when developing
`berrybrew`, each links to its own dedicated documentation that outlines the
steps and procedures to complete the tasks.

| Task                                                                          | Description                |
|-------------------------------------------------------------------------------|----------------------------|
| [Add a new class (API)](Add%20a%20New%20Class.md)                             | Add a new class to the API | 
| [Add a new command (binary)](Add%20a%20New%20Binary%20Command.md)             | Add a new command to `berrybrew.exe` |
| [Add a new API method](Add%20a%20New%20API%20Method.md)                       | Add a method to an API related source file |
| [Modify an API method](Modify%20an%20API%20Method.md)                         | Modify an existing API method |
| [Modify a binary command](Modify%20a%Binary%20Command.md)                     | Modify an existing binary command (add/change subcmd, options etc) |
| [Add a new config option](Add%20a%20New%20Config%20Option.md)                 | Add a new configuration option |
| [Modify a config option](Modify%20a%20Config%20Option.md)                     | Modify an existing configuration option |
| [Add an exit code](Add%20an%20error%20code.md)                                | Add a new error code |
| [Add a new display text section](Add%20a%20new%20display%20text%20section.md) | Add a new `Message` class section |

&copy; 2016-2023 by Steve Bertrand
