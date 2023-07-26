# berrybrew Usage

### Command List:

- [debug](#debug)
- [associate](#associate)
- [available](#available)
- [list](#list)
- [clean](#clean)
- [clone](#clone)
- [config](#config)
- [exec](#exec)
- [fetch](#fetch)
- [info](#info)
- [install](#install)
- [modules](#modules)
- [options](#options)
- [off](#off)
- [register](#register)
- [remove](#remove)
- [switch](#switch)
- [unconfig](#unconfig)
- [use](#use)
- [virtual](#virtual)
- [help](#help)
- [license](#license)
- [version](#version)

### Hidden Commands

Here are [certain features](#hidden-command-list) that may be useful by the developers and
maintainers of this software. 

- [currentperl](#currentperl)
- [download](#download)
- [error](#error)
- [error-codes](#error-codes)
- [exit](#exit)
- [info](#info)
- [options-update](#options-update)
- [options-update-force](#options-update-force)
- [orphans](#orphans)
- [register-orphans](#register-orphans)
- [test](#test)
- [trace](#trace)
- [status](#status)

### Command Usage

#### debug

Usage:  `berrybrew debug <command> [options]`

This command preceeds all others, and can be used in conjunction with
all other commands. Depending on the scenario, it will print out verbose
debugging information.

#### associate

    berrybrew associate [option]

View, set or revert `.pl` file association on the system.

Note that you can use `assoc` as a short hand alias for `associate`.

##### associate options

    set     - Allow berrybrew to manage the association
    unset   - Revert the association back to what it was previously
       
If no option is sent in, we'll simply display the current association.

#### available

    berrybrew available [option]

Displays a list of available Perl versions, which includes installed and custom
versions. A shortened example:

    The following Strawberry Perls are available:

            5.24.1_64       [installed]
            ...
            5.22.3_64
            5.12.3_32
            5.10.1_32       [installed]
            template-5.24   [custom] [installed]
            unit_test-5.18  [custom] [installed] *

    * Currently using

If the optional command is set to `all`, we'll list all available versions.
Otherwise, we list only the most recent point release for each major Perl version.

#### list

    berrybrew list
    
Takes no options, displays a list of the currently installed Perl instances:

    berrybrew list
            5.26.2_64
            5.10.1_32
            
#### clean

Usage:  `berrybrew clean [option]`

By default, if either `help` or a subcommand are not specified, we'll
simply delete the downloaded Perl installation zip files from the temporary
directory.

##### clean options

    help        Displays the subcommand help screen
    all         Runs all clean operations
    build       Deletes the developer's staging build directory in the repo
    dev         Deletes all developer data (testing and staging directories)             
    temp        Deletes all Perl installation zip files
    module      Deletes the exported module list directory                
    orphan      Deletes any directories in the Perl install directory that
                berrybrew hasn't registered

#### clone

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

#### config

Takes no arguments. Simply sets up your `PATH` environment variables so that
`berrybrew` can be found without specifying the full path to the binary.

#### exec

Usage:  `berrybrew exec [options] <version> <commands>`

Executes the command and its arguments found in `<commands>`, and  executes it across
all installed or only specified select Perl instances, less ones that have
either `tmpl` or `template` in the name.

Also, by default, we don't execute on custom (cloned) instances. Set
`custom_exec` to `true` in the config file to `exec` on those as well.

##### exec options:

    --with version,version,...  Run only on the listed versions

##### exec exit status

The exit status for the `exec` command will be that of the process that you're
executing. If running with multiple Perls and any of them fail, the status will
be that of the failed process, regardless if all others succeed.

#### fetch

Usage:  `berrybrew fetch`

Pulls the JSON list of available Strawberry Perl instances from the Strawberry
website, and puts them into the `data/perls.json` file. Any updates will be
available immediately with `berrybrew available`.

#### info

Usage: `berrybrew info <option>`

Retrieves and displays specific implementation and installation details
regarding the `berrybrew` installation itself.

Run `berrybrew info` to get a list of the valid options.

#### install

Usage:  `berrybrew install <version>`

Installs a single Perl version as seen in `berrybrew available`, and makes it
available for use.

#### modules

Usage: `berrybrew modules <command> [option]`

Allows you the ability to export the currently installed module list
from one instance of Perl for import and installation on a different
instance of Perl.

##### modules commands

    export  Exports a list of all installed modules in the current Perl
    import  Imports a previously exported module list and installs them in the current perl
     
`import` command has an optional argument, which is the name of the
instance of Perl that you've previously exported from. If no argument is
sent in, we'll list the available exports you can choose to install
from.

#### options

Usage:  `berrybrew options [option] [value]`

Retrieve and set `berrybrew`'s options.

If the `option` argument isn't supplied, we'll display the values for all
configured options.

If an `option` is sent in, we'll display the value for that single option.

If both `option` and `value` are sent in, we'll set the option to the value,
then display the updated value for that option.
        
#### off

Usage:  `berrybrew off`

Disables all `berrybrew` Perl installations. If you have a Strawberry or
ActiveState system Perl installed, it'll be used until you `berrybrew switch`
back to a `berrybrew` controlled Perl.

#### register

Usage:  `berrybrew register <directory>`

Registers a custom installation within the Perl instance directory that was
placed there outside of `berrybrew`.

This allows you to copy in other portable Strawberry Perl instances from
elsewhere on your system, or from remote systems and have them operate under
the `berrybrew` umbrella.

#### remove

Usage:  `berrybrew remove <version>`

Removes a single version of Perl, as seen in `berrybrew available`.

#### switch

Usage:  `berrybrew switch <version> [quick]`

Sets the verion of Perl as seen in `berrybrew available` to the default
system Perl. This change is persistent. Use `berrybrew off` to disable the
switched-to Perl, or use `switch` to change to a different one.

If the `quick` argument is sent in, we'll make system changes in a way that
a new console window isn't required. WARNING: Some binaries and other features
may not work correctly using this method. If you have problems, simply run
`berrybrew-refresh`.

#### unconfig

Usage:  `berrybrew unconfig`

Removes berrybrew's binary directory from the `PATH` environment variable.

#### use

Usage:  `berrybrew use [options] version[,version[,...]]`

Runs a command-line environment with the selected version or versions of perl
at the head of the path, so it will be the active perl.

By default, it will run them inside the same window which ran berrybrew. To
exit a given version, type `exit`; this will either move you forward to the
next selected version of perl, or it will return you to the shell that called
berrybrew.  Inside each subshell, the `PATH` will be changed to point to
the selected version of perl, but when it returns to the shell that ran
berrybrew, the `PATH` will return to its previous setting.

##### use options:

    berrybrew use --win version[,version[,...]]
    berrybrew use --window version[,version[,...]]
    berrybrew use --windowed version[,version[,...]]

With the `--win` option (or it's variants), berrybrew will spawn a new window
for each version of perl selected.  Type `exit` to close the spawned
environment.  After spawning one or more windows, the window from which
berrybrew was run is still available for use.

#### virtual

Usage: `berrybrew virtual <desired_instance_name>`

This command will register an external Perl installation for use within
`berrybrew`. You will be prompted for three paths: a path to the external perl
binary, a path to the external perl's library directory, and an auxillary
diretory for any other custom path you may have.

The most common use case is so that you can have a berrybrew perl as your
standard perl, but you can `use` the external (eg: system) perl temporarily
without having to use `off` to access it.

#### help

Usage:  `berrybrew help`

Displays a summarized view of the available commands.

#### license

Prints the `berrybrew` license to `STDOUT`.

#### version

Usage:  `berrybrew version`

Displays the current version of the `berrybrew.exe` binary and `bbapi.dll`
library.

### Hidden Command List

#### currentperl

This feature simply fetches the Perl instance that's currently in use,
prints out its name, and exits. It will not display anything if there's no
Perl currently in use.

Used primarily for certain unit tests.

#### download

If a single perl version is sent in as an argument, we'll download that
particular version. If `all` is sent in, we'll download all available
perl versions (limited to the most recent point release of each major
release).

#### error

Usage: `berrybrew error <ErrNum>`

Translates an error code and displays the name of the error, where `ErrNum`
is the error number.

#### error-codes

Usage: `berrybrew error-codes`

Simply prints to `STDOUT` the list of all valid error status code numbers and
their associated error name, one pair per line.

Example:

    170 - MODULE_IMPORT_SAME_VERSION_ERROR
    175 - MODULE_IMPORT_VERSION_REQUIRED
    180 - OPTION_INVALID_ERROR
    -1  - GENERIC_ERROR

#### exit

Usage: `berrybrew exit 0`

Tests the exit mechanism. Requires an `Int32` as an argument.

To get the error name displayed, run the command with the `status` command:

```
  berrybrew status exit -5
```
#### info

Displays paths and other information regarding the `berrybrew` installation
itself.

#### options-update

Checks the base distribution's configuration file, and if there are any newly
added directives, we'll insert them into the registry. Used for upgrades and
testing.

#### options-update-force

Loads all configuration options from the configuration file into the registry.
Be warned that this will overwrite all changes that were previously changed
within the registry.

#### orphans

Displays all orphaned Perl instances that haven't been registered with
`berrybrew`.

#### register-orphans

This will register all orphaned Perl instances at once.

#### test

This feature should only be used by developers of berrybrew.

Like the `debug` feature, I've added a new `test` argument. It must
follow `berrybrew` and preceed all further operations. To include the
`debug` argument as well, specify it first, then include `test`, then
your command and any options:

Examples: 

- Test feature only:

    `berrybrew test clean ...`
    
- Test and Debug:

    `berrybrew debug test clean ...`

Currently, it's only used in the `t/99_clean.t` test to strip off
unneeded path elements for a couple of specific tests.

#### trace

Forces the printing of the full stack trace to `STDERR` upon program exit. We
also display the exit code, and its name.

Like `debug`, it needs to be the first argument, with all other command
arguments following it. Only `debug` should preceed it.

Examples: 

- Trace feature only:

    `berrybrew trace install 5.10.1_32`
    
- Trace and Debug:

    `berrybrew debug trace remove 5.10.1_32`

#### status

Displays the exit code information on exit.

Usage: `berrybrew status <command> [options]`.

If `debug` is also used, the `status` command must follow it.

&copy; 2016-2023 by Steve Bertrand
