# Add a new display text section

- [Normal text entry](#normal-text-entry)
- [Subcommand entry](#subcommand-entry)
 
Most of the output displayed by `berrybrew` is handled by the `Message` class.
It gets its data from the `dev\data\messages.json` file.

Each text entry contains a label that is used by the system for the particular
message selection. It also contains a `contents` section which contains a list
of quoted strings that make up the text content.

### Normal text entry

Here are two examples of standard display text entries. They would be used.

Example usages:

- `bb.Message.Print('virtual_command_required);`
- `bb.Message.Print('register_ver_required);`
 
    {
        "label": "virtual_command_required",

        "content": [
          "\nvirtual command requires a name for the virtual instance."
        ]
    },
    {
        "label": "register_ver_required",

        "content": [
          "\nregister command requires a directory as an argument."
        ]
    },

### Subcommand entry

Subcommand entries go at the bottom of the `messages.json` file, in alphabetical
order.

Commands with subcommands allow you to use help on them to get the list of
subcommands available. They have special labels.

As an example, here is the relevant section in `messages.json` file for when a
user executes `berrybrew clean help`.

Example usage: `bb.Message.Print('subcmd.clean);`

    {
        "label": "subcmd.clean",

        "content": [
          "",
          "   berrybrew 'clean' subcommands:",
          "",
          "   all     Clean everything",
          "   dev     Deletes all files/directories related to dev perl instances",
          "   module  Deletes the exported module list directory",
          "   orphan  Deletes all orphaned Perl installations",
          "   staging Deletes the staging build directory",
          "   temp    Removes all downloaded Perl installation files",
          "   testing Removes the testing build directory"
        ]
    },

&copy; 2016-2023 by Steve Bertrand