# Managing Installer File Integrity

We take great care in managing the files that get installed with the installer.

This ensures that everything we install is eventually removed upon uninstall,
which allows us to remove everything cleanly.

Failure to follow all steps will result in an error describing what you've
missed during the installer build process.

- [Add files to be installed to installer script](#add-the-files-to-be-installed-to-installer-script)
- [Add files to be installed to manifest](#add-the-files-to-be-installed-to-manifest)
- [Add files to be deleted to installer script](#add-the-files-to-be-deleted-to-installer-script)
- [Add files to be ignored to manifest skip](#add-the-files-to-be-ignored-to-manifest-skip)

File integrity checking procedure and solution table. 
 
- [File integrity checking procedure](#file-integrity-checking-procedure)
- [File integrity checking solution table](#file-integrity-checking-solution-table) 
 
### Add the files to be installed to installer script

The installer script locations:

Staging:    `dev\create_staging_installer.nsi`

Production: `dev\create_prod_installer.nsi`

The file you want to add goes into a `File` directive underneath of the
appropriate `SetOutPath` section within the installer script. Use the existing
entries as a guide.

Essentially, the `File` directive copies the file from the location of its
value and places that file into the `SetOutPath` location.

### Add the files to be installed to manifest

The staging manifest is `MANIFEST.STAGING`, production is `MANIFEST`.

The manifest stores the names of all files that will be installed by the
installer. The manifest ensures we aren't forgetting anything. Each entry
correlates to a `File` directive in the installer script.

Simply add the file with path to a logical place within the file.

### Add the files to be deleted to installer script

The installer script locations:

Staging:    `dev\create_staging_installer.nsi`

Production: `dev\create_prod_installer.nsi`

You need to also tell the installer to delete these files when we uninstall, so
add a `Delete` directive for the file within the `Section Uninstall`.

Eg.

    Delete "$INSTDIR\data\messages.json"

### Add the files to be ignored to manifest skip

The staging manifest ignore file is `MANIFEST.STAGING.SKIP`, production is
`MANIFEST.SKIP`.

The manifest ignore file contains every single file in the repository that must
not be installed with the installer. We use this file to ensure rogue entries
aren't supposed to be present aren't in the installer script.

Simply add the file names to the logical location in the file.

### File integrity checking procedure

We perform several different checks to ensure consistency with files related to
the installer. These checks are performed during the build process and are
managed by the `lib\BuildHelper::check_installer_manifest()` function.

The process is as follows:

- Read over the `MANIFEST.SKIP` file and create a list of files and directories
to ignore
- Collect all file and directory entries in the repository, excluding the ones
collected from `MANIFEST.SKIP`
- Read in the entries in the `MANIFEST` file and put them into a list
- Compares each entry in the filtered repository file list to the `MANIFEST`
- Compares each entry in the `MANIFEST` to all the files in the repo
- Compares each entry in the `MANIFEST` to each entry in the installer script
- Compares the installer script install files (`File`) to the installer script
uninstall files (`Delete`)

### File integrity checking solution table

|Warning|Solution|
|---|---|
| **file is in REPO but isn't in the MANIFEST** | Add the file to either `MANIFEST` or `MANIFEST.SKIP` |
| **file is in the MANIFEST but isn't in the REPO** | Remove the entry from `MANIFEST`, or restore the file on disk |
| **file is in INSTALLER but isn't in the MANIFEST** | Remove the `File` entry in the installer, or add it to the`MANIFEST` |
| **file is in MANIFEST but isn't in the INSTALLER** | Add a `File` entry to the installer, or remove it from the `MANIFEST` |
| **file is in INSTALLER but isn't in UNINSTALL** | Add a `Delete` entry, or remove the `File` entry from the installer |

&copy; 2016-2023 by Steve Bertrand