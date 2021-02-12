# Contributing to berrybrew

Bug/issue reporting and feature requests via Github are very welcomed!

What contributors should do at a minimum:

- Fork the repository
- Create an aptly named branch for the change from the master branch
- Make the changes

Note: Patches are welcome as well, if the contributor can't/doesn't want to go
through the fork/PR process.

If Mono is installed:

- Build the new `dll` and `exe` using the `dev\build.bat` script
- Ensure the changes work by calling `berrybrew` in the `build` directory: `build\berrybrew <cmd> [opts]`

Then:

- Send the PR
- Wait for review

What we do as the authors/maintainers:

- Create a new branch appropriately named for the changes we're making
- Make the changes
- Build the new `dll` and `exe` using the `dev\build.bat` script
- Ensure the changes work by calling `berrybrew` in the `build` directory: `build\berrybrew <cmd> [opts]`
- Ensure that the appropriate documentation has been updated
- Ensure that any functionality/API/runtime changes have explicit unit tests written
- Ensure that all existing unit tests pass, per [Unit testing](doc/Unit%20Testing.md)
- PR (Pull Request) the branch to the most recent vx.xx branch that exists

What contributors shouldn't do:

- Work on `master` directly
- Update any version information
- Change any license information (I inherited this project)
- Change any copyrights (without written authorization)
- Update the `Changes` file
- Commit locally required changes to the `.gitignore` file

&copy; 2016-2021 by Steve Bertrand