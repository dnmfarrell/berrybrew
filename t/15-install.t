use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;

my $c = 'c:/repos/berrybrew/build/berrybrew';

my @avail = BB::get_avail();
my $pre_installed = BB::get_installed();

`$c install $avail[-1]`;

my $post_installed = BB::get_installed();

ok $post_installed > $pre_installed, "install ok";

done_testing();
