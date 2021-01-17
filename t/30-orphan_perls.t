use warnings;
use strict;

use lib 't/';

use BB;
use Capture::Tiny qw(:all);
use IPC::Run3;
use Test::More;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my $dir = 'c:/berrybrew/test/';

{
    my @perls = qw(5.99.0 5.005_32);

    for (@perls){
        if (! -d "$dir/$_") {
            mkdir "$dir/$_" or die $!;
        }            
        my $o = `$c`;
        like $o, qr/WARNING! orphaned/, "orphaned perl $_ caught";
    }

    my ($out, $err) = capture {
        eval { run3 "$c clean orphan"; };
    };

    my $errcode = $? >> 8;
    is $errcode, 0, "exit status success ok";
    
    like $out, qr/5\.99\.0/, "first orphan removal logged";
    like $out, qr/5\.005_32/, "second orphan removal logged";

    is -d "$dir/5.99.0", undef, "first orphan deleted";
    is -d "$dir/5.005_32", undef, "second orphan deleted";
}

{ # data/bin dirs

    my @dirs = qw(data bin);

    for (@dirs){
        mkdir "$dir/$_" if ! -d "$dir/$_" or die $!;
        my $o = `$c`;
        like $o, qr/WARNING! orphaned/, "orphaned perl $_ caught";
    }

    my $o = `$c clean orphan`;
    is -d "$dir/bin", undef, "first valid dir ok";
    is -d "$dir/data", undef, "second valid dir ok";
}

done_testing();
