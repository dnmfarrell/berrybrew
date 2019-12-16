use warnings;
use strict;

use Test::More;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my ($bbpath) = $c =~ m|(.*)/berrybrew|;
$bbpath =~ s|/||g;

my %valid_opts = (
    archive_path    => 'c:\berrybrew\test\temp',
    bin_path        => $bbpath,
    install_path    => $bbpath,
    root_path       => 'C:\berrybrew\test',
);

like `$c info`, qr/requires an option argument/, "info with no args ok";

like `$c info invalid`, qr/is not a valid option/, "info with bad arg ok";

for my $f (keys %valid_opts){
    my $o = `$c info $f`;
    $o =~ tr/\n//d;
    $o =~ s/^\s+//;
    $o =~ s|\\|/|g;
    $o =~ s/\/$//;

    $valid_opts{$f} =~ s/\/$//;
    $valid_opts{$f} =~ s/\\/\//g;

    is lc $valid_opts{$f}, lc $o, "'$f' has proper path returned";
}

done_testing();
