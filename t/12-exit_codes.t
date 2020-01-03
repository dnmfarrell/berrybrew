use warnings;
use strict;

use Test::More;

if (! $ENV{BB_TEST_ERRCODES} && ! $ENV{BB_TEST_ALL}) {
    plan skip_all => "BB_TEST_ERRCODES env var not set";
}

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my %err_codes = (
    GENERIC_ERROR					=> -1,
    ADMIN_BERRYBREW_INIT			=> 5,
    ADMIN_FILE_ASSOC	 			=> 10,
    ADMIN_PATH_ERROR	 			=> 15,
    ADMIN_REGISTRY_WRITE			=> 20,
    ARCHIVE_PATH_NAME_NOT_FOUND		=> 25,
    BERRYBREW_UPGRADE_FAILED		=> 30,
    DIRECTORY_CREATE_FAILED			=> 40,
    DIRECTORY_LIST_FAILED			=> 45,
    DIRECTORY_NOT_EXIST 			=> 50,
    FILE_DELETE_FAILED				=> 55,
    FILE_DOWNLOAD_FAILED 			=> 60,
    FILE_NOT_FOUND_ERROR			=> 65,
    FILE_OPEN_FAILED 				=> 70,
    INFO_OPTION_INVALID_ERROR		=> 75,
    INFO_OPTION_NOT_FOUND_ERROR		=> 80,
    JSON_FILE_MALFORMED_ERROR		=> 85,
    JSON_INVALID_ERROR 				=> 90,
    JSON_WRITE_FAILED				=> 95,
    PERL_ARCHIVE_CHECKSUM_FAILED 	=> 100,
    PERL_CLONE_FAILED				=> 105,
    PERL_CLONE_FAILED_IO_ERROR 		=> 110,
    PERL_FILE_ASSOC_FAILED 			=> 115,
    PERL_INVALID_ERROR				=> 120,
    PERL_MIN_VER_GREATER_510 		=> 125,
    PERL_NAME_INVALID				=> 130,
    PERL_NONE_IN_USE 				=> 135,
    PERL_NONE_INSTALLED 			=> 140,
    PERL_NOT_INSTALLED				=> 145,
    PERL_REMOVE_FAILED				=> 150,
    PERL_UNKNOWN_VERSION			=> 155,
    PERL_VERSION_ALREADY_REGISTERED	=> 160,
    MODULE_IMPORT_FILE_UNAVAIL		=> 165,
    MODULE_IMPORT_SAME_VERSION_ERROR=> 170,
    MODULE_IMPORT_VERSION_REQUIRED	=> 175,
    OPTION_INVALID_ERROR			=> 180,
);

for (2, 255, -5) {
    like `$c error $_`, qr/UNDEFINED_ERROR_CODE/, "errcode $_ eq UNDEFINED_ERROR_CODE ok";
}

for (keys %err_codes) {
    like `$c error $err_codes{$_}`, qr/$err_codes{$_}:.*$_/, "errcode $err_codes{$_} eq $_ ok";
}

done_testing();
