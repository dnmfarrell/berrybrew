call dev\build_testing.bat

IF NOT DEFINED BBTEST_PERLROOT CALL t\setup_test_env.bat
IF NOT DEFINED BBTEST_REPO     CALL t\setup_test_env.bat
IF NOT DEFINED BB_TESTING      CALL t\setup_test_env.bat

IF EXIST c:\berrybrew\testing RMDIR /s /q c:\berrybrew\testing
IF EXIST %BBTEST_REPO%\testing\data\perls_custom.json DEL %BBTEST_REPO%\testing\data\perls_custom.json

mkdir c:\berrybrew\testing
mkdir c:\berrybrew\testing\temp

%BBTEST_PERLROOT%\perl\bin\perl t/run_tests.pl %*
