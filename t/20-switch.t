use warnings;
use strict;

use Test::More;
use Win32::TieRegistry;

my $p = 'c:/repos/berrybrew/perl/perl/bin';
my $c = 'c:/repos/berrybrew/bin/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $path = $Registry->{$path_key};
#print $path;

my $o = `$c switch xx`;
like $o, qr/Unknown version of Perl/, "switch to bad ver ok";

my @installed = get_installed();
my @avail = get_avail();

if (! @installed){
    `$c install $avail[-1]`;    
}
if (@installed == 1){
    `$c install $avail[-2]`;
}

@installed = get_installed();
{
    my $ver = $installed[-1];

    $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\$ver/, "PATH set ok for $ver";
}

{
    my $ver = $installed[-2];

    $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\$ver/, "PATH set ok for $ver";
}

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
