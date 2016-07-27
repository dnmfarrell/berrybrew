##berrybrew API (bin/bbapi.dll)

API source code is located in the `src/berrybrew.cs` file. It is
standalone namespace/class code, and contains no entry points.

The code for the `berrybrew.exe` binary itself resides in `src/bbconsole.cs`.
This source file contains the `Main()` entry point. 

- [Berrybrew Class](#class-berrybrew)
- [Message Class](#class-message)

##Berrybrew Class

The `Berrybrew` class is the base of the system.

|Method name|Available|Description|
|---|---|---|
[Available](#available)| **public** | Displays all available Perls
[CheckName](#checkname)| internal | Validates the name of a custom Perl install
[CheckRootDir](#checkrootdir)| internal | Creates the Perl install directory if required
[Clean](#clean) | **public** | Stages removal of temp files and orphaned Perls
[CleanOrphan](#cleanorphan)| internal | Removes all orphaned Perls
[CleanTemp](#cleantemp)| internal | Removes temporary files
[Clone](#clone)| **public** | Copies an installed Perl to a new name
[Config](#config)| **public** | Puts `berrybrew.exe` in `PATH`
[Exec](#exec)| internal | Runs commands on all installed Perls
[ExecCompile](#execcompile)| **public** | Staging for `Exec()`
[Extract](#extract)| private | Extracts Perl installation zip archives  
[Fetch](#fetch)| private | Downloads the Perl installation files
[FileRemove](#fileremove)| internal | Deletes a file
[FileSystemResetAttributes](#filesystemresetattributes)| internal | Defaults filesystem attrs
[Install](#install)| **public** | Installs new instances of Perl
[JsonParse](#jsonparse)| internal | Reads JSON config files
[JsonWrite](#jsonwrite)| internal | Writes out JSON configuration
[Off](#off) | **public** | Completely disables `berrybrew`
[PathAddBerryBrew](#pathaddberrybrew)| internal | Adds `berrybrew` to `PATH`
[PathAddPerl](#pathaddperl)| internal | Adds a Perl to `PATH`
[PathGet](#pathget)| internal | Retrieves the Machine `PATH`
[PathRemoveBerrybrew](#pathremoveberrybrew)| internal | Removes berrybrew from `PATH`
[PathRemovePerl](#pathremoveperl)| internal | Removes specified Perl from `PATH`
[PathScan](#pathscan)| internal | Checks `PATH` for a specific binary file
[PathSet](#pathset)| internal | Writes all `PATH` changes to the registry
[PerlArchivePath](#perlarchivepath)| internal | Returns the path and filename of the zip file
[PerlFindOrphans](#perlfindorphans)| **public** | Locates non-registered directories in Perl root
[PerlGenerateObjects](#perlgenerateobjects)| internal | Generates the `StrawberryPerl` class objects
[PerlInUse](#perlinuse)| internal | Returns the name of the Perl currently in use
[PerlIsInstalled](#perlisinstalled)| internal | Checks if a specific Perl is installed
[PerlRemove](#perlremove)| **public** | Uninstalls a specific instance of Perl
[PerlRegisterCustomInstall](#perlregistercustominstall)| internal | Make `berrybrew` aware of custom instances
[PerlResolveVersion](#PerlResolveVersion)| internal | Resolves the name of a Perl to its StrawberryPerl object
[PerlUpdateAvailableList](#PerlUpdateAvailableList)| **public** | **incomplete** Automatically fetches new Strawberry Perls available
[Switch](#switch)| **public** | Change to a specific version of Perl (persistent)
[Unconfig](#unconfig)| **public** | Removes berrybrew bin dir from `PATH`
[Upgrade](#upgrade)| **public** | Performs a safe `berrybrew` upgrade
[Version](#version)| **public** | Return the version of the current `berrybrew`

##Message Class Methods

The `Message` class is a helper that manages the various output
that is displayed to the user.

|Method name|Available|Description|
|---|---|---|
[Message.Add](#Message.Add)| **public** | Adds a new message to the collection
[Message.Get](#Message.Get)| **public** | Fetches the content of a specific message
[Message.Print](#Message.Print)| **public** | Prints the content of a specific message
[Message.Say](#Message.Say)| **public** | Same as `Print()`, but terminates


##Class Berrybrew

####Available

    public void Available()

Displays the names of the versions of Perl that are available to `berrybrew`,
as found in `this.Perls`, where `this.Perls` is a 
`OrderedDictionary<string name, Berrybrew.StrawberryPerl>`.

####CheckName

    private static bool CheckName(string perlName)

        argument:   perlName
        value:      Name of an available Perl

        return:     true on success, false on fail

Checks the name of a custom Perl to ensure it fits within the guidelines.

####CheckRootDir

    internal void CheckRootDir()

Checks whether the Perl root installation directory exists, and creates it if not.

####Clean

    public void Clean(string subcmd="temp")

        argument:   subcmd
        values:     "temp", "orphan", "all" 

By default, `subcmd` is set to "temp", which we delete all downloaded Perl
installation zip files from the temporary directory. With "orphan", we'll
delete all directories found in the Perl installation root directory that
`berrybrew` has not registered as valid Perl installs.

####CleanOrphan

    internal bool CleanOrphan()

Removes all directories found in the Perl installation directory that aren't
associated with any registered Perl instances.

Returns `true` if any orphans were found/deleted, `false` if not.

####CleanTemp

    internal bool CleanTemp()
    
Removes all Perl installation zip files from the temporary staging directory.

Returns `true` if any files were found/deleted, `false` if not.

####Clone

    public bool Clone(string src, string dest)

        argument:   src
        values:     Name of an installed berrybrew Perl instance

        argument:   dest
        values:     Any string name by which you want the clone to appear
                    in 'berrybrew available'

        return:     bool

Makes an exact copy of an existing installed Perl instance with a name of your
choosing, and makes it available just like all others. `berrybrew available`
will label these custom installs appropriately. Returns `true` on success,
`false` otherwise.

####Config

    public void Config()

Adds the path to the `berrybrew.exe` executable into the `PATH` environment
variable.

####Exec
    
    internal void Exec(StrawberryPerl perl, string command, string sysPath)

        argument:   perl
        value:      A single StrawberryPerl object
        
        argument:   command
        value:      The full command string you want all installed Perls to execute

        argument:   sysPath
        value:      String containing the full Machine PATH environment variable

Called by `ExecCompile()`, sends a single Perl instance a command to execute.

####ExecCompile

    public void ExecCompile(string parameters)

        argument:   parameters
        value:      Full command string that Exec() hands off, including
                    any Exec() specific instructions

Sets things up before handing each command off to `Exec()` for final
processing. If the `--with` flag is included, we'll strip it off and only
send the commands to be executed to those specific Perls.

This method sends a single Perl at a time to `Exec()`, and will always skip
any Perls that have either `tmpl` or `template` in the name.

By default, we also skip over all custom (cloned) instances. To have them
included, set `custom_exec` to `true` in the configuration file.

####Extract()

    private void Extract(StrawberryPerl perl, string tempDir)

        argument:   perl
        value:      A single instance of the StrawberryPerl class

        argument:   tempDir
        value:      The full path to the temporary Perl installation staging directory
        typical:    this.archivePath

Extracts a Perl instance zip archive into the Perl installation directory.

####Fetch

    public string Fetch(StrawberryPerl perl)

        argument:   perl
        value:      Single instance of the StrawberryPerl class
        
        return:     The name of the folder the zip file was downloaded to

Downloads the zip file for the version of Perl found in the StrawberryPerl
object, and returns the directory of where it was put.

####FileRemove

    internal static string FileRemove(string filename)

        argument:   filename
        value:      Name of an existing file on the system
        
        return:     Stringified Exception or "true"

Deletes a file from the file system. Returns stringified "true" on success,
and a stringified `IO` exception on failure.

####FileSystemResetAttributes

    internal void FileSystemResetAttributes(string dir)

        argument:   dir
        value:      Name of a directory that exists in the filesystem
    
Recursively resets all files and directories within the directory being
operated on back to default. This method was written specifically to ensure
that no files were readonly, which prevented us from removing Perl
installations.

####Install

    public string Install(string version)

        argument:   version
        value:      Name of an available Perl, as seen with 'berrybrew available'
        
        return:     The name of the Perl we've installed

Installs and registers a new instance of Perl.

####JsonParse

    internal dynamic JsonParse(string type, bool raw=false)

        argument:   type
        value:      The name of the JSON file, with the '.json' extension removed

        argument:   raw
        value:      bool
        default:    false

        return:     dynamic // object or JSON string

Extracts the JSON string from the various JSON files, and returns it. If `raw`
is set to `false` (default), we send the data back de-serialized. If `raw` is
`true`, we'll send back the JSON string as-is, with no de-serialization.

####JsonWrite

    internal void JsonWrite(
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

####Off

    public void Off()

Disabled all `berrybrew` managed Perls, by removing them from `PATH`
environment variables. This will return you to a system Strawberry or
ActiveState system installed Perl.

####PathAddBerryBrew

    internal void PathAddBerryBrew(string binPath)

        argument:   binPath
        value:      Full path to the directory the berrybrew.exe binary resides in

Called by `Config()`, this enables `berrybrew` to be called from the command
line without having to specify the full path to the executable.

####PathAddPerl

    internal void PathAddPerl(StrawberryPerl perl)

        argument:   perl
        value:      Single instance of the StrawberryPerl class

Sets the `PATH` environment variables up to ensure the version of Perl
housed in the `perl` object will be used on the system.

####PathGet

    internal static string PathGet()

        return:     String containing the machine's PATH data

Using the registry, retrieves the current Machine (System) `PATH` environment
variable. Using the registry ensures we have the most current data, even if
the current shell has not yet been updated.

Does not expand any variable-based `PATH` entries on extraction.

####PathRemoveBerrybrew

    internal void PathRemoveBerrybrew()

Removes berrybrew binary directory from `PATH`.

####PathRemovePerl

    internal string PathRemovePerl(bool process=true)

        argument:   process
        value:      bool
        default:    false
        purpose:    Action a PathSet()

        return:     String containing the full PATH data, after removal

Removes any and all Perl instances from the `PATH` environment variable.
If `process` is set to `true` (default), we'll execute the removal via
`PathSet()`. If `false`, we'll simply return the modified `PATH` string,
but we won't modify the environment.

####PathScan

    internal static bool PathScan(Regex binPattern, string target)

        argument:   binPattern
        value:      Regex object containing an executable's filename

        argument:   target
        value:      "machine" or "user"

        return:     true if found, false if not

Looks through either the Machine or User `PATH` environment variables,
searching for the binary name. Returns `true` on success, `false` otherwise.

####PathSet

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

####PerlArchivePath

    internal static string PerlArchivePath(StrawberryPerl perl)

        argument:   perl
        value:      Instance of the StrawberryPerl class

        return:     The full path plus filename of the Perl install

Creates the directory that will house a new Perl installation.

####PerlFindOrphans

    public List<string> PerlFindOrphans()

        returns:    List of the names of orphaned Perl installs found

Gathers a list of directory names in the Perl installation directory, that
don't have any association or registration with `berrybrew`.

####PerlGenerateObjects

    internal List<StrawberryPerl> PerlGenerateObjects(
        bool importIntoObject=false
    )

        argument:   importIntoObject
        default:    false
        purpose:    Insert the Perl objects into the Berrybrew object

        return:     List of all Perls available, as StrawberryPerl objects

Collects up both the default and custom available Perls from the available
JSON configuration files, and turns the information into `StrawberryPerl`
objects.

Set `importIntoObject` to `true` to have the list of objects imported into the 
`Berrybrew` object, at `this.Perls`.

####PerlInUse

    internal StrawberryPerl PerlInUse()

        return:     Instance of the StrawberryPerl class

Locates which instance of Perl is currently in use, and returns the 
`StrawberryPerl` object that represents it.

####PerlIsInstalled

    internal static bool PerlIsInstalled(StrawberryPerl perl)

        argument:   perl
        value:      Instance of the StrawberryPerl class

        return:     true if the passed in perl is installed, false if not

Checks to see whether a specific Perl instance is installed. Returns `true`
if it is, and `false` if not.

####PerlRemove

    public void PerlRemove(string versionToRemove)

        argument:   versionToRemove
        value:      Name of an installed Perl to uninstall

Removes the Perl instance corresponding to the name sent in.

####PerlRegisterCustomInstall

    public void PerlRegisterCustomInstall(
        string perlName,
        StrawberryPerl perlBase = new StrawberryPerl()
    )

        argument:   perlName
        value:      The name you want to use for this new install, which will
                    appear in "berrybrew available"

        argument:   perlBase
        value:      Instance of the StrawberryPerl class
        default:    A non-populated instance

Registers custom Perl instances with `berrybrew`, so they appear in
`berrybrew available` and aren't considered orphans.

If a populated instance is sent in as `perlBase`, we'll use its configuration
information (version, path info, download info etc) in the new custom one. Be
sure if you do this that the base and the new custom instances are the same 
version.

####PerlResolveVersion

    internal StrawberryPerl PerlResolveVersion(string name)

        argument:   name
        value:      Name of a Perl as seen in 'berrybrew available'

        return:     The corresponding StrawberryPerl instance object

Resolves the name of a Perl that's available (per `berrybrew available`), and returns 
the corresponding object.

####PerlUpdateAvailableList

    public void PerlUpdateAvailableList()

INCOMPLETE - DO NOT USE. Fetches the list of available Strawberry Perls on [Strawberry's download site](https://strawberryperl.com/releases.html), and updates the internal `perls.json` available list.

####Switch

    public void Switch(string perlVersion)

        argument:   perlVersion
        value:      Name of an available and installed Perl instance

Updates `PATH` with the relevant path details in order to make this Perl
instance the default used across the board. This is persistent until changed.

####Unconfig

    public void Unconfig()

Removes Berrybrew from PATH.

####Upgrade

    public void Upgrade()

Creates a `backup_timestamp` backup directory in the repository root directory,
copies the live configuration files from `data` directory, performs a
`git pull`, then copies the backup files back to the `data` dir.

####Version

    public string Version()

        return:     berrybrew version string

Returns the version of the current `berrybrew` binary/library.

##Class Message

Manages the importing, collection and printing of various `berrybrew` output.

####Message.Add

    public void Add(dynamic json)
    
        argument:   Deserialized JSON string

    value:      {"label":"msgname","content":["msgline 1", "msgline 2"]}
                converted to:
                Dictionary<(string)label, (List<string>)content>

Adds a message to the structure. 

####Message.Get

    public string Get(string label)

        argument:   label
        value:      Name of a label that coincides with the message content

        return:     String of the message content
    
Returns the message content that corresponds with a specific message label.

####Message.Print

    public void Print(string label)
    
        argument:   label
        value:      Name of a message label
    
`Console.WriteLine()` the message content corresponding with the labelto
`STDOUT`

####Message.Say

    public void Say(string label)
    
        argument:   label
        value:      Name of a message label
        
Same thing as `Message.Print`, but after printing, calls `Environment.Exit(0)`
and terminates the application.

&copy; 2016 by Steve Bertrand
