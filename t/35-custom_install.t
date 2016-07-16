use warnings;
use strict;

use Test::More;
use Win32::TieRegistry;

my $p = 'c:/repos/berrybrew/perl/perl/bin';
my $c = 'c:/repos/berrybrew/build/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $o;
my $path;


my @avail = get_avail();
my @installed = get_installed();

if (! @installed){
    `$c install $avail[-1]`;    
}

$o = `$c clone 5.10.1_32 custom`;

@installed = get_installed();

{
    my $ver = 'custom';

    $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to custom install ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\$ver/, "PATH set ok for $ver";
}

{
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\/, "PATH set ok for 'off'";
}

$o = `$c remove custom`;
print $o;

@avail = get_avail();
ok ! grep {print "*$_\n"; 'custom' eq $_} @avail;

@installed = get_installed();
ok ! grep {print "i$_\n"; 'custom' eq $_} @installed;

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
