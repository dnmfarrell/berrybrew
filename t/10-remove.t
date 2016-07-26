use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $c = 'c:/repos/berrybrew/build/berrybrew';

my @installed = BB::get_installed();

if (! @installed){
    plan skip_all => "no perls installed... nothing to do";
}

for (@installed){
    my $o = `$c remove $_`;
    like $o, qr/Successfully/, "$_ removed ok";
}

@installed = BB::get_installed();

is @installed, 0, "all perls removed";

done_testing();
