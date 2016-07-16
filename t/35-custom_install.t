use warnings;
use strict;

use Test::More;
use Win32::TieRegistry;

my $p = 'c:/repos/berrybrew/perl/perl/bin';
my $c = 'c:/repos/berrybrew/build/berrybrew';
my $customfile = 'c:/repos/berrybrew/build/data/perls_custom.json';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $o;
my $path;

my @avail = get_avail();
my @installed = get_installed();

if (! @installed){
    `$c install $avail[-1]`;    
}

$o = `$c clone 5.10.1_32 custom`;
ok -s $customfile > 2, "custom perls file size ok after add";

$o = `$c available`;
open my $fh, '<', 't\data\custom_available.txt' or die $!;
my $base;
{
    local $/;
    $base = <$fh>;
}
close $fh;
is $o, $base, "available shows ok after custom add";
    
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
like $o, qr/Successfully/, "remove custom install ok";

@avail = get_avail();
ok ! grep {'custom' eq $_} @avail;

@installed = get_installed();
ok ! grep {'custom' eq $_} @installed;

is -s $customfile, 2, "custom perls file size ok after remove";

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
