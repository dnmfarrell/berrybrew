use warnings;
use strict;

use File::Path qw(rmtree);
use Test::More;
use Win32::TieRegistry;

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';



my $regkey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\berrybrew-testing\\";
my $testing_instance_dir = 'c:/berrybrew-testing/instance';
my $staging_instance_dir = 'c:/berrybrew-staging/instance';
my $testing_build_dir = `$c info install_path`;
chomp $testing_build_dir;
$testing_build_dir =~ s/\s+//;

my $o = `$c test clean dev`;

like $o, qr/removed the staging and testing instance_dir directories/, "clean dev ok";
isnt -e $testing_instance_dir, 1, "clean dev: testing instance dir gone";
isnt -e $staging_instance_dir, 1, "clean dev: staging instance dir gone";

rmtree $testing_build_dir;
isnt -d $testing_build_dir, 1, "testing directory removed ok";

# clean registry

like delete $Registry->{$regkey}, qr/HASH/, "removed registry key ok";

my $store = $Registry->{$regkey};
is $store, undef, "Deleted test registry key ok";

done_testing();