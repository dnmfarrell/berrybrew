Revision history for berrybrew

1.41    2023-08-03
- Renamed `PerlOp.PerlOrphansIgnore()` to `Berrybrew.SpecialInstanceDirectories()`
and added `special-instance-dirs` hidden command (closes #353)
- `special-instance-dirs` and `orphans-ignored` now explain that no special
dirs are currently listed in the system if none are actually listed

1.40    2023-08-01
- Add file comparison routine in release script that compares a MANIFEST to
the repo directory structure, and the MANIFEST to the installer script.
This ensures that we aren't missing (or trying to include extra) files in
the installer we don't want (closes #258)
- Installer manifest check now also ensures that files installed are also
listed as files to delete upon uninstall (closes #333)
- Added "Testing the Installer" to Unit Test doc (closes #331)
- Removed all CS library includes that aren't required (closes #329) 
- Release script now updates the copyright year in all files in `doc/`
(closes #324)
- Added snapshot framework (`snapshot` command, `SnapshotInit()`,
`SnapshotCompress()`, `SnapshotExtract()` and `SnapshotList()`
(work on #185)
- Fix major previously unknown bug where if at least two custom or virtual
perls were installed, removing one would cause the writing of
`perls_custom.json` (or `perls_virtual.json`) to fail, due to the values
of the Strawbery Perl object being `null`. This raised an exception. Fix
was to set the properties of the object to an empty string if the
attempted assignment was null (eg. `data["url"] 	= perlBase.Url ?? "";`).
We now also set the `Version` property to `0.0.0` in custom/virtual
installs so the sorting of versions in `JsonWrite()` get all three version
sections (when it didn't, it too was causing an exception to be raised).
(fixes #335)
- Completed `berrybrew snapshot` (aka `archive`). Allows a user to save zip
archives of perl instances with custom file names, and re-import them
with the same, or a custom instance name (closes #185)
- Renamed `PerlFindOrphans()` to `PerlOrphansFind()`
- Add `PerlOrphansIgnore()`, returns a list (dict) of directories we shall
not classify as orphaned perl instances
- Add `berrybrew orphans-ignored` hidden command. Displays the list of
directories we won't classify as orphans (closes #334)
- Add tests to ensure that both BB::error_codes() and the Berrybrew API
`ErrCodes` enum have the same data
- `staging` environment now uses `c:\berrybrew-staging` and `testing` uses
`c:\berrybrew-testing`. This is to prevent polluting the production
storage location with orphans we have to ignore (closes #344)
- Perl instance `root_dir` (aka `rootPath`) is now an `instance` subdir of
`c:\berrybrew`, instead of putting them in the root of that dir. Prevents
orphan collisions (same for the staging and testing environments
(closes #344)
- Renamed `clean build` to `clean staging`, and `CleanBuild()` to
`CleanStaging()`
- Added `clean testing` and `CleanTesting()` to wipe the dev testing build
directory
- Added `dev\build_prod.bat`, production berrybrew build (closes #342)
- Because we now house berrybrew instance perl instances in separate
directories than the root of `c:\berrybrew`, we removed all the previous
`PerlOp.PerlOrphansIgnore()` entries. To this end, for unit testing
purposes, we now return one if the `BB.Testing` flag is set (closes #345)
- We now disallow cloning/snapshotting to any reserved special data
directories (`PerlOp.PerlOrphansIgnore()`) (closes #341)
- Added `berrybrew archives` hidden command and `BB.ArchiveList()`. Returns
the list of previously downloaded Perl instance archive/zip files
- Added `BB.ArchiveAvailable()`; Checks whether an archive file already
exists on the file system for a given perl instance
- `berrybrew download` will now only fetch an archive of a perl instance if
it isn't already on disk (closes #336)
- Renamed `root_dir` option to `instance_dir`, and added new `storage_dir`
option which is the top level environment data storage directory
(closes #351)
- Refactor all unit test scripts to conform to the new directory structure
and new/renamed option directives/values
- Small tweaks and changes to ensure UI and installer behave properly after
the major changes in this version (closes #352)
- Fix missing `$installer_script` parameter when building the installer in
`release.pl`
- Added a slew of new developer documentation on directory layouts,
modifying config, performing common development tasks etc (closes #355)

1.39    2023-07-25
- Code cleanup, minor reorganization for more efficient short-circuits
- Rename "See Also" in the README to "Other Documentation"
- Release script (dev/release.pl) now updates Copyright year in
`CONTRIBUTING.md`
- Completely renamed the various environments. 'build' is now 'staging', and
'test' is now 'testing'. Refactored/renamed all scripts, registry and
config elements, all references in documentation, test files etc. (Closes
#319)
- Create build_staging_installer.bat which wraps the original Perl script.
This is just for consistency of all the other staging build scripts being
batch files
- Rename `dev/post_release.pl` to `dev/release_post.pl`
- Add new `dev/release_cycle.pl` script, automates the preparation of the
next version's development branch (closes #266)
- Moved `Message` class to its own `messaging.cs` source file with the
namespace `BerryBrew.Messaging` (work on #184)
- Moved `StrawberryPerl` struct to its own `perlinstance.cs` source file
with the namespace `BerryBrew.PerlInstance` (work on #184)     
- Removed the deprecated `berrybrew upgrade` command along with its
corresponding `Upgrade()` method in the API
- Moved all Path functionality to new `src/pathoperations.cs` file with
namespace `BerryBrew.PathOperations` and class `PathOp`
- `s/PerlarchivePath/PerlArchivePath/` in method name and call (fixes #327)
- Moved all Perl functionality to new `src/perloperations.cs` file with
namespace `BerryBrew.PerlOperations` and class `PerlOp` (work on #184)
- Updated entire API doc with relevant parameter and permissions for each
class method after splitting libraries (work on #184) (closes #326)
- Added "Testing the UI" section in Unit Testing doc (closes #328)
- Added `berrybrew clean build` with `CleanBuild()` to clean the staging
build directory from the repo (fixes #322)
- Add StrawberryPerl struct instantiation parameters and property lists to
the API doc (closes #325)
- Staging build script now updates the recommended perl instance to the most
recent available version
- Finished separating out various chunks of functionality from the main
source file and spread it across logical files to contain like
functionality (closes #184)
            
1.38    2023-07-21
- Remove the perltricks.com link from David Farrell acknowledgement
(Fixes #313)
- Properly handle thrown exception when trying to install with an invalid
Perl version (Fixes #316)
- Added updated perls.json file that includes 5.36 and 5.38
- Fix issue where `berrybrew use` was exporting wrong module list. We now
don't allow exporting modules in a temp instance (by using
BERRYBREW_TEMP_INSTANCE env var) (Fixes #312)
- dev\release.pl release script now updates Copyright year in LICENSE file
(closes #314)

1.37    2023-07-20
- When launching an instance with Powershell, we now change into the user's
home directory
- Added 'download' hidden command (with Download() API method). Downloads
one, or all version(s) of Strawberry Perl portable
- Added `dev/generate_github_releases_json.pl` as a helper to keep the
perl version's `releases.json` file up-to-date
- Added "doc/Update Releases JSON.md" documentation to help with the
releases update process

1.36    2021-10-31
- Modify exit code test script to get exit code list from the BB test object
- Add note on how to get exit command output to STDOUT in berrybrew doc
- Add ability to use Powershell when using a Perl (closes #245)
- For displaying the version in the UI, we now get the run mode from
bb.Options() as opposed to an environment variable
- Add check in BB::check_test_platform() to ensure that no berrybrew perls
are already configured before executing tests. Added call to it in
t\run_tests.pl (closes #308)
- Added check in release script check_readme() to actually compare the SHA1
checksums in the README to the actual checksums of the installer and zip
archives (closes #307)

1.35    2021-10-08
- Add 'assoc' short hand alias for the 'associate' command (closes #294)
- In BitSuffixCheck() we now anchor the perl version to ensure the user is
specifying a legitimate version as opposed to a custom one (closes #271)
- `berrybrew off` registry security violation when run with a non-admin
user is now handled properly (fixes #299)
- Updated registry path information in uninstall doc section of README
(fixes #298)
- Removed 'upgrade' command. We now use only the installer or git to
upgrade berrybrew (closes #295, closes #296)
- Finally fixed the double-backslash regex replacement issue we were having
in the unit test suite setup scripts
- Fix typo in CleanModules() section of API doc (fixes #302)
- Added BB::check_test_platform(). All tests call it and gracefully exit
with a warning that dev\build_tests.bat needs to be run if the test build
directories are missing
- Fix quoting issue in argument parameter when setting file association
string. This was causing an empty argument when none were sent in, and
only a single one when multiple were sent in (fixes #303)

1.34    2021-05-25
- Added dev/build_installer.pl; builds a development installer using the
dev/create_build_installer.nsi NSIS script
- Build scripts now modify the 'run_mode' config option directive to 'build',
and set BB_RUN_MODE=build env var. UI will reflect run mode in title
- Test scripts now set run_mode config option to 'test'
- UI refreshes after each command is completed instead of minimizing and
redrawing on tray icon click (closes #283)
- Added 'Fetch' button in UI to fetch the latest available Perls
(closes #286)
- Fix 'Hidden Features' link in Unit Test document (fixes #290)
- Fix broken path to test directory in Unit Test doc (fixes #291)
- Add section index for Unit Test and Create a Release docs (closes #289)
- Clarify documentation on 'exec' command (closes #242)
- Fix process order in release script (create Changes markdown before
creating zip file) (fixes #288)
- PerlGenerateObjects() now checks if OrderedDictionary perlName key exists
before compiling the list (work on #281)
- PerlsInstalled() now calls PerlGenerateObjects() to regenerate the list
of installed Perls, so that the UI can be refreshed without needing a new
instance of berrybrew (work on #281)
- Added 'clone' functionality to the UI (closes #281)
- Fix issue where if berrybrew is managing file association, running a
script without preceeding it with "perl" would fail if the directory path
had a space in its name (fixes #285)
- Minor corrections in escaping backslashes in test setup/configuration

1.33    2021-02-12
- Added berrybrew version to the UI (work on #260)
- Fix issue where UI wasn't updating/redrawing after command line changes to
berrybrew/Perls
- Options() now defaults the two string parameters to null as opposed to
empty strings. This allows us to set empty string values. Previously,
FileAssoc() wouldn't work correctly with an empty string value
- UI now provides access to debug, warn_orphans, file_assoc and
windows_homedir options (closes #260)
- We will now default to 64-bit perls (ie. '_64') if the suffix is omitted.
Added BitSuffixCheck() to perform this task (closes #268)
- UI now allows a user to use an installed Perl. We spawn a new CLI window
from the GUI (closes #270)
- UI now allows user to spawn a CLI to the currently in-use Perl
(closes #273)
- UI now has an 'Off' button, allows disabling all berrybrew perls
- When UI started new 'use' shell, it didn't have access to the %PROMPT% env
var, causing the full prompt from being displayed. Changed it to use $P$G
instead (fixes #276)

1.32    2021-01-30
- Added 'warn_orphans' config/option directive, default false. Prevents
the orphaned perls warning, except when 'list' is called (closes #251)
- Removed 'env' requirement for managing file associations. We now
dynamically update the ftype in the registry with the absolute path to
the berrybrew perl currently in use (fixes #244)
- Switch() now updates FileAssoc() (work on #244)
- Removed env.exe and related libraries from the project
- Fix issue in installer where if berrybrew is already installed, it was
trying to install into the ../bin/ directory (fixes #250)
- Release script now adds the berrybrew version properly to the install script
(fixes #254)
- Installer aborts if trying to install the same version
- Options() now does a significantly better job handling the registry.
User/Admin access to the registry has been separated and handled properly
- Fix issue in FileAssoc() where an exception was raised if we were trying
to set 'file_assoc' to the same value that already exists
- New options added to berrybrew configuration are now merged into the registry
on an upgrade, and existing option values are not changed (fixes #257)
- Release process automatically updates available Perl list (closes #261)
- Changed all documentation links from absolute to relative (closes #259)
- Modified OpenSubKey() in Options() to allow write access
- Changes date is now updated by release script (closes #265)

1.31    2021-01-21
- Fix issue where on first-run BaseConfig(), if the system didn't have
a file association set for the .pl file type, we'd attempt to send
in a null value to Options() for the file_assoc which threw an
InvalidArgument exception (fixes #237)
- Added bracing around all conditional and loop structures (if, else,
foreach etc). This prevents adding additional statements that fall
outside of the single-line approach
- All methods and commands now exit with a proper error code
(closes #236)
- All paths now exit with an appropriate success or error status
(closes #239)
- Added 'error' command, translates error codes to their names
- Added a display statement to inform the user that we're attempting
a clone
- Install() now exits with failure and displays an error message if
trying to install an already-installed Perl
- Added 'error-codes' hidden command, returns all valid exit status
code values
- Made 'bypassOrphanCheck' object property public so we can set it
from within the berrybrew binary ('error-codes' specifically)
- Added 'hidden' command to berrybrew, displays all of the hidden
commands
- Moved all of the hidden command details to the berrybrew doc, and
left a reference to them in the README
- Added Exit(), a wrapper for Environment.Exit(), and 'exit' command
for testing
- Added 'trace' feature. When set, we display the entire stack trace
to `STDERR`
- Added 'status' feature. Displays the exit status code and its name
when exiting the program
- Added tests in release.pl to verify that SHA checksums and version
information in README was updated correctly
- Bumped SSL security to TLS1.2 for the WebClient() (fixes #248)
- Remove Exit() from install click in UI, and modified Available()
to Msg.Print() from Msg.Say() so that it doesn't exit the program.
The exit was causing the UI to exit as well (fixes #243)
- error-codes now displays the exit code number as well as its name
- Compress NSIS directory; Can be extracted when required to build the
installer
- Prevent FileAssoc() from exiting when not running as administrator
(fixes #246)
- Fix issue where the name of the perl version wasn't being displayed
if that version was already installed
- Disabled file association checkbox in installer due to issue #246)

1.30    2019-12-25
- updated docs to reflect ability to remove berrybrew using
Add/Remove Programs (closes #209)
- berrybrew Perls can now be managed through the System Tray Icon
(closes #210)
- remove examples/synopsis section of README, and refer to berrybrew
doc instead (closes #211)
- berrybrew-ui is now stopped if the installer senses an upgrade is
occurring (closes #212)
- added AvailableList() to the API doc (closes #213)
- fix issue in UI where Switch was duplicating PATH env var
(fixes #218)
- configuration information is now stored in the Windows Registry
(closes #215)
- added Options(), and 'berrybrew options', used to get/set the
various configuration option values (closes #216)
- Strawberry Perl objects now have a new "newest" attribute, to track
if an instance is the most recent point release of a major version
- 'fetch' no longer has the 'all' option, it now performs its duties
with the 'available' command instead. All Perl versions are now
written to the `perls.json` file (closes #219)
- Perls that were installed previous to a new point release of a major
version are now not interfered with and are no longer cast as orphan
and registered as custom (fixes #217)
- added 'env.exe' as a binary, and updated installer script and build
tools to include it
- added FileAssoc() and 'berrybrew associate'. Allows managing and
unmanaging Perl file associations (work on #15)
- we now allow the managing of .pl file association; we set it so that
the first perl found in PATH is used (closes #15)
- split out build scripts, so each element (bb, ui, api) each have
their own (this prevents building all elements all the time)
- dev and test builds are now identified in the class by the assembly
directory, and we configure things accordingly instead of using env
vars (closes #224)
- added OptionsUpdate() and 'options-update'. Updates the registry
configuration with any newly added config directives (closes #221)
- dev and test builds now reliably create and use their own registry
section
- installer now removes registry configuration on uninstall, and it
along with 'upgrade' make a call to 'OptionsUpdate()' (closes #225)
- installer now has option to allow managing of .pl file association
(closes #227)
- added ability to change Perl instance root directory in the
installer (closes #208)
- added "Run UI at startup" option in installer
- added bb.exe, a short-form for the full berrybrew command
(closes #226)
- update Configuration document with pertinent information regarding
the new registry configuration system (closes #222)
- full documentation review and updates (closes #223)
- installer no longer requires on "PROGRAMFILES", which allows the
uninstaller to operate on the correct install directory (closes #228)
- uninstaller now removes top-level install directory (closes #229)
- fix issue in OptionsUpdate() where we were updating registry values
where we shouldn't be, due to not scoping an if() statement
(fixes #230)
- added SHChangeNotify() to API, to send an icon refresh if the .pl
file association changes
- fix issue in PathRemoveBerrybrew() where if the $INSTDIR in the
installer had a lowercase drive letter, the PATH would not be
removed on uninstall (fixes #233)
- added 'force' arg to OptionsUpdate() along with `options-update-force`,
to load all config file options into the registry (closes #232)

1.29    2019-12-08
- add missing closing parens on 'remove' if a Perl isn't installed
(fixes #196)
- added "berrybrew info" command, and BB.Info() method. retrieves and
displays various installation information (paths etc) (work on #193
and #194)
- PathScan() now accepts a string to search %PATH% with, as opposed to
a regex. All methods that add/remove from %PATH% now use BB object
path attributes so we always know we are looking for/working on the
absolute correct entry (fixes #193; closes #194)
- if 'fetch' is called w/o administrator privileges, we gracefully
catch this and inform the user instead of crashing (fixes #198)
- installer now uses nsExec::Exec functions as to ensure the command
line windows we need to open are not visible (closes #200)
- implemented logic in installer to identify whether there's an
existing berrybrew, and whether the install will overwrite or sit
beside it (closes #195)
- update test data with Perl 5.30.1 data
- added 'berrybrew-refresh' batch script to allow updating PATH env
var in current window (saves from having to open new cmd windows all
the time
- 'clean modules' and 'clean temp' now check for directory existence
before processing to avoid uncaught exceptions (fixes #202)
- added 'register_orphans' hidden command
- added calls to PerlUpdateAvailableListOrphans() in Upgrade() and
added call to 'berrybrew register_orphans' in the installer. This
ensures that if 'perls.json' is updated, the old Perl instances will
be registered and available (fixes #199)
- if source Perl for clone operation isn't available, exit gracefully
(fixes #205)

1.28    2019-11-22
- modified PathAddBerrybrew() to insert berrybrew's path to the start
of the PATH as opposed to the end of it
- modified the installer so that it asks the user if we can try to
disable a previous version

1.27    2019-11-22
- we now have a self-extracting installer!
- fix issue where 'list' wasn't showing the 'virtual' tag (fixes #186)
- aded installer NSIS script and logic to automatically build the
binary during the release process
- 'remove' now displays notification of its actions (closes #190)
- fix label ordering issue in custom_install test file
- update release script to automatically update version numbers in the
installer script (closes #189)

1.26    2019-09-02
- added 'berrybrew virtual' command; allows using external perls (eg.
installed system Active State) from within berrybrew. Useful for
bypassing berrybrew switched perls without having to disable berrybrew
entirely
- added new 'berrybrew switch x.xx quick' argument, allows switching
to a new perl without needing to open a new console window (some
functionality may suffer until a new window is actually opened though)
- modified calls to String.Compare() to specify a string array instead
of just a string for .Net 4.6 compliance
- modified calls to String.Split() to specify a char array instead of
just a char for .Net 4.6 compliance
- updated dev\release.pl to create a Changes markdown file for ease
of online viewing

1.25    2019-08-30
- fix typo in API doc (fixes #163)
- added new testing environment variables (set in t\setup_test_env.bat)
which set up some test-specific flags to shim the system where
necessary
- reworked unit test platform; the "dev build" and "test" infrastructures
are now completely separate. This eliminates issues where in some
cases, the two environments overlapped each other
- reworked how the "modules" infrastructure works. We now create the
directory on 'import' if it doesn't exist, and produce more sound
warnings in certain circumstances (fixes #169)
- "clean modules" now leaves any custom export lists in place, and
operates at file level, not simply remove the entire directory
(closes #162)
- added new config file directive, "windows_homedir", which allows a user
on a global basis to use the Portable homedir (default), or the
Windows homedir. This happens during 'berrybrew install' (closes #121)

1.24    2019-08-29
- removed confirmation dialog when running "config" as to allow for
unattended installations (closes #173)

1.23    2019-05-26
- fixed issue in README where some external doc links were pointing to
the wrong branch (fixes #147)
- removed the "update dnmfarrell repo" section from README; it's been
moved to the "create a release" doc (closes #146)
- "Compile your own" link wasn't directing to the proper doc
(closes #148)
- added "dev" and "all" to the "clean" operation (closes #149)
- added t/99_clean.t for testing the "berrybrew clean dev" feature
(closes #151)
- added new "test" argument to "berrybrew". This is for developers to
tell the system we're in unit testing mode to shim the system where
necessary
- added an exception handling routine in PerlFindOrphans() to catch an
error in certain cases when running "berrybrew clean dev"
- t/95-remote.t no longer cleans up the test directory, that's left to
t/99-clean.t now
- slight modifications to the testing infrastructure and documentation
instructions
- rename "Synopsis" to "Examples" in README (closes #124)
- corrected issue in ExecCompile(), where if we're using --with to exec
with only one perl instance, it wasn't filtering correctly
- added ExportModules(), saves the current perl's module list for import
later in a different perl (work on #150)
- added CleanModules() with new `berrybrew clean modules` to clean up
the exported module list directory
- added ImportModules(), ImportModulesExec(), modules export and
import (work on #150)
- update all documentation with the new "modules export/import"
functionality (closes #150)
- added new "currentperl" hidden command which displays which Perl
is currently in use. Changed "PerlInUse()" from private to public to
facilitate the new feature
- bumped perls.json to include the new 5.30 Perl release
- several other minor issues closed (mostly small doc fixes etc)

1.22    2018-11-05
- clarifications and updates to the "Create a Release" doc (closes #137)
- major, sweeping code refactoring: method privileges, exception
handling, variable renaming to conform to C# standards etc
- update API doc with all relevant changes due to the code refactoring
- update PerlUpdateAvailableListOrphans() details in API doc
(closes #140)
- added PerlsInstalled() method to API doc (closes #139)
- added PathGetUsr() method to API doc (closes #138)
- when running a dev build, the Perl instance and berrybrew temp
directories are now located under a "build" dir, as opposed to the
"test" directory as it was previously. This separates the unit testing
platform from the development build one (closes #114)
- added "developed using" section in README (closes #141)
- add new "dev\build_tests.bat" script that the "t\test.bat" script
calls to set up the unit test environment. We used to call
"t\build.bat", but since changing the dev build system, we needed to
separate the functionality (fixes #143)

1.21    2018-11-04
- updated CONTRIBUTING doc (closes #128)
- moved the "Compile your own installation", "Create a dev build", and
"Create a release" README sections to their own documentation files.
Also greatly expanded on the "Create a release" doc to include steps
to complete the entire release cycle (closes #126)
- added a config directive index at the top of the configuration doc
file (closes #129)
- integrate hurricup's PR#132, proxy of error code if 'exec' is run
in single mode (prep for integration with Camelcade Perl5 plugin)
- test changes in PR#132, and modify docs accordingly (closes #133)
- the "test" directory is no longer seen as an orphaned Perl install
(closes #134)
- add note in "create a dev release" doc that all Perl instances are
located under the "test" directory (closes #135)

1.20    2018-11-02
- fix improper indentation of the Off() method in berrybrew.cs
(fixes #120)
- add note in Unit Testing doc to update the t/data/available.txt and
t/data/custom_available.txt files if Perls available has changed
before running tests (fixes #118)
- rearrange the Installation and Configuration sections in README
(closes #122)
- replace magic number in CheckName() with a private const int
(closes #119)
- update examples with most recent Perl versions in README (SYNOPSIS)
(closes #123)
- fix a missed exception handling in List() when no Perls are installed
(fixes #117)

1.19    2018-11-01
- integrated hurricup's PR#107, which includes changes required in
preparation for berrybrew's inclusion within the Camelcade Perl5
intelliJ IDEA plugin
- added notes regarding version bumping and updating in README
(closes #108)
- add new "Create a development build" section in README (closes #109)
- fix typo in "exec --with" section in SYNOPSIS (fixes #115)
- added "list" command to docs (bin/API) and README (closes #111)
- add "Uninstall" section in README (closes #106)

1.18    2018-02-16
- Perls listed with "berrybrew available" are now listed in numerical,
descending order (closes #101) (thanks @shawnlaffan for the
report!)
- fixed issue where the orphaned Perls that were installed prior to
using "fetch" weren't being registered as custom correctly (required
two calls to "fetch"). To fix, added new
PerlUpdateAvailableListOrphans()(closes #102; closes #99) (Thanks
@pryrt for the report!)
- added ability to fetch every single Perl version Strawberry has to
offer with the new "all" argument to "berrybrew fetch" (closes #100;
closes #103) (thanks @pryrt for the report!)
- fix issue where we were missing an exception if trying to register
the same custom version of Perl more than once (fixes #104)

1.17    2017-10-04
- task information displayed during install operation is now in logical
order (fixes #95)
- fix issue where when running the "build" version (ie. unreleased), it
was using the same Perl installation directory as the "live" version.
In the build.bat script, we now update the config.json file
accordingly to ensure we're using two separate install locations.
(fixes #97)

1.16    2017-08-03
- UseInNewWindow() and UseInSameWindow() API calls were incorrectly
listed as "public" in the API doc which are actually "internal", and
UseCompile() was "internal" and should be "public"
- gracefully catch a download problem and report to the user
(closes #63)
- fix small typo in README (closes #89)
- gracefully let user know that berrybrew can't be installed in
c:\berrybrew on "config" (fixes #55)
- consolodated version info. Only hardcoded in the berrybrew.cs file.
Removed it from messages.json, and updated release.pl script to bump
the version in the README at release time (closes #86)
- replaced calls, where appropriate to PathGet() instead of
PathRemovePerl(process=false) for clarity (closes #91)

1.15    2017-07-28
- fix doc links in API, and resized headings in berrybrew doc
(closes #84)
- 'register' command now outputs a success message to the console upon
successful registration (closes #79)
- use will both output each Perl specified if it's not installed, then
warn and exit if none of the specified versions are installed
(closes #83)
- bumped versions of Perl in doc examples to reflect current
availability

1.14    2017-07-28
- code cleanup
- bumped 5.24.1 to 5.24.2 in perls.json (prod and test)
- new "use" feature, allows spawning into a different version in the
existing cmd window, or a new ones. Multiple perls can be specified
in a single run. This feature acts similar to Perlbrew's "use"
feature, in that upon exit, you'll automatically be put back to your
previously "switch"ed version (@pryrt PR #80)

1.13    2017-07-15
- updated license information (closes #75)
- updated perls_available.json to include 5.26
- we now remove the "test" directory after all tests complete
(closes #76)
- correct markdown formatting in Configuration doc (closes #73)
- small README correction (closes #71)
- added new "register" command, provides ability to register perl
installs that happened outside of berrybrew (closes #70)
- add check to ensure we don't duplicate custom registrations
- massive unit test framework upgrade (thanks pryrt!). There's still
more work to be done here, but it's already much better
- changed the bracing syntax. I dislike the opening brace on a separate
line

1.12    2017-03-31
- fix local links in the markdown in the documentation
- added "berrybrew fetch" (PerlUpdateAvailableList()). This function
fetches the releases.json file from the Strawberry Perl website, and
updates our data/perls.json file with the most recent patch version
of each Perl version for 64bit, 32bit and PDL, where each is
available (work on #62)
- updated all documentation with the new 'fetch' feature
- added exception and error handling in PerlUpdateAvailableList() for
both the web fetch of the JSON file, and the reading of the JSON
itself
- enhanced exception handling for 'fetch'. We now print out the full
exception if in debug mode
- implement orphan handling for 'fetch'. If there are any orphans, we
will not proceed any further
- if there are perl instances orphaned after 'fetch' we now put them
into the perls_custom.json file so they are still usable. They will
appear as [custom] installs
- upgrade now backs up config files, but only restores the
perls_custom.json file. The user will have to manually merge in any
custom changes they had from the backup into the updated config files

1.11    2017-03-30
- replaced Ionic.Zip with SharpZipLib as the former failed to
consistently extract the zip files completely
- updated 5.10.1_32 from 5.10.1.2 to 5.10.1.5
- updated SHA1 checksums for various instances

1.10    2017-02-12
- fixed issue where the updated perls weren't being reflected in the
perls.json file in the zip archive
- added 'unconfig' help menu listing
- updated SYNOPSIS with more recent versions of Perl (closes #58)
- a request was made to include the PDL versions, but I'm holding off on
that for the time being, as I'm focusing my efforts on the automatic
list update feature (PerlUpdateAvailableList() in the API, I haven't
decided on the CLI command name yet)

1.09    2017-02-03
- added 5.24.1_xx and 5.22.3_xx

1.08    2016-11-11
- doc cleanup/tidying
- PerlFindOrphans() now ignores the '.cpanm' directory
- version now hardcoded in the library

1.07    2016-11-11
- fix issue where 5.22.2_32 wasn't downloading the portable zip file
thereby the checksum didn't match (reported by 'atcroft') (fixes #53)
- test updates/fixes
- doc updates/cleanup (closes #51 & #54)

1.06    2016-07-26
- fixed issue where berrybrew config fails if the perl Root dir doesn't
exist (fixes #46)
- added "unconfig" command to remove berrybrew from PATH
- updated API and berrybrew docs (closes #48)
- removed release.sh, modified release.pl to be cross-platform
- cleaned up zip file creation
- better handling of config files for git push
- perls_custom.json file is no longer included in an install, it is
created on the fly if it doesn't exist. This prevents it from being
overwritten during an upgrade
- changed zip libraries to Ionic.Zip, for easier usage when we
implement backup/restore functionality
- added 'upgrade', safely performs a berrybrew upgrade
- added new attributes 'confPath' and 'binPath'. 'installPath' is now
the actual root dir of the repo

1.05    2016-07-18
- updated dev/release.pl to put default config files in place before
commit/zip
- added dev/post_release.pl to restore the backups
- 'exec' no longer will process perls with either 'tmpl' or 'template'
in their name
- new config option, 'custom_exec' (default is false) will skip over
Custom (cloned) Perls on 'exec'. Set to true in the config file to
exec on them
- added exception handling for when a clone fails
- tests now perform their work in a test subdir of berrybrew, as to not
accidentally remove custom installs as orphans

1.04    2016-07-17
- added documentation in doc/ for berrybrew, the API and configuration
- fixed issue with name length, max is now 25 chars for custom perls
- added CheckName(), validates the name of a custom perl
- added release.pl and removed release.sh

1.03    2016-07-16
- added debug support within API. Set within the data\config.json file.
For the API, call ``obj.Debug = bool;'', for the CLI, ``berrybrew debug ...''
- major code restructuring
- cleaned up Exec() PATH configuration (closes #10, #12 and #23)
- removed Dir class, path info now stored in member variables
- added doc/
- added unit test doc
- on destruction, we now check for orphaned perl installs, and warn the user
(closes #16)
- StrawberryPerl objects are now only generated once, and stored within the
BerryBrew object
- removed several unused imports
- sub command help for the commands that have sub commands
- 'clean' removes temp files by default, but can also clean orphaned perls with
the 'orphan' subcommand
- consolodated ParseJson() and ParseConfig() into the former
- completed most of the work on the auto updating of the Perls list
- renamed ParseJson() to JsonParse(), and added JsonWrite()
- added 'clone', allows cloning of instances with custom names (useful for
creating perl installation templates, and snapshots)
- more tests

1.01    2016-07-13
- added RemoveFilesystemAttributes(), removes RO flag on all files
in perl install dirs before deleting. C# can't delete them otherwise
- the API has been separated out into a DLL for re-use and easier
testing. Source file for the library is berrybrew.cs. The console app
resides in bbconsole.cs
- changed to a more Perlish version numbering system

20160703 2016-07-12
- fixed issues where %VARIABLE% based PATH entries weren't being
expanded properly. We now set the registry value to REG_EXPAND_SZ
type instead of REG_SZ
- added initial set of basic Perl unit tests

20160702 2016-07-11
- added 'clean' option, removes all archive zips of perl (closes #5)
- consolodated directory locations into a class
- moved almost all console messages to data/messages.json
- moved perls.json to data/
- added exception handling for missing and/or malformed json files
- variable based PATH env vars no longer get expanded and clobbered (fixes #10)
(reported by PeterCJ (pryrt)
- add preliminary ability to change Perl installation directory (closes #11)

20160701 2016-07-07
- fixed bug where PATH wasn't being set correctly when switching perls
- added release.sh script, performs all tasks related to a release

20160603 2016-06-24
- added perl.Paths to the struct, holds all three paths in a list
- re-worked the setting of PATH in exec subshells

20160602
- added 'off' feature, we now play nice with ActiveState and Strawberry
Perl installations
- moved from using the User PATH env var to Machine(System) PATH, so
that 'off' works correctly

20160501 2016-05-13
- separated fork from dnmfarrell's original
- added --with flag for exec, like perlbrew
- added better ENV handling on spawned sub-shells in exec
- added ability to load perls available through an external perls.json
file, located in the repo root directory
- tidied up README
