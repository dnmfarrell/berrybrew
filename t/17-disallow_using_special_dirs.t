use warnings;
use strict;

use Data::Dumper;
use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Test::More;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my $installed_count = BB::get_installed();

if ($installed_count == 0) {
    if (`$c list` !~ /5\.10\.1_32/) {
        note "\nInstalling 5.10.1_32";
        `$c install 5.10.1_32`;
    }
}

my $special_dir_output = `$c orphans-ignored`;

my @special_dirs = $special_dir_output =~ /\t(.*)\n/g;

for (@special_dirs) {
    my $install_err = BB::trap("$c clone 5.10.1_32 $_");
    is $? >> 8, BB::err_code('PERL_DIRECTORY_SPECIAL'), "if clone to special dir, exit ok";
    like $install_err, qr/special directory/, "...and error message is ok";   
    exit;
}

`$c remove 5.10.1_32`;
is BB::get_installed, $installed_count, "removed perls ok";

done_testing();
