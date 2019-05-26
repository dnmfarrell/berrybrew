use warnings;
use strict;

use lib 't/';
use BB;
use File::Path qw(rmtree);
use Test::More;
use Win32::TieRegistry;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';

my @installed = BB::get_installed();

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

done_testing();
