use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Data::Dumper;
use Test::More;
use Win32::TieRegistry;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

system("assoc", ".pl=PerlScript");

my $o;

# issue #237
my $pl_assoc_key = "HKEY_CLASSES_ROOT\\.pl\\";
my $bb_key = "HKEY_LOCAL_MACHINE\\SOFTWARE\\berrybrew-testing\\";

delete $Registry->{$bb_key};
# delete $Registry->{$pl_assoc_key};

is $Registry->{$bb_key}, undef, "confirmed berrybrew regkey deleted";
# is $Registry->{$pl_assoc_key}, undef, "confirmed .pl assoc regkey deleted";

like `$c`, qr/view subcommand/, "issue #237 fixed ok";

$Registry->{$pl_assoc_key}{''} = 'PerlScript';
is $Registry->{$pl_assoc_key}{''}, 'PerlScript', "re-added .pl assoc regkey ok";
# end issue #237

system("assoc", ".pl=PerlScript");

$o = `$c options`;

my @option_list = split /\n/, $o;

my %options;

for my $opt (@option_list) {
    next if $opt =~ /^$/;
    next if $opt =~ /Option configuration/;
    $opt =~ s/^\s+//;
    my $opt_name = (split(/:/, $opt))[0];
    $options{$opt_name} = 1;
}

my %test_options = (
    debug           => qr/debug:\s+false/,
    storage_dir     => qr/storage_dir:\s+C:\\berrybrew-testing/,
    instance_dir    => qr/instance_dir:\s+C:\\berrybrew-testing\\instance/,
    temp_dir        => qr/temp_dir:\s+C:\\berrybrew-testing\\temp/,
    custom_exec     => qr/custom_exec:\s+false/,
    windows_homedir => qr/windows_homedir:\s+false/,
    strawberry_url  => qr|strawberry_url:\s+https://strawberryperl.com|,
    warn_orphans    => qr/warn_orphans:\s+false/,
    download_url    => qr|download_url:\s+https://strawberryperl.com/releases.json|,
    run_mode        => qr/run_mode:\s+testing/,
    shell           => qr/shell:\s+cmd/,
    file_assoc      => qr/file_assoc:\s+/,
    file_assoc_old  => qr/file_assoc_old:\s+/,
);

is
    keys %test_options,
    keys %options,
    "we're testing all the known production options ok";

my $aggregate = `$c options`;

for (keys %test_options) {
    like 
        $aggregate, 
        qr/$test_options{$_}/, 
        "option $_ has proper default value ok in aggregate check";

    like 
        `$c options $_`,
        qr/$test_options{$_}/,
        "calling option $_ individually results in proper default value ok";
}

like `$c options run_mode option_test`, qr/^\s+run_mode:\s+option_test\s+$/, "changing run_mode opt ok";
like `$c options run_mode testing`, qr/^\s+run_mode:\s+testing\s+$/, "changing run_mode back ok";

done_testing();
