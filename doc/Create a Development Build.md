# Create a Development Build

## Sections

- [Development Directory Items](#development-directory-items)
- [Full Build (Except Installer)](#development-environment-build)
- [Full Build (With Installer)](#development-installer-build)
- [Binary Only](#berrybrew-binary-only)
- [API Only](#api-only)
- [User Interface](#user-interface)
- [Production Build for Testing](#production-build)
 
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

&copy; 2016-2023 by Steve Bertrand