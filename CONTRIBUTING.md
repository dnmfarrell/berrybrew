# Contributing to berrybrew

Bug/issue reporting and feature requests via Github are very welcomed!

What contributors should do at a minimum:

- fork the repository
- create an aptly named branch for the change from the master branch
- make the changes

Note: Patches are welcome as well, if the contributor can't/doesn't want to go
through the fork/PR process.

If Mono is installed:

- build the new `dll` and `exe` using the `dev\build.bat` script
- ensure the changes work by calling `berrybrew` in the `build` directory: `build\berrybrew <cmd> [opts]`

Then:

- send the PR
- wait for review

What we do as the authors/maintainers:

- create a new branch appropriately named for the changes we're making
- make the changes
- build the new `dll` and `exe` using the `dev\build.bat` script
- ensure the changes work by calling `berrybrew` in the `build` directory: `build\berrybrew <cmd> [opts]`
- ensure that the appropriate documentation has been updated
- ensure that any functionality/API/runtime changes have explicit unit tests written
- ensure that all existing unit tests pass, per [Unit testing](https://github.com/stevieb9/berrybrew/blob/master/doc/Unit%20Testing.md)
- PR (Pull Request) the branch to the most recent vx.xx branch that exists


What contributors shouldn't do:

- work on `master` directly
- update any version information
- change any license information (I inherited this project)
- change any copyrights (without written authorization)
- update the `Changes` file
- commit locally required changes to the `.gitignore` file


&copy; 2018 by Steve Bertrand
