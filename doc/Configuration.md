## berrybrew Configuration

The Global Configuration file, `data\config.json` is no longer used for runtime
operation. It's used for initial configuration at install time only. Any changes
to the configuration file after installation will have no effect on **berrybrew**.

For changing configuration options after install, see [**berrybrew options**](berrybrew.md#options).

Most options are also configurable through the UI.

Config sections:

- [Global config](#global-config)
- [Messages config](#messages-config)
- [Perl config](#perl-config)

### Global Config

Handles application wide configuration. 

**Note**: The defaults from the global configuration file  are loaded on first use
and then put into the Windows Registry. The global configuration file is not
used after this initial import so changing any settings after your first run of
`berrybrew` won't have any effect.

To modify configuration options, please run `berrybrew options help` instead.

File location:

    data\config.json

Directive list:

| Directive                            | Description                                                |
| --- | --- |
| [debug](#debug)                      | Enable/disable debugging                                   |
| [storage_dir](#storage_dir)          | Top level directory for Perl instance management           |
| [instance_dir](#instance_dir)        | Location of the Perl installations                         |
| [temp_dir](#temp_dir)                | Location of the Strawberry Perl instance zip archive files |
| [strawberry_url](#strawberry_url)    | The Strawberry Perl website                                |
| [download_url](#download_url)        | The URL for the Strawberry release JSON file               |
| [custom_exec](#custom_exec)          | Include custom (cloned) instances under `berrybrew exec`   |
| [windows_homedir](#windows_homedir)  | Default home directory for `File::HomeDir`                 |
| [run_mode](#run_mode)                | Used to identify prod, staging or testing environment      |
| [shell](#shell)                      | Shell to run when 'use'ing a Perl                          |
| [file_assoc](#file_assoc)            | The current .pl file association handler                   |
| [file_assoc_old](#file_assoc_old)    | The previous .pl file association handler                  |
| [warn_orphans](#warn_orphans)        | Warn if non-Perl directories are found                     |

Available options:

#### debug

Enables debugging output.

Default: `false`

Values: "true" or "false"

#### storage_dir

Top level directory for the `berrybrew` environments Perl instance management
data.

Default: `C:\berrybrew`

#### instance_dir

Directory where we'll house all of your Perl installations. 

Default: `C:\berrybrew\instance`

Values: Any directory accessible on the system.

#### temp_dir

Directory where we store the Perl installation zip files.

Default: `C:\berrybrew\temp`

Values: Any directory accessible on the system.

#### download_url

Link to the Strawberry Perl instance release JSON file.

Default: `http://strawberryperl.com/releases.json`

#### custom_exec

Include custom (cloned) instances when using `berrybrew exec`.

Default: `false`

Values:  "true", "false"

#### windows_homedir

When using `File::HomeDir` with Portable Strawberry Perl instances,
the default home directory location is different than a full-blown
Strawberry Install.

If you set this directive to `true`, we'll revert back to the way
the full-blown install does things.

Default: `false`

Values: "true", "false"

#### run_mode

This option is used by the system to idenfity whether it's running in `prod`,
`staging` or `testing` mode. 

It should not be modified by the end user.

#### shell

The shell to deploy when `berrybrew use` is executed. Valid options are `cmd`
and `powershell`.

#### file_assoc

This is a dynamic option used internally, and should never be modified by the
end user.

#### file_assoc_old

This is a dynamic option used internally, and should never be modified by the
end user.

#### warn_orphans

Disabled by default, enabling this option will warn if there are any directories
within the Perl installation directory that aren't registered with `berrybrew`.

### Messages Config

Maps `STDOUT` message labels to their corresponding content. This configuration file is used by the `Message` class.

File location:

    data\messages.json

The format of the file is as follows:

    [
        {
            "label": "perl_not_found",
            "content": [
                "Perl not found\n",
                "the Perl you're trying to install isn't available"
            ]
        },
        {
            "label": "error",
            "content": [
                "this is a generic error message",
            ]
        }
    }

### Perl Config

Contains information on all Perls we have available.

File location:

    data\perls.json

Example of the file's contents:

    [
        {
            "name" : "5.24.0_64",
            "file" : "strawberry-perl-5.24.0.1-64bit-portable.zip",
            "url"  : "http://strawberryperl.com/download/5.24.0.1/strawberry-perl-5.24.0.1-64bit-portable.zip",
            "ver"  : "5.24.0",
            "csum" : "40094b93fdab1057598e9474767d34e810a1c383"
        },
        {
            "name" : "5.22.2_32",
            "file" : "strawberry-perl-5.22.2.1-32bit.zip",
            "url"  : "http://strawberryperl.com/download/5.22.2.1/strawberry-perl-5.22.2.1-32bit.zip",
            "ver"  : "5.22.2",
            "csum" : "6c750c56a4eccf3b5f77af56e4cee572c360a1c2"
        }
    ]

&copy; 2016-2023 by Steve Bertrand