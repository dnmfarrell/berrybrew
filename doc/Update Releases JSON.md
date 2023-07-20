# Updating the Repositories `releases.json` File

**NOTE**: This procedure updates the `releases.json` data from the Github
repository. This is normally done from the build system, but access is currently
an issue. Once that is resolved, this process will no longer be needed.

## Create a Github token

This exercise is left up to the reader. Once created you must export it as the
`GITHUB_TOKEN` environment variable.

## Authorize the Github CLI

    gh auth login

## Create new directory

    mkdir ~/scratch/releases

## Get the releases list

    gh release list -R StrawberryPerl/Perl-Dist-Strawberry

Output:

    TITLE                                  TYPE         TAG NAME
    Strawberry Perl 5.38.0 and 5.36.1      Latest       SP_5380_5361
    dev_5.38.0_20230705_gcc13                           dev_5380_20230705_gcc13

## Download the 'Latest' release

Use the "TAG NAME" as your download target. Pass in the directory created above
with the `-D` argument, and the repo to list the releases from with the `-R`
arg.

    gh release download SP_5380_5361 -D ~/scratch/releases -R StrawberryPerl/Perl-Dist-Strawberry

## Run the `dev/generate_github_releases_json.pl` script with the new directory
and the release tag as arguments:

    generate_github_releases.json.pl --dir ~/scratch/github SP_5380_5361 --tag SP_5380_5361

The data you need will be printed to `STDOUT` as a list of JSON objects.

## Merge the new data

Copy just the JSON objects (leave out the surrounding list brackets) to the
clipboard, and paste them as the first item in the list in the `releases.json`
file in the `StrawberryPerl/strawberryperl.com` repository.

## Execute the JSON to HTML translator

In the `StrawberryPerl/strawberryperl.com` repo, run the `