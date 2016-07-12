use warnings;
use strict;

use Test::More;

my $c = 'c:/repos/berrybrew/bin/berrybrew';

my @avail = get_avail();
my $pre_installed = get_installed();

`$c install $avail[-1]`;

my $post_installed = get_installed();

ok $post_installed > $pre_installed, "install ok";

sub get_avail {
    my $list = `$c available`;
    my @avail = split /\n/, $list;

    @avail = grep {s/\s+//g; $_ =~ /^5/} @avail;

    @avail = grep {$_ !~ /installed/} @avail;

    return @avail;
}
sub get_installed {
    my $list = `$c available`;
    my @avail = split /\n/, $list;

    @avail = grep {s/\s+//g; $_ =~ /^5/} @avail;

    my @installed;

    for (@avail){
        if (/(.*)\[installed\]/){
            push @installed, $1;
        }
    }    
    return @installed;
}

done_testing();
