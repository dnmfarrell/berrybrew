use warnings;
use strict;

use Test::More;
use Win32::TieRegistry;

my $c = 'c:/repos/berrybrew/build/berrybrew';

my $o = `$c clone 5.10.1_32 1234567890123456789012345`;

like $o, qr/Successful/, "max name length ok";

$o = `$c clone 5.10.1_32 12345678901234567890123456`;

like $o, qr/25 chars or less/, "name too long ok";

$o = `$c remove 1234567890123456789012345`;

like $o, qr/Successful/, "test install removed ok";

done_testing();
