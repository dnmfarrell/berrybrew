# Berrybrew Unit Testing

Unit tests are written in Perl.

#### Prerequisites

- You must be on a Windows system

- You must have an alternate Perl installation available.
   - Default is the first perl in your path
   - If you don't have perl in your path, or you would like to use a different perl for testing, set `BBTEST_PERLROOT`: it will use `%BBTEST_PERLROOT%\perl\bin\perl.exe` for running perl, and will include `%BBTEST_PERLROOT%\perl\bin\`,  `%BBTEST_PERLROOT%\perl\site\bin\`, and  `%BBTEST_PERLROOT%\c\bin\` in the PATH.
      - if you have `c:\strawberry\perl\bin\perl.exe`, then `SET BBTEST_PERLROOT=c:\strawberry\`
      - if you have `c:\perl\bin\perl.exe`, then `SET BBTEST_PERLROOT=c:\`
   - if perl is not in your path, and you have not set `BBTEST_PERLROOT`, it will attempt to guess at reasonable locations, including the two examples above

- You must be located (`cd`) in the root of the `berrybrew` repository

#### Running the tests

Execute the following batch file to run all tests

    t\test.bat

Or, if you would like to run individual tests

    t\setup_test_env.bat
    perl t\##-name.t        &rem runs without test harness, so note() messages will show
    prove t\##-name.t       &rem runs with test harness, so note() messages will not show
    perl t\run_tests.pl --stopfirstfail     &rem will `prove` each test, in numeric order, until a failing test is reached
    perl t\run_tests.pl --sff               &rem easier to type than --stopfirstfail

#### What's happening

- `t\test.bat` calls `t\run_tests.pl` using your system Strawberry Perl's perl

- `t\run_tests.pl` prepends `%PATH%` with the Strawberry Perl's paths, and then it
executes `prove t\*.t`

