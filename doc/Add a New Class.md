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

### Update the 'Create a Release' doc

Add a 'compiles' entry under the "Execute the `perl dev\releases.pl`" section. Use
the **$msg_build** as a guide. The output filename must be **bbNAMESPACE.dll**. In
this case, **bin/bbimaging.dll**.

### Update the 'Compile Your Own' doc

Using the "Compile the Messaging library" as a roadmap, add a new compile section
for the new library, and add a reference (ie. `-r:bbimaging.dll`) entry in each of
the libraries/binaries that will need to use it (usually only API).

### Update the `build_staging.pl` script

Pretty much the same as [Update the 'Compile Your Own'](#update-the-compile-your-own-doc)
section above.

### Update the Berrybrew API doc

Add the class and relevant information into the table at the top of the file, then,
using the other existing layouts, create a new one for the new class. Each class should
have a method table that link to the individual methods listed in that table.

### Update the release build script

Add a **"-r:bbimaging.dll" .** line to each build section that will require the
new class. Many will only be needed in the **$bb_api** build, but some classes
may be required directly in the `berrybrew` binary or UI as well.

### Add the include where needed

Add a **using BerryBrew.Imaging;** directive to **src/berrybrew.cs**, and any other
CS file that may need it.

### Test

Run `dev/build_staging.bat` to ensure the new library gets build and put into `staging/`,
and that all other components build and link to it successfully.

Run `staging\bb available` and any other relevant commands to ensure things work
properly.

Copy and paste each section from the "Compile Your Own" document (which you've already
updated) into a CLI, and ensure that everything is built properly.

Run `bin\bb available` and any other relevant commands to ensure things work
properly.
