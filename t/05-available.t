use warnings;
use strict;

use Test::More;

my $p = 'c:/repos/berrybrew/perl/perl/bin';
my $c = 'c:/repos/berrybrew/bin/berrybrew';

my $list = `$c available`;

#@avail = grep {s/\s+//g; $_ =~ /^5/} @avail;

$list =~ s/\[installed\]\*?//g;

open my $fh, '<', 't/data/available.txt' or die $!;

my $base;

{
    local $/;
    $base = <$fh>;
}
close $fh;

is $list, $base, "berrybrew available ok";

done_testing();
