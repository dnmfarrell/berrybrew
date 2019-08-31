use warnings;
use strict;

use File::Path qw(rmtree);
use Test::More;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my $test_dir = 'c:/berrybrew/test';
my $build_dir = 'c:/berrybrew/build';

my $o = `$c test clean dev`;

like $o, qr/removed the build and test directories/, "clean dev ok";
isnt -e $test_dir, 1, "clean dev: test dir gone";
isnt -e $build_dir, 1, "clean dev: build dir gone";

rmtree "$ENV{BBTEST_REPO}/test" or die $!;

isnt -e "$ENV{BBTEST_REPO}/test", 1, "test build directory removed ok";

done_testing();