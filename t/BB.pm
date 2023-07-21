package BB;

use strict;
use warnings;

use Capture::Tiny qw(:all);
use Data::Dumper;
use IPC::Run3;
use Test::More;

$ENV{BERRYBREW_ENV} = "test";

my $c = exists $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';
my $test_repo = exists $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/$ENV{BERRYBREW_ENV}" : 'c:/repos/berrybrew/test';

my %error_codes = error_codes();

sub check_test_platform {
    if (! -e $test_repo && ! -e 'c:/repos/berrybrew/test') {
        die "\nCan't continue, test platform not set up... run dev/build_tests.bat\n";
    }
    if ($ENV{PATH} =~ /berrybrew/) {
        die "\nCan't continue, 'berrybrew' is in your path. Please run 'off' on the configured berrybrew instance\n";
    }
}
sub get_avail {
    # returns a list of available strawberry perls that are _not_ already installed
    my $list = `$c available`;
    my @avail = split /\n/, $list;

    @avail = grep {/^\s+/} @avail;

    @avail = grep {$_ !~ /installed/} @avail;

    return @avail;
}
sub get_installed {
    # returns a list of installed strawberry perls
    my $list = `$c available`;
    my @avail = split /\n/, $list;

    @avail = grep {/^\s+.*/} @avail;

    my @installed;

    for (@avail){
        s/^\s+//;
        if (/(.*?)\s+.*\[installed\]/){
            push @installed, $1;
        }
    }

    return @installed;
}
sub error_codes {
    my %codes = (
        GENERIC_ERROR                   => -1,
        SUCCESS                         => 0,
        ADMIN_BERRYBREW_INIT            => 5,
        ADMIN_FILE_ASSOC                => 10,
        ADMIN_PATH_ERROR                => 15,
        ADMIN_REGISTRY_WRITE            => 20,
        ARCHIVE_PATH_NAME_NOT_FOUND     => 25,
        BERRYBREW_UPGRADE_FAILED        => 30,
        DIRECTORY_CREATE_FAILED         => 40,
        DIRECTORY_LIST_FAILED           => 45,
        DIRECTORY_NOT_EXIST             => 50,
        FILE_DELETE_FAILED              => 55,
        FILE_DOWNLOAD_FAILED            => 60,
        FILE_NOT_FOUND_ERROR            => 65,
        FILE_OPEN_FAILED                => 70,
        INFO_OPTION_INVALID_ERROR       => 75,
        INFO_OPTION_NOT_FOUND_ERROR     => 80,
        JSON_FILE_MALFORMED_ERROR       => 85,
        JSON_INVALID_ERROR              => 90,
        JSON_WRITE_FAILED               => 95,
        PERL_ALREADY_INSTALLED          => 98,
        PERL_ARCHIVE_CHECKSUM_FAILED    => 100,
        PERL_CLONE_FAILED               => 105,
        PERL_CLONE_FAILED_IO_ERROR      => 110,
        PERL_FILE_ASSOC_FAILED          => 115,
        PERL_INVALID_ERROR              => 120,
        PERL_MIN_VER_GREATER_510        => 125,
        PERL_NAME_INVALID               => 130,
        PERL_NONE_IN_USE                => 135,
        PERL_NONE_INSTALLED             => 140,
        PERL_NOT_INSTALLED              => 145,
        PERL_REMOVE_FAILED              => 150,
        PERL_TEMP_INSTANCE_NOT_ALLOWED  => 153,
        PERL_UNKNOWN_VERSION            => 155,
        PERL_VERSION_ALREADY_REGISTERED => 160,
        MODULE_IMPORT_FILE_UNAVAIL      => 165,
        MODULE_IMPORT_SAME_VERSION_ERROR=> 170,
        MODULE_IMPORT_VERSION_REQUIRED  => 175,
        OPTION_INVALID_ERROR            => 180,
    );

    return %codes;
}
sub err_code {
    my ($name) = @_;

    die "err_code() requires error name\n" if ! defined $name;

    my @valid_codes = split /\n/, `$c error-codes`;
    is scalar(keys %error_codes), scalar(@valid_codes), "error code count ok compared to valid";
    return $error_codes{$name};
}
sub trap {
    my ($cmd) = @_;
    $? = 0;
    is $?, 0, "\$? reset to exit status 0 ok";
   return capture_stderr { eval { run3 $cmd; }; };
}

1;
