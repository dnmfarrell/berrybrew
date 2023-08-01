# Add a New Binary Command

In this tutorial, we'll create four new commands. Each will increase in
complexity.

If your new command doesn't use an existing API method, see
[adding a new API method](Add%20a%20New%20API%20Method.md). It is assumed here
that this has already been done.

After the new command is implemented, unit tests must be added.

### Table of Contents

  - [Simple command](#simple-command)
  - [Command with an option](#command-with-an-option)
  - [Command with subcommand](#command-with-subcommand)
  - [Command with subcommands and options](#command-with-subcommands-and-options)
  - [Hidden command](#hidden-command)

### Simple command

For this procedure, I'll use the existing `berrybrew list` command as the
example.

###### Add the command to `bbconsole.cs`

Your new command must be a `case` statement listed within the `switch` section,
in alphabetical order with the rest. Your `case` must have a call to an API
method, a call to `bb.Exit()` with an appropriate error code (almost always `0`).

    case "list":
        bb.List();
        bb.Exit(0);
        break;

###### Add the command to the help screen

In `dev\data\messages.json`, find the `help` label, and in the `content` array,
add the new command in the exact format that already exists:

    "    list           List installed Strawberry Perl versions",

###### Update the berrybrew documentation

Add a relevant entry, in alphabetical order, to the
[berrybrew documentation](berrybrew.md).

First, add an entry in the [Command List](berrybrew.md#command-list) section:
 
    - [list](#list)

Then add your command and descriptions to the
[Command Usage](berrybrew.md#command-usage) section (in alphabetical order):

    #### list

        berrybrew list

    Takes no options, displays a list of the currently installed Perl instances:

    berrybrew list
            5.26.2_64
            5.10.1_32

### Command with an option

For this section, we'll use `berrybrew install` command.

###### Add the command to `bbconsole.cs`

Your new command must be a `case` statement listed within the `switch` section,
in alphabetical order with the rest. Your `case` must have a call to an API
method, a call to `bb.Exit()` with an appropriate error code (almost always `0`).

When we have an option, there needs to be validation checking on the parameters
which often requires using exception handling as well as using our `Message`
class to display pertinent usage information.

    case "install":
        if (args.Length == 1) {
            bb.Message.Print("install_ver_required");
            bb.Exit(0);
        }

        try {
            bb.Install(args[1]);
            bb.Exit(0);
        }
        catch (ArgumentException error){
            if (bb.Debug) {
                Console.WriteLine(error);
            }

            bb.Message.Print("install_ver_unknown");
            Console.Error.WriteLine(error);
            bb.Exit(-1);
        }
        break;

###### Add a `Message` class entry for missing option

These go into the `dev\data\messages.json` file.

You can see where the content of this `Message` label is used above.

    {
      "label": "install_ver_required",

      "content": [
        "\ninstall command requires a version argument\n",
        "Use the available command to see what versions of Strawberry Perl are available"
      ]
    },

###### Add the command to the help screen

In `dev\data\messages.json`, find the `help` label, and in the `content` array,
add the new command in the exact format that already exists:

    "    install        Download, extract and install a Strawberry Perl",

###### Update the berrybrew documentation

Add a relevant entry, in alphabetical order, to the
[berrybrew documentation](berrybrew.md).

First, add an entry in the [Command List](berrybrew.md#command-list) section:

    - [install](#install)

Then add your command and descriptions to the
[Command Usage](berrybrew.md#command-usage) section (in alphabetical order). In
this case, the option is mandatory, so we surround it with `<>`.

    #### install

    Usage:  `berrybrew install <version>`

    Installs a single Perl version as seen in `berrybrew available`, and makes it
    available for use.

### Command with a subcommand

In this case, we'll use the `berrybrew associate` command as our example. It's a
command that has two possible sub commands available.

###### Add the command to `bbconsole.cs`

Your new command must be a `case` statement listed within the `switch` section,
in alphabetical order with the rest. Your `case` must have a call to an API
method, a call to `bb.Exit()` with an appropriate error code (almost always `0`).

When we have subcommands, we add special entries into the `Message` class that
output help information that supports the command. When subcommands are present
for a command, we allow help to be displayed for the subcommands with
`berrybrew associate help`.

In the case here, the subcommands are not mandatory. If none are sent in, we
simply display the current settings.

    case "associate":
        if (args.Length > 1) {
            if(args[1] == "-h" || args[1] == "help") {
                bb.Message.Print("subcmd.associate");
                bb.Exit(0);
            }
            else {
                bb.FileAssoc(args[1]);
                bb.Exit(0);
            }
        }
        bb.FileAssoc();
        bb.Exit(0);
        break;

###### Add a `Message` class entry for the subcommand options

These go into the `dev\data\messages.json` file.

You can see where the content of this `Message` label is used above.

    {
        "label": "subcmd.associate",

        "content": [
          "",
          "   berrybrew 'associate' options:",
          "",
          "   set    '.pl' file extensions will be managed by berrybrew",
          "",
          "   unset  Restore the previous file association configuration"
        ]
    },

###### Add the command to the help screen

In `dev\data\messages.json`, find the `help` label, and in the `content` array,
add the new command in the exact format that already exists.

**NOTE**: When a command has subcommands, we denote this with an asterisk after
the command name.

    "    associate *    View and set Perl file association",

###### Update the berrybrew documentation

Add a relevant entry, in alphabetical order, to the
[berrybrew documentation](berrybrew.md).

First, add an entry in the [Command List](berrybrew.md#command-list) section:

    - [associate](#associate)

Then add your command and descriptions to the
[Command Usage](berrybrew.md#command-usage) section (in alphabetical order). In
this case, the subcommand/option is optional, so we surround it with `[]`.

    #### associate

        berrybrew associate [command]

    View, set or revert `.pl` file association on the system.

### Command with subcommands and options

This is by far the most complex type of command to add. It typically involves a
significant amount of parameter validations, exception handling and flow
control.

We'll use the `berrybrew snapshot <command> <option> [option]` command for this
example.

&copy; 2016-2023 by Steve Bertrand