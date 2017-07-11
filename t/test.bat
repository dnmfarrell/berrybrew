mkdir c:\berrybrew\test
mkdir c:\berrybrew\test\temp
call dev\build.bat

IF NOT DEFINED BBTEST_PERLROOT CALL t\setup_test_env.bat
IF NOT DEFINED BBTEST_REPO     CALL t\setup_test_env.bat

call perl -i.bak -ne "s/berrybrew/berrybrew\\\\test/; print" build/data/config.json
%BBTEST_PERLROOT%\perl\bin\perl t/run_tests.pl %*
