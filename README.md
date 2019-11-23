# berrybrew

The perlbrew for Windows Strawberry Perl! 

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
- [Undocumented Features](#undocumented-features)

## Install

##### Self-installing executable

The easiest and most straight forward method.

[berrybrewInstaller.exe](https://github.com/stevieb9/berrybrew/blob/master/download/berrybrewInstaller.exe?raw=true "berrybrew MSI installer") `SHA1: 29b4ea3d819b6f5436d69f72c7a543cbb843fde6`

##### Git clone

    git clone https://github.com/stevieb9/berrybrew
    cd berrybrew
    bin\berrybrew.exe config

##### Pre-built zip archive

[berrybrew.zip](https://github.com/stevieb9/berrybrew/blob/master/download/berrybrew.zip?raw=true "berrybrew zip archive") `SHA1: 41aa9f0ae7c71b4763d7f37504e457d18e34b581`

After extraction:

    cd berrybrew
    bin\berrybrew.exe config

#### Compile your own
    
You can also [Compile your own](https://github.com/stevieb9/berrybrew/blob/master/doc/Compile%20Your%20Own.md)
installation.

## Uninstall

If you used the self-extracting installer, simply run the uninstaller.

If you installed via any other method:

First run the `berrybrew unconfig` command which removes the `PATH` environment
variables for any in-use Perl installation, and then removes `berrybrew` from
the `PATH` as well.

If you wish to delete the actual installation:

- remove the `C:\berrybrew` directory which contains the installation, perl
installations and all configuration and temporary data
- remove the original download directory

## Commands

    berrybrew <command> [subcommand] [option]

    available      List available Strawberry Perl versions and which are installed
    list           List installed Strawberry Perl versions
    clean *        Remove all temporary berrybrew files
    clone          Make a complete copy of a Perl installation
    config         Add berrybrew to your PATH
    exec *         Run a command for every installed Strawberry Perl
    fetch          Update the list of Strawberry Perl instances available
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

List all versions of Perl that are available, installed, and currently used:
    
    > berrybrew available

    The following Strawberry Perls are available:

        5.30.0_64
        5.30.0_64_PDL
        5.30.0_32
        5.28.0_64
        5.28.0_64_PDL
        5.28.0_32
        5.26.2_64
        5.26.2_64_PDL
        5.26.2_32
        5.24.4_64
        5.24.4_64_PDL
        5.24.4_32
        5.22.3_64
        5.22.3_64_PDL
        5.22.3_32
        5.20.3_64
        5.20.3_64_PDL
        5.20.3_32
        5.18.4_64
        5.18.4_32
        5.16.3_64
        5.16.3_32
        5.14.4_64
        5.14.4_32
        5.12.3_64
        5.12.3_32
        5.10.1_32       [installed]
        5.8.9_32
        5.24.1_64       [custom] [installed] *

    * Currently using
    
List all currently installed versions of Perl:

    > berrybrew list

        5.30.0_64
        5.30.0_64_PDL
        5.30.0_32
        5.28.0_64
        5.26.2_64
        5.10.1_32
    
Install a specific version:

    > berrybrew install 5.30.0_64

Switch to a different version (permanently):

    > berrybrew switch 5.30.0_64

    Switched to 5.30.0_64, start a new terminal to use it.

Start a new cmd.exe to use the new version:

    > perl -v

    This is perl 5, version 30, subversion 0 (v5.30.0) built for MSWin32-x64-multi-thread

Switch to a different version (permanently) without needing a new console window:

    > berrybrew switch 5.30.0_64 quick
    
You may run into issues running external binaries along with certain features with
the 'quick' feature. If so, simply close the existing window, and open a new one.
    
Clone an installed instance (very useful for setting up a main instance,
and cloning it into an instance named "template")

    > berrybrew clone 5.30.0_64 template

Uninstall a version of perl:

    > berrybrew remove 5.30.0_64

    Successfully removed Strawberry Perl 5.30.0_64

Manage an external instance of Perl (system ActiveState for example):

    > berrybrew virtual activestate

    Specify the path to the perl binary: c:\strawberry\perl\bin

    Specify the library path:

    Specify an additional path:

    Successfully registered virtual perl activestate

Manually register a custom directory within the Perl installation directory

    > berrybrew register my_custom_install

Disable berrybrew entirely, and return to system Perl (Strawberry or 
ActiveState), if available (re-enable with 'switch'):

    > berrybrew off

Temporarily use a selected version:

    > berrybrew use 5.10.1_32

Temporarily use a Perl version, but spawn in a new command window:

    > berrybrew use --win 5.10.1_32

Temporarily spawn several versions, all in new windows:

    > berrybrew use --win 5.10.1_32,5.24.2_64,5.28.0_64

Execute something across all perls (we do not execute on Perls that has
'tmpl' or 'template' in the name):

    > berrybrew exec prove -l

    Perl-5.30.0_64
    ==============
    t\DidYouMean.t .. ok
    All tests successful.
    Files=1, Tests=5,  0 wallclock secs ( 0.06 usr +  0.00 sys =  0.06 CPU)
    Result: PASS

    Perl-5.22.3_32
    ==============
    t\DidYouMean.t .. ok
    All tests successful.
    Files=1, Tests=5,  0 wallclock secs ( 0.03 usr +  0.03 sys =  0.06 CPU)
    Result: PASS

    Perl-5.18.4_64
    ==============
    t\DidYouMean.t ..
    Dubious, test returned 5 (wstat 1280, 0x500)
    Failed 5/5 subtests

    Test Summary Report
    -------------------
    t\DidYouMean.t (Wstat: 1280 Tests: 5 Failed: 5)
      Failed tests:  1-5
      Non-zero exit status: 5
    Files=1, Tests=5,  0 wallclock secs ( 0.02 usr +  0.05 sys =  0.06 CPU)
    Result: FAIL

Execute on only a selection of installed versions:

    > berrybrew exec --with 5.30.0_64,5.10.1_32 perl -e die()

    Perl-5.30.0_64
    ==============
    Died at -e line 1.

    Perl-5.10.1_32
    ==============
    Died at -e line 1.

Upgrade:

    > berrybrew upgrade

Remove `berrybrew` from `PATH` (useful for switching between versions of
`berrybrew`):

    > berrybrew unconfig
    
## Upgrading

Easiest way is to use `berrybrew upgrade`. This requires Git to be
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

## Cloning Modules

Currently, this is a two-phase operation, and is in beta. Here's the
procedure. first, `berrybrew switch` to the Perl instance you want to
export the module list for, and:

    > berrybrew modules export

Then, `berrybrew switch` to the Perl instance you want to import the
exported modules into. You'll need to close and reopen a new command
window, as always.

Then, the following command will display a list of all exported module
files from any/all Perl instances you've done an export from:

    > berrybrew modules import
    
    re-run the command with one of the following options:

    5.16.3_64    
    
In my case here, I've only got one export, from a `5.16.3_64` Perl
instance. Use it (I'm currently on `5.20.3_64`):

    > berrybrew modules import 5.16.3_64
    
NOTE: It is best to export from an older Perl and install on a newer
one, as it can take a significant amount of time to re-install ALL
exported modules.

NOTE: You can edit the module export file (by default in `C:\berrybrew\modules\`).
Each export file has the name of the Perl it was exported from. Just
add and/or remove any entries you'd like. You can even create the files
manually by hand so you have a custom, ready made template for all new
Perl installs. There is no limit on naming convention, so you can
literally manually create a file called `base_modules_template` for
example.
    
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

    1.27

## Undocumented Features

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
prints out its name, and exits.

Used primarily for certain unit tests.

## Original Author

David Farrell [http://perltricks.com]

## This Fork Maintained By

Steve Bertrand `steveb<>cpan.org`

## See Also

- [StrawberryPerl](http://strawberryperl.com) - Strawberry Perl for
Windows

- [Perlbrew](http://perlbrew.pl) - the original Perl version manager for
Unix based systems.
