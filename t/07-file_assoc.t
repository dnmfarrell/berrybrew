use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
#use BB;
use Test::More;

#BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';
my $refresh = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew-refresh.bat" : 'c:/repos/berrybrew/testing/berrybrew-refresh.bat';

like `$c assoc`, qr/Perl file association handling/, "'assoc' works as an alias for 'associate' command";

like `assoc .pl=Perl_program_file`, qr/\.pl=Perl_program_file/, "assoc set to Perl_program_file";
like 
    `ftype Perl_program_file=C:\\Strawberry\\perl\\bin\\perl.exe %1 %*`,
    qr/Perl_program_file=C:\\Strawberry.* %1 %\*/,
    "ftype set to Strawberry ok";

like `assoc .pl`, qr/pl=Perl_program_file/, "file assoc Perl_program_file registered";
like `$c options file_assoc`, qr/file_assoc:\s+Perl_program_file/, "file_assoc option ok initially";

`$c install 5.8.9_32`;
`$c switch 5.8.9_32`;

# first pass needs set called twice... this is not needed at the CLI
like `$c associate set`, qr/berrybrew is now managing/, "associate set ok";
`$c associate set`;
like `t\\data\\assoc.pl`, qr/5\.8\.9_32/, "script works, 5.8.9_32";
like `assoc .pl`, qr/pl=berrybrewPerl/, "file assoc berrybrewPerl registered";
like `$c options file_assoc`, qr/file_assoc:\s+berrybrewPerl/, "file_assoc option ok after set";
like `$c options file_assoc_old`, qr/file_assoc_old:\s+Perl_program_file/, "file_assoc_old option ok after set";
like `ftype berrybrewPerl`, qr/berrybrew.*5\.8\.9_32/, "bb 5.8.9_32 ftype ok";

`$c install 5.10.1_32`;
`$c switch 5.10.1_32`;
`$refresh`;

is `$c associate set`, '', "associate set after switch ok";
like `t\\data\\assoc.pl`, qr/5\.10\.1_32/, "script works, 5.10.1_32";
like `assoc .pl`, qr/pl=berrybrewPerl/, "file assoc berrybrewPerl registered after switch ok";
like `$c options file_assoc`, qr/file_assoc:\s+berrybrewPerl/, "file_assoc option ok after switch & set";
like `$c options file_assoc_old`, qr/file_assoc_old:\s+Perl_program_file/, "file_assoc_old option ok after switch &set";
like `ftype berrybrewPerl`, qr/berrybrew.*5\.10\.1_32/, "bb 5.10.1_32 ftype ok";

# Issue 303: assoc string causes incorrect argument passing
is `t\\data\\assoc_no_arguments.pl`, 1, "assoc with no args ok";
is `t\\data\\assoc_arguments.pl 1 2 3`, 3, "assoc with args ok";
# End Issue 303

like `$c associate unset`, qr/Set Perl file assoc/, "associste unset ok";
like `$c options file_assoc`, qr/file_assoc:\s+Perl_program_file/, "file_assoc option ok after unset";
like `assoc .pl`, qr/pl=Perl_program_file/, "file assoc Perl_program_file set back to default";

`$c remove 5.8.9_32`;
`$c remove 5.10.1_32`;

done_testing();
