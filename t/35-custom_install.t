use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';
my $customfile = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/data/perls_custom.json" : 'c:/repos/berrybrew/build/data/perls_custom.json';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $o;
my $path;

my @avail = BB::get_avail();
my @installed = BB::get_installed();

while(@installed < 2) {
    note "\nInstalling $avail[-1] because only " .scalar(@installed). " test perl".(@installed==1?' was':'s were')." installed\n";
    `$c install $avail[-1]`;

    @installed = BB::get_installed();
    @avail = BB::get_avail();
}

note "\nCloning $installed[-1] to custom\n";
$o = `$c clone $installed[-1] custom`;
ok -s $customfile > 5, "custom perls file size ok after add";

$o = `$c available`;

open my $fh, '<', 't\data\custom_available.txt' or die $!;

my @o_lines = split /\n/, $o;

my $count = 0;
for my $base (<$fh>){
    chomp $base;
    s/(?:^\s+|\s+$)//g   for $base, $o_lines[$count];    # make leading/trailing spaces insignificant
    is $o_lines[$count], $base, "line $count ok after custom add";
    $count++;
}

@installed = BB::get_installed();

{
    my $ver = 'custom';

    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to custom install ok";
    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

{
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\test/, "PATH set ok for 'off'";
}

$o = `$c remove custom`;
like $o, qr/Successfully/, "remove custom install ok";

@avail = BB::get_avail();
ok ! grep {'custom' eq $_} @avail;

@installed = BB::get_installed();
ok ! grep {'custom' eq $_} @installed;

is -s $customfile, 2, "custom perls file size ok after remove";

done_testing();
