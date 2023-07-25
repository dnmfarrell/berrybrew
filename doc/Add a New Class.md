# Add a New Class

For this example, we'll create a new class called `Images`. It will consist of
the following items:

| Class | Namespace | File |
|---|---|---|
Images | BerryBrew.Imaging | src/imaging.cs

### Table of Contents

  - [Create new CS file](#create-new-cs-file)
  - [Add the includes](#add-the-include-where-needed)
  - [Update the build_staging.pl script](#update-the-build_stagingpl-script)
  - [Update the other build scripts](#update-the-other-build-scripts)
  - [Update release build script](#update-the-release-build-script)
  - [Update 'Compile Your Own' doc](#update-the-compile-your-own-doc)
  - [Update Berrybrew API doc](#update-the-berrybrew-api-doc)
  - [Test](#test)


### Create new CS file

The new file name should be the name of the namespace it will contain. For
`BerryBrew.Imaging`, the file name should be `imaging.cs`.

Create the new **src/imaging.cs** file with the overarching namespace. See
**src/messaging.cs** as an example of the layout.

### Add the include where needed

Add a **using BerryBrew.Imaging;** directive to **src/bbapi.cs** file.

In some cases, if the `berrybrew` binary or the UI use the new classes
directly, you'll need to add a `using` statement to **src/berrybrew.cs** or
**src/berrybrew-ui.cs** files respectively.

### Update the `build_staging.pl` script

Add a reference to the new source file in the relevant builds (API and if
needed binary and UI).

Example, add `src\imaging.cs^`.

### Update the other build scripts

Where necessary

- `src\build_staging_api.bat`
- `src\build_staging_bb.bat`
- `src\build_staging_ui.bat`

Pretty much the same as [Update the 'build_staging.pl'](#update-the-build_stagingpl-script)
section above.

### Update the release build script

Pretty much the same as [Update the 'build_staging.pl'](#update-the-build_stagingpl-script)
section above.

### Update the 'Compile Your Own' doc

Pretty much the same as [Update the 'build_staging.pl'](#update-the-build_stagingpl-script)
section above.

### Update the Berrybrew API doc

Add the class and relevant information into the table at the top of the file, then,
using the other existing layouts, create a new one for the new class. Each class should
have a method table that link to the individual methods listed in that table.

### Test

Run `dev\build_staging.bat` to ensure the new library gets build and put into `staging/`,
and that all other components build and link to it successfully.

Run `staging\bb available` and any other relevant commands to ensure things work
properly.

Copy and paste each section from the "Compile Your Own" document (which you've already
updated) into a CLI, and ensure that everything is built properly.

Run `bin\bb available` and any other relevant commands to ensure things work
properly.
