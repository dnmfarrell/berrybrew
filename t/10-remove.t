use warnings;
use strict;

use Test::More;
use Win32::TieRegistry;

my $c = 'c:/repos/berrybrew/build/berrybrew';

my @installed = get_installed();

if (! @installed){
    plan skip_all => "no perls installed... nothing to do";
}

for (@installed){
    my $o = `$c remove $_`;
    like $o, qr/Successfully/, "$_ removed ok";
}

@installed = get_installed();

is @installed, 0, "all perls removed";

sub get_installed {
    my $list = `$c available`;
    my @avail = split /\n/, $list;

    @avail = grep {$_ =~ /^\s+.*/} @avail;

    my @installed;

    for (@avail){
        s/^\s+//;
        if (/(.*)\s+\.*?\[installed\]\*?/){
            push @installed, $1;
        }
    }    
    return @installed;
}

done_testing();
