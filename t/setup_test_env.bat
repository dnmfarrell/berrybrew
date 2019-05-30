@echo off

rem Let's set up some core test environment variables
SET BB_TESTING=1

rem Switch() testing
SET BB_SWITCH_TEST=1 

IF DEFINED BBTEST_PERLROOT GOTO KnownStrawberryRoot

:GrabPerlFromPath
echo FindSystemStrawberry: Check PATH for perl.exe
( perl -v > NUL 2>&1 ) || GOTO GuessPerlLocation
echo FindSystemStrawberry: Found perl.exe in PATH: Use that perl to populate BBTEST_PERLROOT
FOR /F "usebackq delims=" %%a IN ( `perl -e "($p=$^X) =~ s{perl\\bin\\perl(?:.exe)?}{}; print $p"` ) DO (
    rem echo result="%%a"
    SET BBTEST_PERLROOT=%%a
)
GOTO KnownStrawberryRoot

:GuessPerlLocation
echo FindSystemStrawberry: Guess at standard Perl locations
IF NOT DEFINED BBTEST_PERLROOT (
    ( c:\strawberry\perl\bin\perl -v > NUL 2>&1 ) && SET BBTEST_PERLROOT=c:\strawberry\
)
IF NOT DEFINED BBTEST_PERLROOT (
    ( c:\perl\bin\perl -v > NUL 2>&1 ) && SET BBTEST_PERLROOT=c:\
)
IF NOT DEFINED BBTEST_PERLROOT (
    echo Guessing didn't work.
    echo I need you to tell me where your system strawberry perl resides:
    echo Please include perl in your PATH, or set BBTEST_PERLROOT
    exit /b 1
)
echo FindSystemStrawberry: Guessed "%BBTEST_PERLROOT%" Correctly
GOTO KnownStrawberryRoot

:KnownStrawberryRoot
echo FindSystemStrawberry: ENV{BBTEST_PERLROOT} = "%BBTEST_PERLROOT%"

rem =====================================
IF NOT DEFINED BBTEST_REPO SET BBTEST_REPO=%CD%\
echo FindRepoDirectory:    ENV{BBTEST_REPO}     = "%BBTEST_REPO%"
rem TODO = update t\test.bat to use %BBTEST_PERLROOT%\perl\bin\perl
rem TODO = update t\*.(pl|pm|t) to use ENV{BBTEST_PERLROOT}, and only add existing directories to the PATH