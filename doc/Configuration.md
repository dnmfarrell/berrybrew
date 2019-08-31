## berrybrew Configuration

### [Global config](#global-config)

|Directive|Description|
|---|---|   
|[debug](#debug)|Enable/disable debugging|
[root_dir](#root_dir)|Location of the Perl installations|
[temp_dir](#temp_dir)|Location of `berrybrew` temporary files|
[strawberry_url](#strawberry_url)|The Strawberry Perl website|
[download_url](#download_url)|The URL for the Strawberry release JSON file|
[custom_exec](#custom_exec)|Include custom (cloned) instances under `berrybrew exec`|
[windows_homedir](#windows_homedir)|Default home directory for `File::HomeDir`|
    
### [Messages config](#messages-config)

### [Perls available config](#perl-config)

#### Global Config

Handles application wide configuration.

File location:

    data\config.json

Available options:

###### debug

Enables debugging output.

Default: `false`

Values: "true" or "false"

###### root_dir

Directory where we'll house all of your Perl installations. 

Default: `C:\berrybrew`

Values: Any directory accessible on the system.

###### temp_dir

Directory where we store the Perl installation zip files.

Default: `C:\berrybrew\temp`

Values: Any directory accessible on the system.

###### strawberry_url

Link to the Strawberry Perl website.

Default: `http://strawberryperl.com`

###### download_url

Link to the Strawberry Perl instance release JSON file.

Default: `http://strawberryperl.com/releases.json`

###### custom_exec

Include custom (cloned) instances when using `berrybrew exec`.

Default: `false`

Values:  "true", "false"

##### windows_homedir

When using `File::HomeDir` with Portable Strawberry Perl instances,
the default home directory location is different than a full-blown
Strawberry Install.

If you set this directive to `true`, we'll revert back to the way
the full-blown install does things.

Default: `false`

Values: "true", "false"

#### Messages Config

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

#### Perl Config

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
