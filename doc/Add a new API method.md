# Add a new API method

This document goes over the procedure for adding a new method to the Berrybrew
API. See the [Berrybrew API](Berrybrew%20API.md) documentation for the class to
file mapping to know where to put your new method.

- [Add the method](#add-the-method)
- [Add a table of contents entry to API doc](#add-a-table-of-contents-entry-to-documentation)
- [Add method definition to API doc](#add-the-method-definition-to-documentation)
- [Add unit tests](#add-unit-tests)
- 
### Add the method

In the relevant CS source file, add your method in alphabetical order with the
others. We'll use `ArchiveAvailable()` from the `Berrybrew` class located in
the `src\berrybrew.cs` source file as our example.

    public bool ArchiveAvailable(StrawberryPerl perl) {
        List<string> archiveList = ArchiveList();

        if (archiveList.Contains(perl.File)) {
            return true;
        }
        
        return false;
    }

### Add a table of contents entry to documentation

In the relevant class' method table of contents, add an entry for your method:

It must contain the name (with a link to the definition we'll create next), the
access permissions (always and only public gets bolded), and a short, meaningful
description of what the method is for.

    [ArchiveAvailable](#archiveavailable)| **public** | Checks whether the archive/zip file of a given Perl instance is available

### Add the method definition to documentation

The entry must be under the relevant class method section. It must list all
arguments (`argument`), the argument type (`value`), whether the argument has a
default value (`default`, not depicted below), and the return type (`returns`)
if not `void` and finally a meaningful explanation and description of what the
method is for.

    #### ArchiveAvailable

      public bool ArchiveAvailable(StrawberryPerl perl)

          argument:   perl
          value:      StrawberryPerl class object

          returns:    bool

    Checks whether the archive/zip file for the given Perl instance is still
    available on the system.

    If it is, we return `true`, otherwise the return will be `false`.

### Add unit tests

If this method will be front-facing (ie. used directly by a `berrybrew`
command), you must add unit tests for it.

&copy; 2016-2023 by Steve Bertrand
