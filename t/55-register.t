use warnings;
use strict;

use lib 't/';
use BB;
use File::Copy;
use File::Path qw(make_path);
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';
my $customfile = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/data/perls_custom.json" : 'c:/repos/berrybrew/build/data/perls_custom.json';

my $dir = 'c:\\berrybrew\\';
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

if (! @installed){
    note "\nInstalling $avail[-1] because none were installed\n";
    `$c install $avail[-1]`;
    push @installed, $avail[-1];
}

make_path "$dir/valid/perl/bin" or die $!;
is -d "$dir/valid/perl/bin", 1, "created valid test dir ok";

note "\nCopying $dir/$installed[-1] to $dir/valid\n";
copy "$dir/$installed[-1]/perl/bin/perl.exe", "$dir/valid/perl/bin";
is -f "$dir/valid/perl/bin/perl.exe", 1, "test 'valid' directory created ok";

`$c register valid`;

$o = `$c available`;
like $o, qr/valid.*\[custom/, "registered a valid instance ok";

$o = `$c remove valid`;

is -d "$dir/valid", undef, "test valid instance dir removed ok";
like $o, qr/Successfully removed/, "successfully unregistered test instance";

done_testing();
