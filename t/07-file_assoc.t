use warnings;
use strict;

use Test::More;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

like `assoc .pl=Perl_program_file`, qr/\.pl=Perl_program_file/, "assoc set to Perl_program_file";
like 
    `ftype Perl_program_file=C:\\Strawberry\\perl\\bin\\perl.exe %1 %*`,
    qr/Perl_program_file=C:\\Strawberry.* %1 %\*/,
    "ftype set to Strawberry ok";

like `assoc .pl`, qr/pl=Perl_program_file/, "file assoc Perl_program_file registered";
like `$c options file_assoc`, qr/file_assoc:\s+Perl_program_file/, "file_assoc option ok initially";

`$c install 5.8.9_32`;
`$c switch 5.8.9_32`;

# file_assoc_old checks
# switch between two perls and check ftype changes for berrybrewPerl

like `$c associate set`, qr/berrybrew is now managing/, "associate set ok";
like `$c options file_assoc`, qr/file_assoc:\s+berrybrewPerl/, "file_assoc option ok after set";
like `assoc .pl`, qr/pl=berrybrewPerl/, "file assoc berrybrewPerl registered";
like `$c associate unset`, qr/Set Perl file assoc/, "associste unset ok";
like `$c options file_assoc`, qr/file_assoc:\s+Perl_program_file/, "file_assoc option ok after unset";
like `assoc .pl`, qr/pl=Perl_program_file/, "file assoc Perl_program_file set back to default";

`$c remove 5.8.9_32`;

done_testing();
