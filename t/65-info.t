use warnings;
use strict;

use Test::More;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

print "$c\n";

my ($bbpath) = $c =~ m|(.*)/berrybrew|;

my %valid_opts = (
    archive_path    => 'c:\berrybrew\test\temp',
    bin_path        => $bbpath,
    install_path    => $bbpath,
    root_path       => 'C:\berrybrew\test',
);

like `$c info`, qr/requires an option argument/, "info with no args ok";

like `$c info invalid`, qr/is not a valid option/, "info with bad arg ok";

for (keys %valid_opts){
    my $o = `$c info $_`;
    $o =~ tr/\n//d;
    $o =~ s/^\s+//;
    $o =~ s|\\|/|g;
    $o =~ s/\/$//;

    $valid_opts{$_} =~ s/\/$//;
    $valid_opts{$_} =~ s/\\/\//g;
    
    is lc $o, lc $valid_opts{$_}, "'$_' has proper path returned";
}

done_testing();
