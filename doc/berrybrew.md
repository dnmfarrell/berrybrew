## berrybrew Usage

#### Command List:

- [available](#available)
- [clean](#clean)
- [clone](#clone)
- [config](#config)
- [exec](#exec)
- [fetch](#fetch)
- [install](#install)
- [off](#off)
- [register](#register)
- [remove](#remove)
- [switch](#switch)
- [unconfig](#unconfig)
- [upgrade](#upgrade)
- [help](#help)
- [license](#license)
- [version](#version)

#### Command Usage

##### debug

Usage:  `berrybrew debug <command> [options]`

This command preceeds all others, and can be used in conjunction with
all other commands. Depending on the scenario, it will print out verbose
debugging information.

##### available

    berrybrew available

Takes no options, displays a list of all available Perl versions, which
includes installed and custom versions. A shortened example:

    The following Strawberry Perls are available:

            5.24.1_64       [installed]
            ...
            5.22.3_64
            5.12.3_32
            5.10.1_32       [installed]
            template-5.24   [custom] [installed]
            unit_test-5.18  [custom] [installed] *

    * Currently using

##### clean

Usage:  `berrybrew clean [option]`

By default, if either `help` or a subcommand are not specified, we'll
simply delete the downloaded Perl installation zip files from the temporary
directory.

###### clean options

    help        Displays the subcommand help screen
    temp        Deletes all Perl installation zip files
    orphan      Deletes any directories in the Perl install directory that
                berrybrew hasn't registered
    all         Performs both a 'temp' and 'orphan' clean

##### clone

Usage: `berrybrew clone <version> <name>`

Makes a copy of an installed `version` (as seen in `berrybrew available`), 
and copies it as an exact duplicate named `name`. The new named Perl will 
appear in `berrybrew available`.

Use cases:

- configuring an instance of Perl with all of your favourite modules, and
cloning it for use as a template to easily reproduce your favourite 
configurations

- making snapshots of Perl installations before making changes, to provide
an immediate restoration point

- creating standalone, project-specific Perl installations, that can be
snapshot and re-cloned

- creating build instances for your own modules

##### config

Takes no arguments. Simply sets up your `PATH` environment variables so that
`berrybrew` can be found without specifying the full path to the binary.

##### exec

Usage:  `berrybrew exec [options] <command>`

Executes the command and its arguments found in `<command>`, and
executes it across all installed Perl instances, less ones that have
either `tmpl` or `template` in the name.

Also, by default, we don't execute on custom (cloned) instances. Set
`custom_exec` to `true` in the config file to `exec` on those as well.

###### exec options:

    --with version,version,...  Run only on the listed versions

##### fetch

Usage:  `berrybrew fetch`

Pulls the JSON list of available Strawberry Perl instances from the Strawberry
website, and puts them into the `data/perls.json` file. Any updates will be
available immediately with `berrybrew available`.

##### install

Usage:  `berrybrew install <version>`

Installs a single Perl version as seen in `berrybrew available`, and makes it
available for use.

##### off

Usage:  `berrybrew off`

Disables all `berrybrew` Perl installations. If you have a Strawberry or
ActiveState system Perl installed, it'll be used until you `berrybrew switch`
back to a `berrybrew` controlled Perl.

##### register

Usage:  `berrybrew register <directory>`

Registers a custom installation within the Perl instance directory that was
placed there outside of `berrybrew`.

This allows you to copy in other portable Strawberry Perl instances from
elsewhere on your system, or from remote systems and have them operate under
the `berrybrew` umbrella.

##### remove

Usage:  `berrybrew remove <version>`

Removes a single version of Perl, as seen in `berrybrew available`.

##### switch

Usage:  `berrybrew switch <version>`

Sets the verion of Perl as seen in `berrybrew available` to the default
system Perl. This change is persistent. Use `berrybrew off` to disable the
switched-to Perl, or use `switch` to change to a different one.

##### unconfig

Usage:  `berrybrew unconfig`

Removes berrybrew's binary directory from the `PATH` environment variable.

##### upgrade

Usage:  `berrybrew upgrade`

Creates a `backup_timestamp` backup directory in the repository root directory,
copies the live configuration files from `data` directory, performs a
`git pull`. All configuration files less the `perls_custom.json` file are
overwritten with any new changes. It is up to the user to manually merge in any
custom changes to the other configuration files from the backups into the new
files in `data/`.

##### help

Usage:  `berrybrew help`

Displays a summarized view of the available commands.

##### license

Prints the `berrybrew` license to `STDOUT`.

##### version

Usage:  `berrybrew version`

Displays the current version of the `berrybrew.exe` binary and `bbapi.dll`
library.

&copy; 2017 by Steve Bertrand
