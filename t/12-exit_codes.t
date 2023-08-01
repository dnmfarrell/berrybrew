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

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

BB::err_code('TEST');

my %err_codes = BB::error_codes();

my %err_nums = reverse %err_codes;

my @output = split /\n/, `$c error-codes`;
my %ret_codes;

for my $line (@output) {
    my ($code, $err) = $line =~ /^(.*)\s+-\s+(\w+)$/;
    $ret_codes{$code} = $err;
}

# check that all error codes in BB::error_codes() are listed in the source

for my $test_errcode (keys %err_nums) {
    is exists($ret_codes{$test_errcode}), 1, "errcode $test_errcode in test matches actual enum"
}

# check that all error codes listed in source are listed in BB::error_codes()

for my $src_errcode (values %ret_codes) {
    is exists($err_codes{$src_errcode}), 1, "errcode $src_errcode in src enum matches BB::error_codes() ok";
}

for (2, 255, -5) {
    like `$c error $_`, qr/EXTERNAL_PROCESS_ERROR/, "errcode $_ eq EXTERNAL_PROCESS_ERROR ok";
}

for my $n (keys %ret_codes) {
    my $ret = `$c error $n`;
    like $ret, qr/$n:.*$err_nums{$n}/, "errcode $err_nums{$n} eq $n ok";
}

done_testing();
