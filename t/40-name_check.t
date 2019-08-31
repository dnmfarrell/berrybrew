use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my @installed = BB::get_installed();
my @avail = BB::get_avail();
while(@installed < 1) {
    note "\nInstalling $avail[-1] because only " .scalar(@installed). " test perl".(@installed==1?' was':'s were')." installed\n";
    `$c install $avail[-1]`;

    @installed = BB::get_installed();
    @avail = BB::get_avail();
}

note "\nCloning $installed[-1]...\n";
my $o = `$c clone $installed[-1] 1234567890123456789012345`;

like $o, qr/Successful/, "max name length ok";

$o = `$c clone $installed[-1] 12345678901234567890123456`;

like $o, qr/25 chars or less/, "name too long ok";

note "\nRemoving 1234567890123456789012345...\n";
$o = `$c remove 1234567890123456789012345`;

like $o, qr/Successful/, "test install removed ok";

done_testing();
