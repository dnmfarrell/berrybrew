use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $p = 'c:/repos/berrybrew/perl/perl/bin';
my $c = 'c:/repos/berrybrew/build/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $o = `$c off`;
like $o, qr/berrybrew perl disabled/, "off ok";

my $path = $Registry->{$path_key};
unlike $path, qr/^C:\\berrybrew\\/, "PATH set ok for 'off'";

my @installed = BB::get_installed();
my @avail = BB::get_avail();

if (! @installed){
    `$c install $avail[-1]`;    
}
if (@installed == 1){
    `$c install $avail[-2]`;
}

@installed = BB::get_installed();
{
    my $ver = $installed[-1];

    $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

{
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\/, "PATH set ok for 'off'";
}

{
    my $ver = $installed[-2];

    $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

{
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\/, "PATH set ok for 'off'";
}

done_testing();
