# Berrybrew Unit Testing

Unit tests are written in Perl.

See [Undocumented Features](../README.md) for the `test` argument to
`berrybrew`. It sets up some additional routines while unit testing.

## Prerequisites

- You must be on a Windows system

- You must have an alternate Perl installation available.
   - Default is the first perl in your path
   - If you don't have perl in your path, or you would like to use a different perl for testing, set `BBTEST_PERLROOT`: it will use `%BBTEST_PERLROOT%\perl\bin\perl.exe` for running perl, and will include `%BBTEST_PERLROOT%\perl\bin\`,  `%BBTEST_PERLROOT%\perl\site\bin\`, and  `%BBTEST_PERLROOT%\c\bin\` in the PATH.
      - if you have `c:\strawberry\perl\bin\perl.exe`, then `SET BBTEST_PERLROOT=c:\strawberry\`
      - if you have `c:\perl\bin\perl.exe`, then `SET BBTEST_PERLROOT=c:\`
   - If perl is not in your path, and you have not set `BBTEST_PERLROOT`, it will attempt to guess at reasonable locations, including the two examples above

- You must be located (`cd`) in the root of the `berrybrew` repository

- If available perls have changed, you _may_ have to update `t\data\available.txt` and `t\data\custom_available.txt`

## Running the tests

IMPORTANT: It is highly recommended to close all command line windows and open a
new one before starting the testing, as changes to the `PATH` environment
variables during development may break the testing routines.

Clean up the Perls available lists

- run `berrybrew fetch`
- run `berrybrew available > t/data/available.txt`
- review `t/data/custom_available.txt`. All of the Perls listed above the
`[installed]` ones need to be replaced with the updated versions from
`berrybrew available`. Simply remove them all, and paste in the new list,
leaving the existing `[installed]` ones in place

Execute one of the following batch calls to run all tests

- Run them all _en masse_, with the standard perl-module test results (with a summary of the number of files and number of tests passing and failing)

        t\test.bat

- Run them all one at a time, stopping after the first failing test file (these two rows are equivalent).  This is useful when you have many failing tests, and the errors scroll beyond the scroll-history of your `cmd.exe` window.

        t\test.bat --stopfirstfail
        t\test.bat --sff                &rem same as --stopfirstfail: harder to remember, but easier to type

### What's happening

- `t\test.bat` builds berrybrew and calls `t\run_tests.pl` using your system Strawberry Perl's perl

    - It ensures that the `c:\berrybrew\test` directory hierarchy exists.  This directory is used for holding the test installs of at least two different strawberry perls, plus the clones and templates.  It is safe to delete the whole `c:\berrybrew\test` hierarchy after testing is complete. We remove this directory and recreate it at the beginning of each test run
    - Removes the `build\data\perls_custom.json` file if it exists at the commencement of the run
    - It changes the `build\data\config.json` file to reference `c:\berrybrew\test` instead of `c:\berrybrew`
    - It calls `t\setup_test_env.bat` to set the BBTEST_PERLROOT and BBTEST_REPO environment variables.  As described above, BBTEST_PERLROOT is used for generating a valid path that includes your already-installed system perl.  BBTEST_REPO defaults to the current directory when runing the test suite (which, per above, should be the root of the `berrybrew` repository)
    - If there are any command-line options given to `t\test.bat`, it will pass them on to `t\run_tests.pl`

- `t\run_tests.pl` prepends `%PATH%` with the Strawberry Perl's paths, and then it executes `prove t\*.t`
    - the `--stopfirstfail` (aka `--sff`) will cause it to `prove $_` individually for each test in `t\*.t`, and stop after the first test file that has a failing test.

### Running Individual Tests

After running `t\test.bat` (to ensure `berrybrew is built`, and the test environment is initialized), you can go back and re-run individual tests (for example, for digging into specific failing tests without spending the time of the whole test suite).

- You may run without test harness: it's more verbose while running (`Test::More::note()` messages in the .t file _will_ print), but doesn't give the end-of-file summary:

        c:> perl t\05-available.t
        ok 1 - >< :: >< ok
        ok 2 - >ThefollowingStrawberryPerlsareavailable:< :: >ThefollowingStrawberryPerlsareavailable:< ok
        ok 3 - >< :: >< ok
        ok 4 - >5.24.1_64< :: >5.24.1_64< ok
        ok 5 - >5.24.1_64_PDL< :: >5.24.1_64_PDL< ok
        ok 6 - >5.24.1_32< :: >5.24.1_32< ok
        ok 7 - >5.22.3_64< :: >5.22.3_64< ok
        ok 8 - >5.22.3_64_PDL< :: >5.22.3_64_PDL< ok
        ok 9 - >5.22.3_32< :: >5.22.3_32< ok
        ok 10 - >5.20.3_64< :: >5.20.3_64< ok
        ok 11 - >5.20.3_64_PDL< :: >5.20.3_64_PDL< ok
        ok 12 - >5.20.3_32< :: >5.20.3_32< ok
        ok 13 - >5.18.4_64< :: >5.18.4_64< ok
        ok 14 - >5.18.4_32< :: >5.18.4_32< ok
        ok 15 - >5.16.3_64< :: >5.16.3_64< ok
        ok 16 - >5.16.3_32< :: >5.16.3_32< ok
        ok 17 - >5.14.4_64< :: >5.14.4_64< ok
        ok 18 - >5.14.4_32< :: >5.14.4_32< ok
        ok 19 - >5.12.3_32< :: >5.12.3_32< ok
        ok 20 - >5.10.1_32< :: >5.10.1_32< ok
        ok 21 - >5.8.9_32< :: >5.8.9_32< ok
        ok 22 - >< :: >< ok
        1..22

- You may run with the test harness: it's less verbose while running (`note` messages in the .t file will _not_ print), but it does give the end-of-file summary:

        C:> prove t\05-available.t
        t\05-available.t .. ok
        All tests successful.
        Files=1, Tests=22,  0 wallclock secs ( 0.05 usr +  0.02 sys =  0.06 CPU)
        Result: PASS

- You may also re-run all the tests without recompiling berrybrew

        c:> perl t\run_tests.pl [--stopfirstfail|--sff]
        
## Environment Variables

As the software becomes more complex and dynamic, sometimes we have to
inform the code that we're in testing mode as to shim up (or unshim) the
system so testing can proceed.

As needed, certain environment variables for testing will be added to
the top of the `t\setup_test_env.bat` script, before the `BEGIN  TEST
ENV SETUP` line.

Nasty, weird things can happen if they are disabled without 
understanding what they do and why they're there.

#### BB_SWITCH_TEST

This flag is to inform the `Switch()` method that we're in testing, so
it won't run the `SwitchProcess()` call. That call removes the parent
process, so everything breaks if this flag isn't set.        
