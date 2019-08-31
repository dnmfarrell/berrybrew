package BB;

use strict;
use warnings;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

sub get_avail {
    # returns a list of available strawberry perls that are _not_ already installed
    my $list = `$c available`;
    my @avail = split /\n/, $list;

    @avail = grep {/^\s+/} @avail;

    @avail = grep {$_ !~ /installed/} @avail;

    return @avail;
}
sub get_installed {
    # returns a list of installed strawberry perls
    my $list = `$c available`;
    my @avail = split /\n/, $list;

    @avail = grep {/^\s+.*/} @avail;

    my @installed;

    for (@avail){
        s/^\s+//;
        if (/(.*?)\s+.*\[installed\]/){
            push @installed, $1;
        }
    }

    return @installed;
}

1;
