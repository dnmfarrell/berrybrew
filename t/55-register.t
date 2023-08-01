use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Capture::Tiny qw(:all);
use File::Copy;
use File::Path qw(make_path);
use IPC::Run3;
use Test::More;
use Win32::TieRegistry;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';
my $customfile = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/data/perls_custom.json" : 'c:/repos/berrybrew/testing/data/perls_custom.json';

my $dir = 'c:\\berrybrew-testing\\instance';

if (! -d $dir){
    mkdir $dir or die $!;
    is -d $dir, 1, "created test dir ok";
}

unlink $customfile or die $! if -f $customfile;

my ($o, $err);

if (! -d "$dir/empty") {
    mkdir "$dir/empty" or die $!;
}    
is -d "$dir/empty", 1, "created empty installation dir ok";

$err = BB::trap("$c register empty");
is $? >> 8, BB::err_code('PERL_INVALID_ERROR'), "registration failure if perl binary not found sets exit status ok";
like $err, qr/empty is not a valid Perl installation/, "no registration if a perl binary not found error msg ok";

rmdir "$dir/empty" or die $!;
is -d "$dir/empty", undef, "removed empty instance ok";

$err = BB::trap("$c register not_exist");
is $? >> 8, BB::err_code('DIRECTORY_NOT_EXIST'), "register failure if perl dir no exist sets exit status ok";
like $err, qr/installation directory.*does not exist/, "won't register if dir doesn't exist errmsg ok";

my @avail = BB::get_avail();
my @installed = BB::get_installed();
while(@installed < 1) {
    note "\nInstalling $avail[-1] because only " .scalar(@installed). " test perl".(@installed==1?' was':'s were')." installed\n";
    `$c install $avail[-1]`;

    @installed = BB::get_installed();
    @avail = BB::get_avail();
}

make_path "$dir/valid/perl/bin" or die $!;
is -d "$dir/valid/perl/bin", 1, "created valid test dir ok";

note "\nCopying $dir/$installed[-1] to $dir/valid\n";
copy "$dir/$installed[-1]/perl/bin/perl.exe", "$dir/valid/perl/bin";
is -f "$dir/valid/perl/bin/perl.exe", 1, "test 'valid' directory created ok";

$o = `$c register valid`;
like $o, qr/Successfully registered/, "register has ok output";

$o = `$c available`;
like $o, qr/valid.*\[custom/, "registered a valid instance ok";

$o = `$c remove valid`;

is -d "$dir/valid", undef, "test valid instance dir removed ok";
like $o, qr/Successfully removed/, "successfully unregistered test instance";

make_path "$dir/dup/perl/bin" or die $!;
is -d "$dir/dup/perl/bin", 1, "created dup test dir ok";

note "\nCopying $dir/$installed[-1] to $dir/dup\n";
copy "$dir/$installed[-1]/perl/bin/perl.exe", "$dir/dup/perl/bin";
is -f "$dir/dup/perl/bin/perl.exe", 1, "test 'dup' directory created ok";

`$c register dup`;

$o = `$c available`;
like $o, qr/dup.*\[custom/, "registered a valid instance (dup) ok";

$err = BB::trap("$c register dup");
is $? >> 8, BB::err_code('PERL_VERSION_ALREADY_REGISTERED'), "exit status for already registered Perl ok";
like $err, qr/dup instance is already registered/, "don't duplicate registration errmsg ok";

for (BB::get_installed()){
    $o = `$c remove $_`;
    like $o, qr/Successfully removed/, "removed $_ ok";
}
done_testing();
