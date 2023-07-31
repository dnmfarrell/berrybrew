use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Test::More;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my @archives = BB::get_downloads();

if (! scalar @archives) {
    `$c download 5.10.1_32`;
    @archives = BB::get_downloads();
}

like
    `$c download 5.10.1_32`,
    qr/already exists/,
    "if an archive file exists, we don't download it";

done_testing();