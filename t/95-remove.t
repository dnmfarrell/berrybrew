use warnings;
use strict;

use lib 't/';
use BB;
use File::Path qw(rmtree);
use Test::More;
use Win32::TieRegistry;

my $dir = 'c:/berrybrew/test';

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';

my @intalled = BB::get_installed();

if (! @installed){
    plan skip_all => "no perls installed... nothing to do"
}

for (@installed){
    note "\nRemoving $_...\n";
    my $o = `$c remove $_`;
    like $o, qr/Successfully/, "$_ removed ok";
}

@installed = BB::get_installed();

is @installed, 0, "all perls removed";

rmtree $dir;
is -d $dir, undef, "$dir directory removed ok";

done_testing();
