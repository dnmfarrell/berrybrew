# berrybrew

The perlbrew for Windows Strawberry Perl! 


###### [Click here to download the installer](https://github.com/stevieb9/berrybrew/blob/master/download/berrybrewInstaller.exe?raw=true "berrybrew MSI installer")

For a quick-start, jump to the [Install](#install) and [Commands](#commands)
sections.

`berrybrew` can download, install, remove and manage multiple concurrent
versions of Strawberry Perl for Windows. There is no 
[requirement](https://github.com/stevieb9/berrybrew#requirements "berrybrew requirements")
to have Strawberry Perl installed before using `berrybrew`.

Updating the list of Strawberry Perls available is as simple as running a single
command: `berrybrew fetch`, and works at runtime.

There is extensive documentation available for the
[berrybrew](https://github.com/stevieb9/berrybrew/blob/master/doc/berrybrew.md)
application, as well as the 
[Berrybrew API](https://github.com/stevieb9/berrybrew/blob/master/doc/Berrybrew%20API.md).
See [SEE ALSO](https://github.com/stevieb9/berrybrew#see-also) for the
full list of documentation.

## Table of Contents

- [Install](#install)
- [Uninstall](#uninstall)
- [Configuration](#configuration)
- [Commands](#commands)
- [Examples](#examples)
- [Upgrading](#upgrading)
- [Update Perls Available](#update-perls-available)
- [Cloning Modules](#cloning-modules)
- [Configure Perl Instance Directory](#configure-root-directory)
- [Requirements](#requirements)
- [Troubleshooting](#troubleshooting)
- [Documentation](#see-also)
- [Developed Using](#developed-using)
- [Caveats](#caveats)
- [License](#license)
- [Version](#version)
- [Hidden Features](#hidden-features)

## Install

##### Self-installing executable

The easiest and most straight forward method.

[berrybrewInstaller.exe](https://github.com/stevieb9/berrybrew/blob/master/download/berrybrewInstaller.exe?raw=true "berrybrew MSI installer") `SHA1: `

##### Git clone

    git clone https://github.com/stevieb9/berrybrew
    cd berrybrew
    bin\berrybrew.exe config

##### Pre-built zip archive

[berrybrew.zip](https://github.com/stevieb9/berrybrew/blob/master/download/berrybrew.zip?raw=true "berrybrew zip archive") `SHA1: `

After extraction:

    cd berrybrew
    bin\berrybrew.exe config

#### Compile your own
    
You can also [Compile your own](https://github.com/stevieb9/berrybrew/blob/master/doc/Compile%20Your%20Own.md)
installation.

## Uninstall

If you used the self-extracting installer, simply run the uninstaller from
either `Add/Remove Programs` in the Control Panel, or the `uninst.exe`
uninstaller program located in the installation directory.

If you installed via any other method:

First run the `berrybrew unconfig` command which removes the `PATH` environment
variables for any in-use Perl installation, and then removes `berrybrew` from
the `PATH` as well.

If you wish to delete the actual installation:

- Remove the Perl installation root directory (by default `C:\berrybrew`) 

- Remove the original download directory

## Commands

See the [berrybrew](https://github.com/stevieb9/berrybrew/blob/master/doc/berrybrew.md)
documentation for a full explanation of all of the following commands.

    berrybrew <command> [subcommand] [option]

    available      List available Strawberry Perl versions and which are installed
    list           List installed Strawberry Perl versions
    clean *        Remove all temporary berrybrew files
    clone          Make a complete copy of a Perl installation
    config         Add berrybrew to your PATH
    exec *         Run a command for every installed Strawberry Perl
    fetch          Update the list of Strawberry Perl instances available
    info           Retrieve details about the berrybrew installation itself
    install        Download, extract and install a Strawberry Perl
    modules *      Export and import a module list from one Perl to install on another
    off            Disable berrybrew perls (use 'switch' to re-enable)
    register       Manually register a custom installation directory
    remove         Uninstall a Strawberry Perl
    switch *       Switch to use a different Strawberry Perl
    unconfig       Remove berrybrew from PATH
    upgrade        Performs a safe upgrade. Requires Git installed
    use *          Use a specific Strawberry Perl version temporarily
    virtual        Allow berrybrew to manage an external Perl instance
    help           Display this help screen
    license        Show berrybrew license
    version        Displays the version

    * - view subcommand details with 'berrybrew <command> help'

## Examples

See the [berrybrew](https://github.com/stevieb9/berrybrew/blob/master/doc/berrybrew.md)
document for usage examples.

## Upgrading

Using the [installer](https://github.com/stevieb9/berrybrew/blob/master/download/berrybrewInstaller.exe?raw=true "berrybrew MSI installer")
is the best and safest way to upgrade your `berrybrew`.

If the new install will not be in the same directory as your previous version, 
copy any new or differing configuration options in the `data\config.json` file
from the old instance to the new one, and if you've got a 
`data\perls_custom.json` or a `data\perls_virtual.json` file, copy them over in
their entirety.

The next best method is to use `berrybrew upgrade`. This requires Git to be
installed and in your `PATH`. It will create a `backup_timestamp`
directory and copy your configuration files into it.

After completion, it'll copy your `perls_custom.json` file back into the `data/`
directory. The rest of the configuration JSON files will be replaced. If you had
any customizations within any of the other configuration files, you'll need to
manually merge those changes back into the updated config file in `data/`.

Doing a straight `git pull` will overwrite your configuration files, so
back them up first (see [Caveats](#caveats)).

## Update Perls Available

Use `berrybrew fetch` to retrieve the most recent availability list from
Strawberry Perl. If any new or changed versions are found, we'll update the
local `perls.json` file with them.

If you supply the `all` subcommand to `berrybrew fetch`, we will load all
available Perls that Strawberry has to offer.

## Configure Root Directory

By default, we manage Perls out of the `C:\berrybrew` directory. To 
change this, modify the `root_dir` value in the `data\config.json` file.
Use double-backslashes (`\\`) as the path separators. 

WARNING: At this time, it is highly advised not to change this after 
you've already installed any instances of Perl. This feature is 
incomplete, and `PATH` and other things don't get properly reset yet.
If you choose to ignore this, follow this procedure:

- run `berrybrew off`, to flush the `PATH` environment variables

- edit the configuration file to reflect the new directory

- move all Perl installations from the old path to the new one

- remove the old directory

- run `berrybrew switch $version` to set things back up

## Requirements

- .Net Framework 2.0 or higher

- Windows only!

- [Mono](http://www.mono-project.com) or Visual Studio (only if 
compiling your own version)

## Troubleshooting

If you run into trouble installing a Perl, try clearing the berrybrew
cached downloads by running `berrybrew clean`. 

You can also enable debugging to get more verbose output on the command
line:

    berrybrew debug <command> [options] 

## SEE ALSO

- [berrybrew](https://github.com/stevieb9/berrybrew/blob/master/doc/berrybrew.md)
 Full documentation for the application

- [Berrybrew API](https://github.com/stevieb9/berrybrew/blob/master/doc/Berrybrew%20API.md)
 API documentation

- [Configuration](https://github.com/stevieb9/berrybrew/blob/master/doc/Configuration.md)
 Guide to various configuration files and options

- [Compile Your Own Installation](https://github.com/stevieb9/berrybrew/blob/master/doc/Compile%20Your%20Own.md)
 Guide to compiling `berrybrew` from source

- [Create a Development Build](https://github.com/stevieb9/berrybrew/blob/master/doc/Create%20a%20Development%20Build.md)
 Guide to creating a development build for testing new functionality

- [Unit Testing](https://github.com/stevieb9/berrybrew/blob/master/doc/Unit%20Testing.md)
 Documentation for unit testing `berrybrew`
 
- [Create and Publish a Release](https://github.com/stevieb9/berrybrew/blob/master/doc/Create%20a%20Release.md)
 Guide to creating a release, publishing it as a production install, and
 setting up the next release branch

## CAVEATS

- When using `git pull` to do an upgrade, your configuration files will
be overwritten with the defaults. If you have any customizations, make a
backup of the `data` directory before upgrade, then copy the files back
to their original location. Note that you may have to manually add any
new config directives into the original config files. The 
`perls_custom.json` file used for custom Perl installations (clones) 
will never be overwritten, and this warning does not apply for it.

- At this time, `berrybrew` requires Administrative privileges to
operate correctly. This is due to the way Windows forces the System 
`PATH` to take precedence over User `PATH`.

## Developed Using

|Software|Description|Notes|
|---|---|---|
|[Jetbrains Rider](https://www.jetbrains.com/rider/)|.Net IDE|Thanks to their [Open Source Licensing](https://www.jetbrains.com/buy/opensource/)
|[Jetbrains intelliJ IDEA](https://www.jetbrains.com/idea/)|IDE for Perl coding|Freely available, also comes with the open source license|
|[Camelcade Perl5 Plugin](https://github.com/Camelcade/Perl5-IDEA)|Perl5 Plugin for intelliJ IDEA||
|[Devel::Camelcadedb](https://metacpan.org/pod/distribution/Devel-Camelcadedb/lib/Devel/Camelcadedb.pod)|Adds Perl5 debug support for intelliJ IDEA||
|[Mono](https://www.mono-project.com/)|Open Source .Net Framework||
|[Mono C# Compiler](https://www.mono-project.com/docs/about-mono/languages/csharp/)|C#|Open Source C# Compiler|

## License

2 Clause FreeBSD - see LICENSE

## Version

    1.30

## Hidden Features

There are certain features that should only be used by developers and
maintainers of this software. There's only a couple, so if I create
more and/or make them more complex, I'll create a separate document
for them.

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

#### currentperl

This feature simply fetches the Perl instance that's currently in use,
prints out its name, and exits. It will not display anything if there's no
Perl currently in use.

Usage:

    berrybrew currentperl

Used primarily for certain unit tests.

#### register_orphans

This will register all orphaned Perl instances. Used primarily during the
self-extracting installer during an upgrade to ensure that if the `perls.json`
file has changed, all previous Perl instances will be visible and usable.

## Original Author

David Farrell [http://perltricks.com]

## This Fork Maintained By

Steve Bertrand `steveb<>cpan.org`

## See Also

- [StrawberryPerl](http://strawberryperl.com) - Strawberry Perl for
Windows

- [Perlbrew](http://perlbrew.pl) - the original Perl version manager for
Unix based systems.
