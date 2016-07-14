use warnings;
use strict;

use Test::More;

my $c = 'c:/repos/berrybrew/build/berrybrew';

my $dir = 'c:/berrybrew';
my @perls = qw(5.99.0 5.005_32);

for (@perls){
    mkdir "$dir/$_" if ! -d or die $!;

    my $o = `$c`;
    like $o, qr/WARNING! orphaned/, "orphaned perl $_ caught";

    rmdir "$dir/$_" or die $!;

    is -d, undef, "$_ dir deleted ok";
}

done_testing();
