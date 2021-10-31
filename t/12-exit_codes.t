use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Test::More;

# if (! $ENV{BB_TEST_ERRCODES} && ! $ENV{BB_TEST_ALL}) {
#     plan skip_all => "BB_TEST_ERRCODES env var not set";
# }

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

BB::err_code('TEST');

my %err_codes = BB::error_codes();

my %err_nums = reverse %err_codes;

my @output = split /\n/, `$c error-codes`;
my %ret_codes;

for my $line (@output) {
    my ($code, $err) = $line =~ /^(.*)\s+-\s+(\w+)$/;
    $ret_codes{$code} = $err;
}

for (2, 255, -5) {
    like `$c error $_`, qr/EXTERNAL_PROCESS_ERROR/, "errcode $_ eq EXTERNAL_PROCESS_ERROR ok";
}

for my $n (keys %ret_codes) {
    my $ret = `$c error $n`;
    like $ret, qr/$n:.*$err_nums{$n}/, "errcode $err_nums{$n} eq $n ok";
}

done_testing();
