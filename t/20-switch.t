use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $path = $Registry->{$path_key};

my $o = `$c switch xx`;
like $o, qr/Unknown version of Perl/, "switch to bad ver ok";

my @installed = BB::get_installed();
my @avail = BB::get_avail();

if (! @installed){
    diag "\nInstalling $avail[-1] because none were installed\n";
    `$c install $avail[-1]`;
    push @installed, $avail[-1];    # [pryrt] needed, otherwise next block would be skipped
}
if (@installed == 1){
    diag "\nsInstalling $avail[-2] because only one was installed\n";
    `$c install $avail[-2]`;
    push @installed, $avail[-2];    # [pryrt] for consistency
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
    my $ver = $installed[-2];

    $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

done_testing();
