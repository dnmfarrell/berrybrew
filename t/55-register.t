use warnings;
use strict;

use lib 't/';
use BB;
use File::Copy;
use File::Path qw(make_path);
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';
my $customfile = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/data/perls_custom.json" : 'c:/repos/berrybrew/test/data/perls_custom.json';

my $dir = 'c:\\berrybrew\\test';

if (! -d $dir){
    mkdir $dir or die $!;
    is -d $dir, 1, "created test dir ok";
}

unlink $customfile or die $! if -f $customfile;

my $o;

mkdir "$dir/empty" or die $!;
is -d "$dir/empty", 1, "created empty installation dir ok";

$o = `$c register empty`;
like $o, qr/empty is not a valid Perl installation/, "no registration if a perl binary not found";


rmdir "$dir/empty" or die $!;
is -d "$dir/empty", undef, "removed empty instance ok";

$o = `$c register not_exist`;
like $o, qr/installation directory.*does not exist/, "won't register if dir doesn't exist ok";


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
like $o, qr/dup.*\[custom/, "registered a valid instance ok";

$o = `$c register dup`;
like $o, qr/dup instance is already registered/, "don't duplicate registration ok";

for (BB::get_installed()){
    $o = `$c remove $_`;
    like $o, qr/Successfully removed/, "removed $_ ok";
}
done_testing();
