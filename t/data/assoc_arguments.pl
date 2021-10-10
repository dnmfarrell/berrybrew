use warnings;
use strict;

my $ok = 0;

for (1..3) {
    my $index = $_ - 1;
    $ok++ if $ARGV[$index] == $_;
}

print $ok;