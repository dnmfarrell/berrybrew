mkdir c:\berrybrew\test
mkdir c:\berrybrew\test\temp
call dev\build.bat

call perl -i.bak -ne "s/berrybrew/berrybrew\\\\test/; print" build/data/config.json
c:\strawberry\perl\bin\perl t/run_tests.pl
