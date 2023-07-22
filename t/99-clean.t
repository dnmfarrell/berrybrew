use warnings;
use strict;

use File::Path qw(rmtree);
use Test::More;
use Win32::TieRegistry;

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my $regkey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\berrybrew-testing\\";
my $test_dir = 'c:/berrybrew/testing';
my $staging_dir = 'c:/berrybrew/staging';

my $o = `$c test clean dev`;

like $o, qr/removed the staging and test directories/, "clean dev ok";
isnt -e $test_dir, 1, "clean dev: test dir gone";
isnt -e $staging_dir, 1, "clean dev: staging dir gone";

rmtree "$ENV{BBTEST_REPO}/testing" or die $!;
isnt -e "$ENV{BBTEST_REPO}/testing", 1, "testing directory removed ok";

# clean registry

like delete $Registry->{$regkey}, qr/HASH/, "removed registry key ok";

my $store = $Registry->{$regkey};
is $store, undef, "Deleted test registry key ok";

done_testing();