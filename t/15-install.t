use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use constant DEBUG => 0;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';

my @avail = BB::get_avail();
    note @avail if DEBUG;

my $pre_installed = BB::get_installed();
    note $pre_installed if DEBUG;

    note $avail[-1] if DEBUG;
    note "$c install $avail[-1]\n" if DEBUG;
`$c install $avail[-1]`;
    note "\$!: $!\n" if DEBUG;

my $post_installed = BB::get_installed();

ok $post_installed > $pre_installed, "install ok";

done_testing();
