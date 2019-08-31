use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';

my $o = `$c off`;
like $o, qr/berrybrew perl disabled/, "off ok";

my $path = $Registry->{$path_key};
unlike $path, qr/^C:\\berrybrew\\test/, "PATH set ok for 'off'";

my @installed = BB::get_installed();
my @avail = BB::get_avail();

while(@installed < 2) {
    note "\nInstalling $avail[-1] because only " .scalar(@installed). " test perl".(@installed==1?' was':'s were')." installed\n";
    `$c install $avail[-1]`;

    @installed = BB::get_installed();
    @avail = BB::get_avail();
}

{
    my $ver = $installed[-1];

    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

{
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\test/, "PATH set ok for 'off'";
}

{
    my $ver = $installed[-2];

    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

{
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\test/, "PATH set ok for 'off'";
}

done_testing();
