## Create and Publish a New Release

- `git checkout` the branch that's to be merged into `master`

- `git pull` to ensure we're up-to-date

- Update the `Changes` file with the current release date along side the
version that's about to be released

- Run `berrybrew fetch`, and then copy the `data\perls.json` file to the 
`dev\data\` directory

- If configuration directives have been added, removed or modified in the
`data\config.json` that are part of the new release, copy that file to the
`dev\data\` directory

- Ensure all unit tests pass per 
[Unit testing](https://github.com/stevieb9/berrybrew/blob/master/doc/Unit%20Testing.md)

- Execute the `perl dev\release.pl` script, which:

    - Compile the `berrybrew.exe` binary and `bbapi.dll` API library
    - Collect the JSON configuration files from the `dev\data` directory
    - Build the bundled zip archive, and places it into the `download/`
    directory
    - Perform SHA checksum tasks on the new zip archive
    - Update the `README.md` file with the zip archive's new SHA sum
    - Update the `README.md` file with the new version from the API's
    `Version()` method

- If you had any custom configuration files in place before running the
`dev\release.pl` script, run `perl dev\post_release.pl` to put them back to
their proper location

- `git commit -a -m "release x.xx"`

- `git push`

- Ensure everything looks proper within the newly published release branch

- In Github, create a new Pull Request from the release branch, and merge it
into the master branch

- Ensure everything appears proper within the master branch

- Update David Farrell's `berrybrew` repository:

    - `git clone https://stevieb9@github.com/dnmfarrell/berrybrew bb-dnm`
    - `cd bb-dnm`
    - `git remote add stevieb9 https://github.com/stevieb9/berrybrew`
    - `git pull stevieb9 master`
    - `git push`
    
- Delete the merged branch in Github
    
## Prepare a Branch for the Next Release Cycle

- Check out the updated `master` branch: `git checkout master`

- Ensure it's up-to-date: `git pull`

- Locally, create a new branch: `git checkout -b vx.xx`

- Push the new branch to Github: `git push -u origin vx.xx` 

- Update the `Version()` method in `src\berrybrew.cs` file with the new version

- Add the following to the `Changes` file:

    - x.xx UNREL
    
- `git commit && git push` to push these changes to the new version's branch
