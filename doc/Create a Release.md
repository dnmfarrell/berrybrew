## Create and Publish a New Release

- `git checkout` the branch that's to be merged into `master`

- `git pull` to ensure we're up-to-date

- If configuration directives have been added, removed or modified in the
`data\config.json` that are part of the new release, copy that file to the
`dev\data\` directory

- Ensure all unit tests pass per 
[Unit testing](Unit%20Testing.md)

- Execute the `perl dev\release.pl` script, which:

    - Compile the `berrybrew.exe` binary and `bbapi.dll` API library
    - Compile the `berrybrew-ui.exe` UI binary
    - Collect the JSON configuration files from the `dev\data` directory
    - Build the bundled zip archive, and places it into the `download/`
    directory
    - Updates the MSI installer script with berrybrew and perl version info
    - Creates the MSI installer program
    - Perform SHA checksum tasks on the new zip archive and MSI installer
    - Update the `README.md` file with the zip and installer's new SHA sum
    - Update the `README.md` file with the new version from the API's
    `Version()` method
    - Sets the date in the Changes file for the release version      
    - Creates a Markdown version of the Changes file
    
- If you had any custom configuration files in place before running the
`dev\release.pl` script, run `perl dev\post_release.pl` to put them back to
their proper location

- On a clean platform, run the self-extracting installer from the `download/`
directory, and ensure that both the `berrybrew`, and Perl versions are correct

- `git commit -a -m "release x.xx"`

- `git push`

- Ensure everything looks proper within the newly published release branch

- In Github, create a new Pull Request from the release branch, and merge it
into the master branch

- Check out master and ensure everything appears proper within the master branch

    - `git checkout master`
    - `git pull`
    - `bin\berrybrew version` (should be the updated version number)

- Tag the new master branch as a release, and push it

    - `git tag vx.xx`
    - `git push --tags`
    
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

- Update the `Version()` method in `src\berrybrew.cs` file with the new version

- Add the following to the `Changes` file:

    - `x.xx UNREL`
   
- Commit the changes:

    - `git commit -a -m "bumped to ver x.xx`    

- Push the new branch to Github: 

    - `git push -u origin vx.xx` 
    
## Updates for the Next Release

- Check out the version branch: `git checkout vx.xx`    

- Ensure it's up-to-date: `git pull`

- Create a working branch: `git checkout -b some_feature`, and perform your work

- Push the new branch after making commits: `git push origin some_feature`

- After all tests pass, create a Pull Request for the feature branch into the
new version branch

- Check out the new version branch `git checkout vx.xx` and update: `git pull`

- Run all unit tests

- Repeat this Release Cycle
