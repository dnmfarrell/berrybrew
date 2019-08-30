use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my @installed = BB::get_installed();

if (! @installed){
    plan skip_all => "no perls installed... nothing to do"
}

my $o = `$c off`;
like $o, qr/berrybrew perl disabled/, "off ok";

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

unlike $path, qr/^C:\\berrybrew\\test/, "PATH set ok for 'off'";
unlike $path, qr/^C:\\berrybrew\\build/, "PATH set ok for 'off'";

for (@installed){
    note "\nRemoving $_...\n";
    my $o = `$c remove $_`;
    like $o, qr/Successfully/, "$_ removed ok";
}

@installed = BB::get_installed();

is @installed, 0, "all perls removed";

done_testing();
