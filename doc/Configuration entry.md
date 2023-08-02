# Configuration entries

- [Configuration implementation process](#configuration-implementation-process)
- [Adding a new config item](#adding-a-new-configuration-item)

### Configuration implementation process

The flow of configuration entry items are as follows

- Entry with its default value goes into `dev\data\config.json`
- When building a release, this file is copied to productions `data\`
directory
- Upon install with the installer, it calls `berrybrew options-update-force`
which installs any new config entries and updates any updated existing values
to the new values to the registry

After adding a new configuration entry during development, you **must** make a
call to `staging\berrybrew options-update-force` to have them installed into
the registry for them to have effect.

The same goes for unit testing. After execution of `dev\build_testing.bat`, you
must then call `testing\berrybrew options-update-force`.

### Adding a new configuration directive

For this example, we'll use the existing `instance_dir` directive. This has a
string value. The `instance_dir` directive tells `berrybrew` where the installed
perl instances reside on the file system.

###### Add the actual config directive

Place your new config directive into the `dev\data\config.json`, along with its
default value: 

    "instance_dir"    : "C:\\berrybrew\\instance",

###### Add directive to valid options

Add the directive to the `validOptions` list in `src\berrybrew.cs` source file

###### Add directive to `Info()`

If, and **only** if the directive contains a file path, it must be added to the
`options` list in the `Info()` method. If it doesn't contain a path, don't do
this.

###### Create a typed variable to house the value

This variable should be declared in the class definition. In the case of
`instance_dir`, I used `public string instancePath`.

###### Assign directive value to the variable

This happens in the `BaseConfig()` method, and is done through a call to the
registry:

    instancePath = (string) registry.GetValue("instance_dir", "");
    instancePath += @"\";

###### Push the new directive to the registry

The following builds the updated system, pushes the value to the registry, and
displays the option.

    dev\build_staging.bat

    staging\berrybrew options-update-force

    staging\berrybrew options

    staging\berrybrew options install_dir

###### Add new option to `messages.json`

If the new directive is a file path, we need to add it to `berrybrew info`. If
it isn't a path, skip this part.

Add the directive name to the `info_option_required` label in 
`dev\data\messages.json`.

###### Add directive to the unit test scripts

In `t\03-options.t`, place your directive, along with its value in the
`%test_options` hash.

If and **only** if your directive's value contains a path, also put it into the
`%valid_opts` hash in `t\65-info.t` test file.

###### Run unit tests

You first need to get the new directive implemented in the test environment:

- `dev\build_testing.pl`
- `testing\berrybrew options-update-force`

Then run the individual test files:

- `perl t\03-options.t`
- `perl t\65-info.t`

If no fallout, run the entire test suite:

- `t\test.bat`

Fix any issues and you're done.

&copy; 2016-2023 by Steve Bertrand