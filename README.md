# berrybrew

The perlbrew for Windows Strawberry Perl! 

For a quick-start, jump to the [Installation](#installation) and
[Configuration](#configuration) sections.

`berrybrew` can download, install, remove and manage multiple concurrent
versions of Strawberry Perl for Windows. There is no 
[requirement](https://github.com/stevieb9/berrybrew#requirements "berrybrew requirements")
to have Strawberry Perl installed before using `berrybrew`.

Updating the list of Strawberry Perls available is as simple as runing a single
command: `berrybrew fetch`, and works at runtime.

There is extensive documentation available for the
[berrybrew](https://github.com/stevieb9/berrybrew/blob/master/doc/berrybrew.md)
application, as well as the 
[Berrybrew API](https://github.com/stevieb9/berrybrew/blob/master/doc/Berrybrew%20API.md).
See [SEE ALSO](https://github.com/stevieb9/berrybrew#see-also) for the
full list of documentation.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [Commands](#commands)
- [Synopsis](#synopsis)
- [Upgrading](#upgrading)
- [Add/Remove Perls Available](#addremove-perls-available)
- [Configure Perl Instance Directory](#configure-root-directory)
- [Compile Your Own](#compile-your-own)
- [Create a Release](#create-a-release)
- [Requirements](#requirements)
- [Troubleshooting](#troubleshooting)
- [Documentation](#see-also)
- [Caveats](#caveats)
- [License](#license)
- [Version](#version)

## Installation

##### Git clone

    git clone https://github.com/stevieb9/berrybrew

##### Pre-built zip archive

[berrybrew.zip](https://github.com/stevieb9/berrybrew/blob/master/download/berrybrew.zip?raw=true "berrybrew zip archive") `SHA1: 5b92aa38fffdba16c9675229548c39296d40d41c`

You can also [Compile your own](https://github.com/stevieb9/berrybrew#configure-root-directory)
installation.

## Configuration

See [Configure Root Directory](https://github.com/stevieb9/berrybrew#configure-root-directory) 
if you wish to change the default location that your Perl installations
will reside.

    cd berrybrew
    bin\berrybrew.exe config

## Commands

    berrybrew <command> [subcommand] [option]

        available   List available Strawberry Perl versions and which are installed
        config      Add berrybrew to your PATH
        clean *     Remove all temporary berrybrew files
        clone       Clones an installed version to a custom-named one
        fetch       Upgrade the list of Strawberry Perl instances available
        install     Download, extract and install a Strawberry Perl
        remove      Uninstall a Strawberry Perl
        switch      Switch to use a different Strawberry Perl
        off         Disable berrybrew perls (use 'switch' to re-enable)
        exec *      Run a command for every installed Strawberry Perl
        unconfig    Remove berrybrew from PATH
        upgrade     Backs up config, does a `git pull`, and restores config
        help        Display this help screen
        license     Show berrybrew license
        version     Displays the version

        * - view subcommand details with 'berrybrew <command> help'

## Synopsis

List all available versions of Perl that are available:
    
    > berrybrew available

    The following Strawberry Perls are available:
    
    > berrybrew available

    The following Strawberry Perls are available:

            5.24.1_64          [installed] *
            5.24.1_64_PDL
            5.24.1_32
            5.22.3_64
            5.22.3_64_PDL
            5.22.3_32          [installed]
            5.20.3_64
            5.20.3_64_PDL
            5.20.3_32
            5.18.4_64
            5.18.4_32
            5.16.3_64
            5.16.3_32
            5.14.4_64
            5.14.4_32
            5.12.3_32
            5.10.1_32
            5.8.9_32           [installed]
            5.8.9-unit_tests   [custom] [installed]
            client_project_X   [custom] [installed]

    * Currently using
    
Install a specific version:

    > berrybrew install 5.24.1_64

Switch to a different version (permanently):

    > berrybrew switch 5.24.1_64

    Switched to 5.24.1_64, start a new terminal to use it.

Start a new cmd.exe to use the new version:

    > perl -v

    This is perl 5, version 24, subversion 1 (v5.24.1) built for MSWin32-x64-multi-thread

    ...       

Clone an installed instance (very useful for setting up a main instance,
and cloning it into an instance named "template")

    > berrybrew clone 5.24.1_64 template

Uninstall a version of perl:

    > berrybrew remove 5.24.1_64

    Successfully removed Strawberry Perl 5.24.1_64

Disable berrybrew entirely, and return to system Perl (Strawberry or 
ActiveState), if available (re-enable with 'switch'):

    > berrybrew off

Execute something across all perls (we do not execute on Perls that has
'tmpl' or 'template' in the name):

    > berrybrew exec prove -l

    Perl-5.24.1_64
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

    > berrybrew exec --with 5.24.1_64,5.10.1_32 perl "die()"

    Perl-5.24.1_64
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

## Add/Remove Perls Available

Simply edit the `data/perls.json` file in the repository's root 
directory.

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

## Compile Your Own 

    git clone https://github.com/stevieb9/berrybrew
    cd berrybrew
    
    # compile the API library

    mcs \
        -lib:bin \
        -t:library \
        -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll \ 
        -out:bin/bbapi.dll \
        src/berrybrew.cs

    # compile the berrybrew.exe binary

    mcs \
        src/bbconsole.cs
        -lib:bin -r:bbapi.dll \
        -out:bin/berrybrew.exe \
        -win32icon:inc/berrybrew.ico

    bin\berrybrew.exe config

## Create a Release

If you've modified the information of the configuration files for the new
build, you must copy them to the `dev\data` directory before performing the
below steps.

Use the included `dev/release.pl` script, which:

- compiles the `berrybrew.exe` binary and the `bbapi.dll` API library

- collects default configuration files

- builds the bundled zip archive, and puts it into `download/`

- performs SHA1 checksum tasks

- updates the `README.md` file with the zip archive's new SHA1 sum

If you had any custom configuration files in place, run 
`dev\post_release.pl` to restore them.

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

- [Unit Testing](https://github.com/stevieb9/berrybrew/blob/master/doc/Unit%20Testing.md)
 Documentation for unit testing `berrybrew`

- [Configuration](https://github.com/stevieb9/berrybrew/blob/master/doc/Configuration.md)
 Guide to various configuration files and options

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

## License

2 Clause FreeBSD - see LICENSE

## Version

    1.10

## Original Author

David Farrell [http://perltricks.com]

## This Fork Maintained By

Steve Bertrand `steveb<>cpan.org`

## See Also

- [StrawberryPerl](http://strawberryperl.com) - Strawberry Perl for
Windows

- [Perlbrew](http://perlbrew.pl) - the original Perl version manager for
Unix based systems.
