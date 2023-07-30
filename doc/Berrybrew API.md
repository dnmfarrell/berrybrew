## berrybrew API (bin/bbapi.dll)

Core API source code is located in the `src/berrybrew.cs` file. It is
standalone namespace/class code, and contains no entry points. This library
compiles in all other API source files. See below for classes compiled in, and
the source file and namespace for each.

The code for the `berrybrew.exe` binary itself resides in `src/bbconsole.cs` and
contains the `Main()` entry point.

The code for the `berrybrew-ui.exe` is in `src/berrybrew-ui.cs`.

Each class link will direct you to a list of that class' methods. Each method
link in the respective class' list will direct you to that method's definition.

| Class |File|Namespace|Description
|---|---|---|---|
[Berrybrew](#berrybrew-class)            | src/**berrybrew.cs** | **BerryBrew** | Core API
[Message](#message-class)                | src/**messaging.cs** | BerryBrew.**Messaging** | Content for all output
[PathOp](#pathop-class)                  | src/**pathoperations.cs** | BerryBrew.**PathOperations** | Environment path management
[PerlOp](#perlop-class)                  | src/**perloperations.cs** | BerryBrew.**PerlOperations** | Operations to manage Perl instances 
[StrawberryPerl](#struct-strawberryperl) | src/**perlinstance.cs** | BerryBrew.**PerlInstance** | Perl instance container

### Exit Status

Most exit statuses will be `berrybrew` specific, except calls that shell out to
separate processes (eg. `Exec()`). In these cases, the exit status code will
be that of the external process, not that from within.

For example, a call to `ExecCompile()` will call `Exec()` which starts one or
more separate processes. If any of those processes fail, the status code will
be that of the failed process (even if all other processes succeed).

## Berrybrew Class

The `Berrybrew` class is the base of the system.

|Method name| Available  |Description|
|---|------------|---|
[Available](#available)| **public** | Displays all available Perls
[AvailableList](#availablelist)| **public** | Returns a list of available Perl names
[BaseConfig](#baseconfig)| private    | Initializes the registry-based configuration
[BitSuffixCheck](#bitsuffixcheck)| **public** | Adds the `_64` bit suffix if required to a Perl name
[CheckName](#checkname)| **public** | Validates the name of a custom Perl install
[CheckRootDir](#checkrootdir)| private    | Creates the Perl install directory if required
[Clean](#clean) | **public** | Stages removal of temp files and orphaned Perls
[CleanBuild](#cleanbuild) | private    | Remove the developer's `staging` build directory 
[CleanDev](#cleandev) | private    | Remove the developer's `staging` and `testing` **data** directories
[CleanModules](#cleanmodules) | private    | Removes the directory where we store exported module lists
[CleanOrphan](#cleanorphan)| private    | Removes all orphaned Perls
[CleanTemp](#cleantemp)| private    | Removes temporary files
[Clone](#clone)| **public** | Copies an installed Perl to a new name
[Config](#config)| **public** | Puts `berrybrew.exe` in `PATH`
[Download](#download)| **public** | Downloads one or all available versions of portable Strawberry Perls
[Exec](#exec)| private    | Runs commands on all installed Perls
[ExecCompile](#execcompile)| **public** | Staging for `Exec()`
[Exit](#exit)| **public** | Custom wrapper for `Environment.Exit()`
[ExportModules](#exportmodules)| **public** | Export an installed module list from current Perl
[Extract](#extract)| private    | Extracts Perl installation zip archives
[Fetch](#fetch)| private    | Downloads the Perl installation files
[FileAssoc](#fileassoc)| **public** | Manage .pl file associations
[FileRemove](#fileremove)| private    | Deletes a file
[FileSystemResetAttributes](#filesystemresetattributes)| **public** | Defaults filesystem attrs
[ImportModules](#importmodules)| **public** | Import modules into a Perl from a previously exported list
[ImportModulesExec](#importmodulesexec)| private    | Helper/executive method for `ImportModules()`
[Info](#info)| **public** | Displays information about specific installation elements
[Install](#install)| **public** | Installs new instances of Perl
[JsonParse](#jsonparse)| **public** | Reads JSON config files
[JsonWrite](#jsonwrite)| **public** | Writes out JSON configuration
[List](#list) | **public** | Lists currently installed Perl versions
[Off](#off) | **public** | Completely disables `berrybrew`
[Options](#options) | **public** | Display or set a single option, or show them all
[OptionsUpdate](#optionsupdate)| **public** | Update registry configuration with new directives
[OrphanedPerls](#orphanedperls)| **public** | Displays the list of orphaned perls 
[ProcessCreate](#processcreate)| **public** | Creates and returns a Windows cmd process
[SnapshotCompress](#snapshotcompress) | Zips and saves an archive of a Perl instance
[SnapshotExtract](#snapshotextract) | Unzips, installs and registered a previously saved snapshot
[SnapshotInit](#snapshotinit)| private | Checks for the snapshot storage directory, creates if necessary
[SnapshotList](#snapshotlist) | Lists all previously saved snapshots
[Switch](#switch)| **public** | Change to a specific version of Perl (persistent)
[SwitchQuick](#switchquick) | **public** | Called by `Switch()`, sets up the new environment
[Unconfig](#unconfig)| **public** | Removes berrybrew bin dir from `PATH`
[UseCompile](#usecompile)| **public** | Staging for `UseInNewWindow()` and `UseInSameWindow()`
[UseInNewWindow](#useinnewwindow)| private    | Spawns new window(s) with the selected version(s) of perl at the head of the PATH
[UseInSameWindow](#useinsamewindow)| private    | Runs a new command-interpreter with the selected version of perl at the head of the PATH (with multiple versions run serially)
[Version](#version)| **public** | Return the version of the current `berrybrew`

## Message Class

The `Message` class is a helper that manages the various output
that is displayed to the user.

|Method name|Available|Description|
|---|---|---|
[Add](#messageadd)| **public** | Adds a new message to the collection
[Error](#messageerror)| **public** | Same as `Print()`, but writes to `STDERR` instead of `STDOUT`
[Get](#messageget)| **public** | Fetches the content of a specific message
[Print](#messageprint)| **public** | Prints the content of a specific message
[Say](#messagesay)| **public** | Same as `Print()`, but terminates

## PathOp Class

Manages all activity and functionality related to the environment paths.

|Method name| Available  |Description|
|---|------------|---|
[PathAddBerryBrew](#pathoppathaddberrybrew)| internal   | Adds `berrybrew` to `PATH`
[PathAddPerl](#pathoppathaddperl)| internal   | Adds a Perl to `PATH`
[PathGet](#pathoppathget)| **public** | Retrieves the Machine `PATH`
[PathGetUsr](#pathoppathgetusr)| internal   | Get the currently logged in user's `PATH` environment variable
[PathRemoveBerrybrew](#pathoppathremoveberrybrew)| **public** | Removes berrybrew from `PATH`
[PathRemovePerl](#pathoppathremoveperl)| **public** | Removes specified Perl from `PATH`
[PathScan](#pathoppathscan)| internal   | Checks `PATH` for a specific binary file
[PathSet](#pathopathset)| internal   | Writes all `PATH` changes to the registry

## PerlOp Class

Manages all operations necessary to maintain the Strawberry Perl instances.

|Method name| Available  |Description|
|---|------------|---|
[PerlArchivePath](#perlopperlarchivepath)| internal   | Returns the path and filename of the zip file
[PerlGenerateObjects](#perlopperlgenerateobjects)| internal   | Generates the `StrawberryPerl` class objects
[PerlInUse](#perlopperlinuse)| **public** | Returns the object that represents Perl currently in use
[PerlIsInstalled](#perlopperlisinstalled)| internal   | Checks if a specific Perl is installed
[PerlsInstalled](#perlopperlsinstalled)| **public** | Fetches the list of Perls installed
[PerlOrphansFind](#perlopperlorphansfind)| internal   | Locates non-registered directories in Perl root
[PerlOrphansIgnore](#perlopperlorphansignore)| **public** | Returns a list (dict) of directories that are never orphans. 
[PerlRegisterCustomInstall](#perlopperlregistercustominstall)| **public** | Make `berrybrew` aware of custom instances
[PerlRegisterVirtualInstall](#perlopperlregistervirtualinstall)| **public** | Make `berrybrew` aware of external Perls
[PerlRemove](#perlopperlremove)| **public** | Uninstalls a specific instance of Perl
[PerlResolveVersion](#perlopperlresolveVersion)| internal   | Resolves the name of a Perl to its StrawberryPerl object
[PerlUpdateAvailableList](#perlopperlupdateavailablelist)| **public** | Automatically fetches new Strawberry Perls available
[PerlUpdateAvailableListOrphans](#perlopperlupdateavailablelistorphans)| **public** | Registers any orphaned Perls after using `Fetch()`

## Struct StrawberryPerl

This struct represents all information and facets of an individual Strawberry
Perl instance.

Its source file is `src/perlinstance.cs` and its namespace is
`BerryBrew.PerlInstance`.

| Property | Available  | Type | Description |
|----------|------------|------------------|---|
**Name** | public readonly | string | The Perl instance's name 
**File** | public readonly | string | Filename portion of the zip file
**Url** | public readonly | string | Download URL for this Perl instance
**Version** | public readonly | string | Version of the instance (eg. 5.10.1_32)
**Sha1Checksum** | public readonly | string | The SHA1 checksum of the zip archive file
**Newest** | public readonly | bool | Is this the most recent point release of the major release?
**Custom** | public readonly | bool | Is this instance a custom install?
**Virtual** | public readonly | bool | Is this instance a virtual instance?
**archivePath** | public readonly | string | Temp directory where we'll extract the instance zip file
**installPath** | public readonly | string | Directory where the instance will be run out of
**CPath** | public readonly | string | Instance auxillary/additional library/include path
**PerlPath** | public readonly | string | Full path to the instances perl.exe binary
**PerlSitePath** | public readonly | string | Primary instance library/include path
**Paths** | public readonly | List<string> | A list of the above mentioned paths

### StrawberryPerl Struct Instantiation Parameters

| Parameter | Mapped Property | Type | Required | Default |
|-----------|-----------------|------|----------|---------|
**bb** | N/A | Berrybrew object | true | N/A 
**name** | Name | Converted JSON object | true | N/A
**file** | File | Converted JSON object | true | N/A
**url** | Url | Converted JSON object | true | N/A
**version** | Version | Converted JSON object | true | N/A
**csum** | Sha1Checksum | Converted JSON object | true | N/A
**newest** | Newest | bool | false | false
**custom** | Custom | bool | false | false
**virtual_install** | Virtual | bool | false | false
**perl_path** | PerlPath | string | false | ""
**lib_path** | PerlSitePath | string | false | ""
**aux_path** | CPath | string | false | ""

## Berrybrew Class Methods

#### Available

    public void Available(allPerls=false)

        argument:   allPerls
        value:      Bool
        default:    false
         
Displays the names of the versions of Perl that are available to `berrybrew`,
as found in `this.Perls`, where `this.Perls` is a
`OrderedDictionary<string name, Berrybrew.StrawberryPerl>`.

If `allPerls` is set to `true`, we will list all available Perls. Otherwise,
we display only the most recent point release of each major version.

#### AvailableList

    public List<string> AvailableList(allPerls=false)
        
         argument:   allPerls
         value:      Bool
         default:    false   
         
Returns a list of strings of Perl names that are available for install.

If `allPerls` is set to `true`, we will return all available Perls. Otherwise,
we return only the most recent point release of each major version.  

#### BaseConfig

    private void BaseConfig()
    
Initializes the registry based configuration.

#### BitSuffixCheck

    public string BitSuffixCheck(string perlName)

        argument:   perlName
        value:      Name of an available Perl

        return:     The name of the Perl sent in, with the bit suffix added

Checks if the name of the Perl sent in contains a bit suffix, and if not, adds
`_64`, and returns the updated name. This allows you to omit the suffix on the
command line when desiring a 64-bit version of Perl.

#### CheckName

    public static bool CheckName(string perlName)

        argument:   perlName
        value:      Name of an available Perl

        return:     true on success, false on fail

Checks the name of a custom Perl to ensure it fits within the guidelines.

#### CheckRootDir

    private void CheckRootDir()

Checks whether the Perl root installation directory exists, and creates it if not.

#### Clean

    public void Clean(string subcmd="temp")

        argument:   subcmd
        values:     "temp", "orphan", "module", "dev", "all"

By default, `subcmd` is set to "temp", which we delete all downloaded Perl
installation zip files from the temporary directory. With "orphan", we'll
delete all directories found in the Perl installation root directory that
`berrybrew` has not registered as valid Perl installs.

#### CleanBuild

    private bool CleanBuild()

Removes the developer's `staging` build directory located in the repository.

Returns `true` if the directory was removed successfully or `false` otherwise.

#### CleanDev

    private bool CleanDev()
    
Removes both the `staging` and `testing` **data** directories. This method
should only be used by developers of `berrybrew`.

Returns `true` if both directories are non-existent after the routine
has been run, or `false` otherwise.

#### CleanModules

    private bool CleanModules()
    
Removes the directory that we store exported module list files into.    

Returns `true` on success, and `false` on failure.

#### CleanOrphan

    private bool CleanOrphan()

Removes all directories found in the Perl installation directory that aren't
associated with any registered Perl instances.

Returns `true` if any orphans were found/deleted, `false` if not.

#### CleanTemp

    private bool CleanTemp()

Removes all Perl installation zip files from the temporary staging directory.

Returns `true` if any files were found/deleted, `false` if not.

#### Clone

    public void Clone(string src, string dest)

        argument:   src
        values:     Name of an installed berrybrew Perl instance

        argument:   dest
        values:     Any string name by which you want the clone to appear
                    in 'berrybrew available'

Makes an exact copy of an existing installed Perl instance with a name of your
choosing, and makes it available just like all others. `berrybrew available`
will label these custom installs appropriately. 

#### Config

    public void Config()

Adds the path to the `berrybrew.exe` executable into the `PATH` environment
variable.

#### Download

    public void Download(string versionString)

        argument:   versionString
        value:      A Strawberry Perl version string, or "all"

Downloads an individual Strawberry Perl given a version number, or if "all" is sent in, we'll download
all perl versions (most recent point release of each major release only).

#### Exec

    private void Exec(StrawberryPerl perl, List<string> parameters, string sysPath, Boolean singleMode)

        argument:   perl
        value:      A single StrawberryPerl object

        argument:   parameters
        value:      The full command string you want all installed Perls to execute

        argument:   sysPath
        value:      String containing the full Machine PATH environment variable
        
        argument:   singleMode
        value:      True if running on a single Perl instance, False otherwise

Called by `ExecCompile()`, executes a command on a single Perl instance a
command to execute.

#### ExecCompile

    public void ExecCompile(List<String> parameters)

        argument:   parameters
        value:      Full command string that Exec() hands off, including
                    any Exec() specific instructions

Sets things up before handing each command off to `Exec()` for final
processing. If the `--with` flag is included, we'll strip it off and only
send the commands to be executed to those specific Perls.

This method sends a single Perl at a time to `Exec()`, and will always skip
any Perls that have either `tmpl` or `template` in the name.

By default, we also skip over all custom (cloned) instances. To have them
included, set `custom_exec` to `true` by using `berrybrew options custom_exec true`.

You can omit the bit suffix (eg: `_64`) if using a 64-bit Perl. We'll default to it.

#### Exit

    public void Exit(int exitCode)
    
        argument:   exitCode
        value:      Integer, the exit code to return
        
Simple wrapper for `Environment.Exit()` which allows for stacktrace information
and other customization.

#### ExportModules

    public void ExportModules()
    
Exports a list of all installed modules from the currently in-use Perl
instance.

The process will create a new `modules` directory under the Perl
installation directory (default is `C:\berrybrew`), and the name of the
file will be the version name of the Perl you're exporting from (eg.
`5.20.3_64`).
    
#### Extract

    private void Extract(StrawberryPerl perl, string archiveDir)

        argument:   perl
        value:      A single instance of the StrawberryPerl class

        argument:   archiveDir
        value:      The full path to the temporary Perl extraction directory
        typical:    this.archivePath

Extracts a Perl instance zip archive into the Perl installation directory.

#### Fetch

    public string Fetch(StrawberryPerl perl)

        argument:   perl
        value:      Single instance of the StrawberryPerl class

        return:     The name of the folder the zip file was downloaded to

Downloads the zip file for the version of Perl found in the StrawberryPerl
object, and returns the directory of where it was put.

#### FileAssoc

    public void FileAssoc(action="", quiet=false)
    
    argument:   action
    value:      String, "set" or "unset"
   
    argument:   quiet
    value:      Bool
    default:    false
     
View, set or unset the file association for `.pl` Perl script files.

If `action` is `set`, we'll update the association and manage it ourselves. If
set to `unset`, we'll revert it back to the way it was prior to a `set` call.

If `action` is left default, we'll display to the console the current setting.

If you do not have elevated administrative privileges, we return early and do
nothing.

Set `quiet` to prevent the default action from displaying output.

#### FileRemove

    private static string FileRemove(string filename)

        argument:   filename
        value:      Name of an existing file on the system

        return:     Stringified Exception or "true"

Deletes a file from the file system. Returns stringified "true" on success,
and a stringified `IO` exception on failure.

#### FileSystemResetAttributes

    public static void FileSystemResetAttributes(string dir)

        argument:   dir
        value:      Name of a directory that exists in the filesystem

Recursively resets all files and directories within the directory being
operated on back to default. This method was written specifically to ensure
that no files were readonly, which prevented us from removing Perl
installations.

#### ImportModules

    public void ImportModules(string version="")
    
        argument:   version
        value:      Name of a Perl instance you've exported a module list from
        
Imports a previously exported module list (from a different Perl instance),
and installs all of the listed modules into the currently in-use Perl.

#### ImportModulesExec

    private void ImportModulesExec(string file, string path)
    
        argument:   file
        value:      The name of a Perl instance that you've exported the module list from
        
        argument:   path
        value:      The full path including the file name listed in the 'file' parameter
        
This method is called by `ImportModules()`, and simply performs the routines
that install all the listed modules within the exported file.

#### Info

    public void Info(string want)

        argument:   want
        value:      One of "archive_path", "bin_path", "root_path" or "install_path"

Writes to the console a string containing the required information.

#### Install

    public void Install(string version)

        argument:   version
        value:      Name of an available Perl, as seen with 'berrybrew available'


Installs and registers a new instance of Perl.

#### JsonParse

    public dynamic JsonParse(string type, bool raw=false)

        argument:   type
        value:      The name of the JSON file, with the '.json' extension removed

        argument:   raw
        value:      bool
        default:    false

        return:     dynamic // object or JSON string

Extracts the JSON string from the various JSON files, and returns it. If `raw`
is set to `false` (default), we send the data back de-serialized. If `raw` is
`true`, we'll send back the JSON string as-is, with no de-serialization.

#### JsonWrite

    public void JsonWrite(
        string type,
        List<Dictionary<string, object>> data,
        bool fullList=false
    )

        argument:   type
        value:      The name of the JSON file, with the '.json' extension removed

        argument:   data
        value:      List of Dictionary objects. Each dict contains the name of an
                    available Perl as the key, and a StrawberryPerl instance as the
                    value

        argument:   fullList
        value:      bool
        default:    false

Writes out a JSON file containing information regarding installed Perls. If
`fullList` is set to `false` (default), we'll read in the existing list in the
file, and append the new objects to it. If `fullList` is set to `true`, we'll
assume you've compiled the list yourself, and we overwrite the file with the
new `data`.

#### List

    public void List()
    
Displays a list of the versions of Perl that are currently installed.

#### Off

    public void Off()

Disabled all `berrybrew` managed Perls, by removing them from `PATH`
environment variables. This will return you to a system Strawberry or
ActiveState system installed Perl.

#### Options

    public string Options(string option=null, string value=null, bool quiet=false)
   
    argument:   option
    value:      String. The name of a valid option
    default:    null
    
    argument:   value
    value:      String. The value of the option you want to set
    default:    null 

    argument:   quiet
    value:      Bool. Display output or not
    default:    false
    
Display, return and set `berrybrew`'s options.

If no arguments are sent in, we'll display the entire list of options, and return
an empty string.

If the `option` arg is sent in with a valid value, we'll display and return the
current value for that option.

If both the `option` and `value` arguments are sent in, we'll set that option
to the value, display and return the updated value.

if `quiet` is set to `true`, we won't display output to the console.

#### OptionsUpdate

    public void OptionsUpdate(bool force=false)

        argument:   force
        value:      Bool
        default:    false
            
Inserts any new configuration file directives to the registry. Used for
upgrades.

If the `force` argument is sent in as `true`, we will reload all of the 
configuration file values into the registry.

#### OrphanedPerls

    public void OrphanedPerls()

Prints to `STDOUT` the list of Perl instances that aren't registered with
`berrybrew`.

#### ProcessCreate

    public System.Diagnostics.Process ProcessCreate(string cmd, bool hidden=true)

        argument:   cmd
        value:      String containing the command and arguments to execute

        argument:   hidden
        value:      true/false whether the new cmd window should be hidden
        default:    true

        variable:   StartInfo.RedirectStandardOutput
        value:      true

        variable:   StartInfo.RedirectStandardError
        value:      true

        variable:   StartInfo.UseShellExecute
        value:      false

        return:     A System.Diagnostics.Process object

Builds and returns a process ready to be modified or have `Start()` called on it.

#### SnapshotCompress

    public void SnapshotCompress(string instanceName, string snapshotName = null)

        argument:   instanceName
        value:      String containing the perl instance name to archive

        argument:   snapshotName
        value:      String containing an optional, desired name for the snapshot
        default:    The name of the Perl instance, with an appended timestamp

Creates a zip archive file (snapshot) of an existing Perl instance. Saves it to
`snapshotPath`.

#### SnapshotExtract

    public void SnapshotExtract(string snapshotName, string instanceName = null)

        argument:   snapshotName
        value:      The name of the snapshot to install (use `berrybrew snapshot list`)

        argument:   instanceName
        value:      The name you want to assign to the Perl instance
        default:    The name of the snapshot

Unzips, installs and registers a previously archived snapshot.

#### SnapshotInit

    private void SnapshotInit()

Checks that the `snapshotPath` directory exists, and creates it if not.

#### SnapshotList()

    public void SnapshotList()

Displays the names of all previously saved snapshots.

#### Switch

    public void Switch(string perlVersion, bool switchQuick=false)

        argument:   perlVersion
        value:      Name of an available and installed Perl instance

        argument:   switchQuick
        value:      Bool, false by default
        
Updates `PATH` with the relevant path details in order to make this Perl
instance the default used across the board. This is persistent until changed.

If `berrybrew` is managing Perl file association, we will update the association
with the newly switched-to version of perl (requires running as Administrator).

If `switchQuick` is sent in as true, we'll update the system without requiring you
to open a new command line window. However, some binaries and features may not work
correctly when switching quickly.

#### SwitchQuick

    public void SwitchProcess()
    
Called by [Switch](#switch), sets up the new environment so we don't need to
close the current `cmd` window and open a new one for environment variables
to be refreshed.

#### Unconfig

    public void Unconfig()

Removes Berrybrew from PATH.

#### UseCompile

    public void UseCompile(string usePerlStr, bool newWindow = false)

        argument:   usePerlStr
        value:      Comma-separated list of strawberry perl instances

        argument:   newWindow
        value:      true/false whether new windows should be spanwed or not
        default:    false

Sets things up before handing each command off to `UseInNewWindow()` or
`UseInSameWindow()` for final processing. If the `--win` flag (or
`--window` or `--windowed`) is included, we'll call `UseInNewWindow()`,
otherwise call `UseInSameWindow()`.

This method sends a single Perl at a time to the appropriate
`UseIn*Window()` function, once for each of the specified Perls from
usePerlStr.

#### UseInNewWindow

    private void UseInNewWindow(StrawberryPerl perl, string sysPath, string usrPath)

        argument:   perl
        value:      A single StrawberryPerl object

        argument:   sysPath
        value:      String containing the full Machine PATH environment variable

        argument:   usrPath
        value:      String containing the full User PATH environment variable

Called by `UseCompile()`: Creates a new window for a single Perl environment,
with that Perl listed first in the PATH inherited by the new process.

#### UseInSameWindow

    private void UseInSameWindow(StrawberryPerl perl, string sysPath, string usrPath)

        argument:   perl
        value:      A single StrawberryPerl object

        argument:   sysPath
        value:      String containing the full Machine PATH environment variable

        argument:   usrPath
        value:      String containing the full User PATH environment variable

Called by `UseCompile()`: Creates a new command processor in the active berrybrew
window, with the selected Perl listed first in the PATH inherited by the new
process.

#### Version

    public string Version()

        return:     berrybrew version string

Returns the version of the current `berrybrew` binary/library.

## Message Class Methods 

Manages the importing, collection and printing of various `berrybrew` output.

It's source file is`src/messaging.cs` and its namespace is `BerryBrew.Messaging`.

#### Message.Add

    public void Add(dynamic json)

        argument:   Deserialized JSON string

        value:      {"label":"msgname","content":["msgline 1", "msgline 2"]}

        converted to: Dictionary<(string)label, (List<string>)content>

Adds a message to the structure.

#### Message.Error

    public void Error(string label)
    
         argument:   label
         value:      Name of a message label

Prints the relevant message to `STDERR` as opposed to `STDOUT`.

#### Message.Get

    public string Get(string label)

        argument:   label
        value:      Name of a label that coincides with the message content

        return:     String of the message content

Returns the message content that corresponds with a specific message label.

#### Message.Print

    public void Print(string label)

        argument:   label
        value:      Name of a message label

`Console.WriteLine()` the message content corresponding with the labelto
`STDOUT`

#### Message.Say

    public void Say(string label)

        argument:   label
        value:      Name of a message label

Same thing as `Message.Print`, but after printing, calls `Environment.Exit(0)`
and terminates the application.

## PathOp Class Methods

#### PathOp.PathAddBerryBrew

    internal void PathAddBerryBrew(string binPath)

        argument:   binPath
        value:      Full path to the directory the berrybrew.exe binary resides in

Called by `Config()`, this enables `berrybrew` to be called from the command
line without having to specify the full path to the executable.

#### PathOp.PathAddPerl

    internal void PathAddPerl(StrawberryPerl perl)

        argument:   perl
        value:      Single instance of the StrawberryPerl class

Sets the `PATH` environment variables up to ensure the version of Perl
housed in the `perl` object will be used on the system.

#### PathOp.PathGet

    public static string PathGet()

        return:     String containing the machine's PATH data

Using the registry, retrieves the current Machine (System) `PATH` environment
variable. Using the registry ensures we have the most current data, even if
the current shell has not yet been updated.

Does not expand any variable-based `PATH` entries on extraction.

#### PathOp.PathGetUsr

    internal static string PathGetUsr()
    
        return: String containing the currently logged in user's PATH environment variable

Fetches and returns a string containing the currently logged in user's `PATH`
environment variable.

Does not expand any variable-based `PATH` entries on extraction.

#### PathOp.PathRemoveBerrybrew

    public void PathRemoveBerrybrew()

Removes berrybrew binary directory from `PATH`.

#### PathOp.PathRemovePerl

    public void PathRemovePerl(bool process=true)

        argument:   process
        value:      bool
        default:    false
        purpose:    Action a PathSet()

Removes any and all Perl instances from the `PATH` environment variable.
If `process` is set to `true` (default), we'll execute the removal via
`PathSet()`.

#### PathOp.PathScan

    internal static bool PathScan(string binPath, string target)

        argument:   binPath
        value:      string that contains the path to check against 

        argument:   target
        value:      "machine" or "user"

        return:     true if found, false if not

Looks through either the Machine or User `PATH` environment variables,
searching for the binary name. Returns `true` on success, `false` otherwise.

#### PathOp.PathSet

    internal void PathSet(List<string> paths)

        argument:   paths
        value:      List of strings, each string contains a PATH entry
                    (less the semi-colon)

Builds the semi-colon separated `PATH` string from the list, and inserts it
into the Machine's `PATH` section in the registry. We then send a broadcast
message to the system to advise of the change.

We use this manual method as opposed to C# methods, because we change the
registry value from a `REG_SZ` type to `REG_EXPAND_SZ` type so that we can
preserve and insert variable-based `PATH` entries.

## PerlOp Class Methods

#### PerlOp.PerlArchivePath

    internal static string PerlArchivePath(StrawberryPerl perl)

        argument:   perl
        value:      Instance of the StrawberryPerl class

        return:     The full path plus filename of the Perl install

Creates the directory that will house a new Perl installation.

#### PerlOp.PerlGenerateObjects

    internal List<StrawberryPerl> PerlGenerateObjects(bool importIntoObject=false)

        argument:   importIntoObject
        default:    false
        purpose:    Insert the Perl objects into the Berrybrew object

        returns:    List of StrawberryPerl instance objects.

Collects up both the default and custom available Perls from the available
JSON configuration files, and turns the information into `StrawberryPerl`
objects.

Set `importIntoObject` to `true` to have the list of objects imported into the
`Berrybrew` object, at `this.Perls`.

#### PerlOp.PerlInUse

    public StrawberryPerl PerlInUse()

        return:     Instance of the StrawberryPerl class

Locates which instance of Perl is currently in use, and returns the
`StrawberryPerl` object that represents it.

#### PerlOp.PerlIsInstalled

    internal static bool PerlIsInstalled(StrawberryPerl perl)

        argument:   perl
        value:      Instance of the StrawberryPerl class

        return:     true if the passed in perl is installed, false if not

Checks to see whether a specific Perl instance is installed. Returns `true`
if it is, and `false` if not.

#### PerlOp.PerlsInstalled

    public List<StrawberryPerl> PerlsInstalled()
    
        return: A list of the Strawberry Perl objects currently installed

Fetches the list of currently installed Perl instances, and returns a list of objects.

Removes the Perl instance corresponding to the name sent in.

#### PerlOp.PerlOrphansFind

    internal List<string> PerlOrphansFind()

        returns:    List of the names of orphaned Perl installs found

Gathers a list of directory names in the Perl installation directory, that
don't have any association or registration with `berrybrew`.

#### PerlOp.PerlOrphansIgnore

    internal Dictionary<string, bool> PerlOrphansIgnore()

Returns a dictionary where each key is a subdirectory within the Perl `rootDir`
that should never be classified as an orphan. The value is ignored, but all
default to `true`.

#### PerlOp.PerlRegisterCustomInstall

    public void PerlRegisterCustomInstall(
        string perlName,
        StrawberryPerl perlBase = new StrawberryPerl()
    )

        argument:   perlName
        value:      The name you want to use for this new install, which will
                    appear in "berrybrew list" and "berrybrew available"

        argument:   perlBase
        value:      Instance of the StrawberryPerl class
        default:    A non-populated instance

Registers custom Perl instances with `berrybrew`, so they appear in
`berrybrew list` and `berrybrew available` and aren't considered orphans.

If a populated instance is sent in as `perlBase`, we'll use its configuration
information (version, path info, download info etc) in the new custom one. If
you do this, be sure that the base and the new custom instances are the same
version.

#### PerlOp.PerlRegisterVirtualInstall

    public void PerlRegisterVirtualInstall(string perlName)

        argument:   perlName
        value:      The name you want to use for this new install, which will
                    appear in "berrybrew available"


Creates a virtual berrybrew instance wrapped around an existing Perl installation.

This can be ActiveState, Strawberry or any other "system" Perl.

#### PerlOp.PerlRemove

    public void PerlRemove(string versionToRemove)

        argument:   versionToRemove
        value:      Name of an installed Perl to uninstall

#### PerlOp.PerlResolveVersion

    internal StrawberryPerl PerlResolveVersion(string name)

        argument:   name
        value:      Name of a Perl as seen in 'berrybrew available'

        return:     The corresponding StrawberryPerl instance object

Resolves the name of a Perl that's available (per `berrybrew available`), and returns
the corresponding object.

#### PerlOp.PerlUpdateAvailableList

    public void PerlUpdateAvailableList()

Fetches the JSON list of Strawberry Perl instances available from
[Strawberry's releases.json](https://strawberryperl.com/releases.json), and
updates the internal `perls.json` available list with the updated data.

#### PerlOp.PerlUpdateAvailableListOrphans

    public void PerlUpdateAvailableListOrphans()

Automatically register any orphaned Perls after using the `Fetch()` method. This
should only be called after a call to `PerlUpdateAvailableList()`.

&copy; 2016-2023 by Steve Bertrand