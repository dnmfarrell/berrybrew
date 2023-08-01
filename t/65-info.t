use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Test::More;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my ($bbpath) = $c =~ m|(.*)/berrybrew|;
$bbpath =~ s|\*||g;

my %valid_opts = (
    install_path    => $bbpath,
    config_path     => "$bbpath/data",
    bin_path        => $bbpath,
    storage_path    => "c:/berrybrew-testing",
    instance_path   => "c:/berrybrew-testing/instance",
    archive_path    => "c:/berrybrew-testing/temp",
    snapshot_path   => "c:/berrybrew-testing/snapshots",
);

like `$c info`, qr/requires an option argument/, "info with no args ok";

my $err = BB::trap("$c info invalid");
is $? >> 8, BB::err_code('INFO_OPTION_INVALID_ERROR'), "invalid info entry exit status ok";
like $err, qr/is not a valid option/, "info with bad arg ok";

for my $f (keys %valid_opts){
    my $o = `$c info $f`;
    $o =~ tr/\n//d;
    $o =~ s/^\s+//;
    $o =~ s|\\|/|g;
    $o =~ s/\/$//;

    $valid_opts{$f} =~ s|\\|/|g;
    
    is lc $o, lc $valid_opts{$f}, "'$f' has proper path returned";
}

done_testing();