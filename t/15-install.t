use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my @avail = BB::get_avail();

my $pre_installed = BB::get_installed();

note "\nInstalling $avail[-1]\n";
`$c install $avail[-1]`;

my $err = BB::trap("$c install $avail[-1]");
is $? >> 8, BB::err_code('PERL_ALREADY_INSTALLED'), "if perl is installed, exit status ok";
like $err, qr/already installed/, "...and error message is ok";

note "\nInstalling 5.16.3\n";
`$c install 5.16.3`;
like `$c list`, qr/5\.16\.3_64/, "install with no suffix installs 64-bit perl ok";

my $post_installed = BB::get_installed();

ok $post_installed > $pre_installed, "install ok";

done_testing();
