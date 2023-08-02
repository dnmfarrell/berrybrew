# Contributing to berrybrew

Bug/issue reporting and feature requests via Github are very welcomed!

- [What code contributors should do](#what-code-contributors-should-do-at-a-minimum)
- [What code contributors shouldn't do](#what-code-contributors-shouldnt-do)
- [What the maintainers do](#what-we-do-as-the-authorsmaintainers)
 
### What code contributors should do at a minimum:

- Read through the [berrybrew development](doc/Berrybrew%20development.md)
documentation
- Fork the repository
- Checkout the current vX.XX branch, and create a new branch appropriately named
for the changes you're making
- Make the changes

Note: Patches are welcome as well, if the contributor can't/doesn't want to go
through the fork/PR process.

If Mono is installed:

- Build the new `dll`, `exe` and `ui` using the `dev\build_staging.bat` script
- Ensure the changes work by calling `berrybrew` in the `staging` directory:
`staging\berrybrew <cmd> [opts]`

Then:

- Create and send a Pull Request against the **current vX.XX** branch
(**not master**)
- Wait for review

### What code contributors shouldn't do:

- Work on `master` or the current vX.XX branch directly
- Update any version information
- Change any license information (I inherited this project)
- Change any copyrights (without written authorization)
- Update the `Changes` file
- Commit locally required changes to the `.gitignore` file

### What we do as the authors/maintainers:

- Use the [berrybrew development](doc/Berrybrew%20development.md) documentation
as a reference
- Create a new branch off of the current vX.XX branch, appropriately named for
the changes we're making
- Make the changes
- Build the new `dll`, `exe`  and `ui` using the `dev\build_staging.bat` script
- Ensure the changes work by calling `berrybrew` in the `staging` directory:
`staging\berrybrew <cmd> [opts]`
- Ensure that the appropriate documentation has been updated
- Ensure that any functionality/API/runtime changes have explicit unit tests
written
- Ensure that all existing unit tests pass, per
[Unit Testing](doc/Unit%20testing.md)
- PR (Pull Request) the branch to the most recent vX.XX branch that exists

&copy; 2016-2023 by Steve Bertrand