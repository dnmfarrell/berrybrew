use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use constant DEBUG => 0;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';

my @avail = BB::get_avail();

my $pre_installed = BB::get_installed();

note "\nInstalling $avail[-1]\n";
`$c install $avail[-1]`;

my $post_installed = BB::get_installed();

ok $post_installed > $pre_installed, "install ok";

done_testing();
