use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $p = 'c:/repos/berrybrew/perl/perl/bin';
my $c = 'c:/repos/berrybrew/build/berrybrew';
my $customfile = 'c:/repos/berrybrew/build/data/perls_custom.json';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $o;
my $path;

my @avail = BB::get_avail();
my @installed = BB::get_installed();

if (! @installed){
    `$c install $avail[-1]`;    
}

$o = `$c clone 5.10.1_32 custom`;
ok -s $customfile > 5, "custom perls file size ok after add";

$o = `$c available`;

open my $fh, '<', 't\data\custom_available.txt' or die $!;

my @o_lines = split /\n/, $o;

my $count = 0;
for my $base (<$fh>){
    chomp $base;
    is $o_lines[$count], $base, "line $count ok after custom add";
    $count++;
}
    
@installed = BB::get_installed();

{
    my $ver = 'custom';

    $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to custom install ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

{
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\/, "PATH set ok for 'off'";
}

$o = `$c remove custom`;
like $o, qr/Successfully/, "remove custom install ok";

@avail = BB::get_avail();
ok ! grep {'custom' eq $_} @avail;

@installed = BB::get_installed();
ok ! grep {'custom' eq $_} @installed;

is -s $customfile, 2, "custom perls file size ok after remove";

done_testing();
