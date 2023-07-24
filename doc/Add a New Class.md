# Add a New Class

For this example, we'll create a new class called `Images`. It will consist of
the following items:

| Class | Namespace | File | Library |
|---|---|---|---|
Images | BerryBrew.Imaging | src/imaging.cs | bin/bbimaging.dll |

### Table of Contents

  - [Create new CS file](#create-new-cs-file)
  - [Update 'Create a Release' doc](#update-the-create-a-release-doc)
  - [Update 'Compile Your Own' doc](#update-the-compile-your-own-doc)
  - [Update the build_staging.pl script](#update-the-build_stagingpl-script)
  - [Update Berrybrew API doc](#update-the-berrybrew-api-doc)
  - [Update release build script](#update-the-release-build-script)
  - [Add the includes](#add-the-include-where-needed)
  - [Test](#test)


### Create new CS file

The new file name should be prepended with **bb** followed by the name of
the namespace.

Create the new **src/imaging.cs** file with the overarching namespace. See
**src/messaging.cs** as an example of the layout.

### Update the 'Compile Your Own' doc

Add a reference to the new source file in the "Compile the API library"
section, eg. `src\imaging.cs^`.

### Update the `build_staging.pl` script

Pretty much the same as [Update the 'Compile Your Own'](#update-the-compile-your-own-doc)
section above.

### Update the Berrybrew API doc

Add the class and relevant information into the table at the top of the file, then,
using the other existing layouts, create a new one for the new class. Each class should
have a method table that link to the individual methods listed in that table.

### Update the release build script

Pretty much the same as [Update the 'build_staging.pl'](#update-the-build_stagingpl-script)
section above.

### Add the include where needed

Add a **using BerryBrew.Imaging;** directive to **src/bbapi.cs** file.

In some cases, if the `berrybrew` binary or the UI use the new classes
directly, you'll need to add a `using` statement to **src/berrybrew.cs** or
**src/berrybrew-ui.cs** files respectively.

### Test

Run `dev/build_staging.bat` to ensure the new library gets build and put into `staging/`,
and that all other components build and link to it successfully.

Run `staging\bb available` and any other relevant commands to ensure things work
properly.

Copy and paste each section from the "Compile Your Own" document (which you've already
updated) into a CLI, and ensure that everything is built properly.

Run `bin\bb available` and any other relevant commands to ensure things work
properly.
