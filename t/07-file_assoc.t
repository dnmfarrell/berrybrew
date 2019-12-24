use warnings;
use strict;

use Test::More;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

like `$c options file_assoc`, qr/file_assoc:\s+PerlScript/, "file_assoc option ok initially";
like `$c associate set`, qr/Handler:\s+berrybrewPerl/, "file_assoc set ok";
like `$c options file_assoc`, qr/file_assoc:\s+berrybrewPerl/, "file_assoc option ok after set";
like `$c associate unset`, qr/Handler:\s+PerlScript/, "file_assoc unset ok";
like `$c options file_assoc`, qr/file_assoc:\s+PerlScript/, "file_assoc option ok after unset";

done_testing();
