##Contributing to berrybrew

What we do:

- create a new branch appropriately named for the changes we're making
- PR (Pull Request) the branch when our work is complete
- send individual patches via any means desired if a PR isn't desired
- build the new `dll` and `exe` using the `dev\build.bat` script
- ensure the changes work by calling `berrybrew` in the `build` directory: `build\berrybrew <cmd> [opts]`
- ensure that the appropriate documentation has been updated *
- ensure that all existing unit tests pass, per [Unit testing](https://github.com/stevieb9/berrybrew/blob/master/doc/Unit%20Testing.md) *
- ensure that any functionality/API/runtime changes have explicit unit tests written *

\* Unlikely to accept changes that don't confirm to these

What we don't do:

- work on `master` directly, unless correcting doc typos and other trivial changes
- update the version number in `data\messages.json` or the documentation
- change any license information (I inherited this project)
- change any copyrights (without written authorization)
- update the `Changes` file
- commit locally required changes to the `.gitignore` file

&copy; 2016 by Steve Bertrand 
