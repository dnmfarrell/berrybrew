# Berrybrew Unit Testing

Unit tests are written in Perl.

#### Prerequisites

- You must be on a Windows system

- You must have an alternate Perl installation available. Default is 
`C:\Strawberry\perl\bin\perl.exe` (change this in `t\test.bat`)

- You must be located (`cd`) in the root of the `berrybrew` repository

#### Running the tests

Execute the following batch file to run all tests

    t\test.bat

#### What's happening

- `t\test.bat` calls `t\run_tests.pl` using your system Strawberry Perl's perl

- `t\run_tests.pl` prepends `%PATH%` with the Strawberry Perl's paths, and then it
executes `prove t\*.t`
 
