use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Capture::Tiny qw(:all);
use IPC::Run3;
use Test::More;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my $dir = 'c:/berrybrew/testing/';
mkdir $dir or die "Can't create dir $dir: $!" if ! -d $dir;

# warn_orphans

like
    `$c options warn_orphans`,
    qr/warn_orphans:\s+false/,
    "warn_orphans false by default";

like
    `$c options warn_orphans true`,
    qr/warn_orphans:\s+true/,
    "warn_orphans set to true ok";

{
    my @perls = qw(5.99.0 5.005_32);

    for (@perls){
        if (! -d "$dir/$_") {
            mkdir "$dir/$_" or die $!;
            my $o = `$c list`;
            like $o, qr/Orphaned Perl installations/, "orphaned perl $_ caught";
            like $o, qr/$_/, "orphaned perl $_ caught";
        }
    }
}

# warn_orphans
{
    like
        `$c options warn_orphans false`,
        qr/warn_orphans:\s+false/,
        "warn_orphans false ok";

    # false

    my $o = `$c list`;
    like $o, qr/Orphaned Perl installations/, "'list' lists orphans even with warn_orphans disabled";

    $o = `$c available`;
    unlike $o, qr/Orphaned Perl installations/, "no orphans listed for available ok";

    $o = `$c available`;
    unlike $o, qr/Orphaned Perl installations/, "no orphans listed for berrybrew ok";

    like
        `$c options warn_orphans true`,
        qr/warn_orphans:\s+true/,
        "warn_orphans true ok";

    # true

    $o = `$c list`;
    like $o, qr/Orphaned Perl installations/, "'list' lists orphans ok";

    $o = `$c available`;
    like $o, qr/Orphaned Perl installations/, "orphans listed for available ok";

    $o = `$c available`;
    like $o, qr/Orphaned Perl installations/, "orphans listed for berrybrew ok";

    like
        `$c options warn_orphans true`,
        qr/warn_orphans:\s+true/,
        "warn_orphans true ok";
}

# clean orphans
{
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
        like $o, qr/Orphaned Perl installations/, "orphaned perl $_ caught";
    }

    my $o = `$c clean orphan`;
    is -d "$dir/bin", undef, "first valid dir ok";
    is -d "$dir/data", undef, "second valid dir ok";
}

# reset warn_orphans to default false

like
    `$c options warn_orphans false`,
    qr/warn_orphans:\s+false/,
    "warn_orphans back to false";

done_testing();
