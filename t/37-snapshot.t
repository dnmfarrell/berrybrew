use warnings;
use strict;

use Data::Dumper;
use File::Path qw(rmtree);
use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Test::More;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my $snapdir = 'C:/berrybrew/testing/snapshots';

note "\nInstalling 5.10.1_32\n";
`$c install 5.10.1_32`;

my @installed = BB::get_installed();
is @installed, 1, "only one perl installed ok";

my $installed_ok = grep { $_ eq '5.10.1_32' } @installed;

is $installed_ok, 1, "5.10.1_32 is installed ok";

# export - custom name

`$c snapshot export 5.10.1_32 unit_test`;

is -d $snapdir, 1, "snapshots directory created ok";
is -e "$snapdir/unit_test.zip", 1, "custom named snapshot saved ok";

# export - timestamp name

`$c snapshot export 5.10.1_32`;

my $timestamp_name_list = `$c snapshot list`;
my $timestamp_snapshot;

like
    $timestamp_name_list,
    qr/5\.10\.1_32\.\d{14}/,
    "with no snapshot name, export appends a timestamp to instance ok";

if ($timestamp_name_list =~ /(5\.10\.1_32\.\d{14})/) {
    $timestamp_snapshot = $1;
    is 
        -e "$snapdir/$timestamp_snapshot.zip", 
        1, 
        "timestamped export $snapdir/$timestamp_snapshot.zip exists on fs ok";
}

# import timestamp

`$c remove 5.10.1_32`;
@installed = BB::get_installed();

is @installed, 0, "removed all versions of perl ok";

`$c snapshot import $timestamp_snapshot`;
@installed = BB::get_installed();

is $installed[-1], '5.10.1_32', "timestamp snapshot import removes timestamp ok";

# import timestamp duplicate

my $installed_err = BB::trap("$c snapshot import $timestamp_snapshot");

is 
    $? >> 8, 
    BB::err_code('PERL_ALREADY_INSTALLED'), 
    "attempt to re-install snapshot fails ok";

like 
    $installed_err, 
    qr/already installed/, 
    "...and error message is ok";

# import with custom instance name

`$c snapshot import $timestamp_snapshot snap_import`;

@installed = BB::get_installed();
is $installed[-1], 'snap_import', "import with custom instance name ok";

my $custom_list = `$c list`;

print $custom_list;

for (BB::get_installed()) {
    print ">$_\n";
    exit;
    `$c remove $_`;
}

is BB::get_installed(), 0, "all Perls cleaned up ok";

rmtree $snapdir or die $!;

is -d $snapdir, undef, "removed snapshot dir ok";

done_testing();
exit;
#@installed = BB::get_installed();

#like
#    { $installed[-1] }
#    qr/unit_test\.\d{14}/,
#    "unnamed snapshot saved with timestamp ok";
#

# import unnamed instance

#`$c snapshot import unit_test`;

__END__
my @avail = BB::get_avail();

my $pre_installed = BB::get_installed();

my $unknown_err = BB::trap("$c install 9.10.11");
is $? >> 8, BB::err_code('PERL_UNKNOWN_VERSION'), "if unknown version, exit ok";
like $unknown_err, qr/unknown version/, "...and error message is ok";

note "\nInstalling $avail[-1]\n";
`$c install $avail[-1]`;

my $err = BB::trap("$c install $avail[-1]");
is $? >> 8, BB::err_code('PERL_ALREADY_INSTALLED'), "if perl is installed, exit status ok";
like $err, qr/already installed/, "...and error message is ok";

note "\nInstalling 5.16.3\n";
`$c install 5.16.3`;
like `$c list`, qr/5\.16\.3_64/, "install with no suffix installs 64-bit perl ok";

my $post_installed = BB::get_installed();

ok $post_installed > $pre_installed, "install ok";

done_testing();
