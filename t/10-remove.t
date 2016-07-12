use warnings;
use strict;

use Test::More;
use Win32::TieRegistry;

my $c = 'c:/repos/berrybrew/bin/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $path = $Registry->{$path_key};

if ($path =~ /c:\\berrybrew\\.*?\\perl/i){
    plan skip_all => "run 'berrybrew off' then start a new cmd for this test";
}

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

    @avail = grep {s/\s+//g; $_ =~ /^5/} @avail;

    my @installed;

    for (@avail){
        if (/(.*)\[installed\]\*?/){
            push @installed, $1;
        }
    }    
    return @installed;
}

done_testing();
