use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Test::More;
use Win32::TieRegistry;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my @installed = BB::get_installed();

if (! @installed){
    plan skip_all => "no perls installed... nothing to do"
}

my $o = `$c off`;
like $o, qr/berrybrew perl disabled/, "off ok";

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

unlike $path, qr/^C:\\berrybrew-testing/, "PATH set ok for 'off'";
unlike $path, qr/^C:\\berrybrew-staging/, "PATH set ok for 'off'";

for (@installed){
    note "\nRemoving $_...\n";
    my $o = `$c remove $_`;
    like $o, qr/Successfully/, "$_ removed ok";
}

@installed = BB::get_installed();

is scalar @installed, 0, "all perls removed";


done_testing();
