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
}

my $o = `$c clean orphan`;
like $o, qr/5\.99\.0/, "first orphan removal logged";
like $o, qr/5\.005_32/, "second orphan removal logged";

is -d "$dir/5.99.0", undef, "first orphan deleted";
is -d "$dir/5.005_32", undef, "second orphan deleted";

done_testing();
