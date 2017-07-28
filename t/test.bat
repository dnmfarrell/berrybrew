call dev\build.bat

IF NOT DEFINED BBTEST_PERLROOT CALL t\setup_test_env.bat
IF NOT DEFINED BBTEST_REPO     CALL t\setup_test_env.bat

call perl -i.bak -ne "s/berrybrew(?!\\\\test)/berrybrew\\\\test/; print" build/data/config.json

IF EXIST c:\berrybrew\test RMDIR /s /q c:\berrybrew\test
IF EXIST %BBTEST_REPO%\build\data\perls_custom.json DEL %BBTEST_REPO%\build\data\perls_custom.json

mkdir c:\berrybrew\test
mkdir c:\berrybrew\test\temp

%BBTEST_PERLROOT%\perl\bin\perl t/run_tests.pl %*
