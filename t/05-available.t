use warnings;
use strict;

use Test::More;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my $v = `$c version`;

my $list = `$c available`;

#@avail = grep {s/\s+//g; $_ =~ /^5/} @avail;

$list =~ s/\[installed\]\s+\*?//g;

open my $fh, '<', 't/data/available.txt' or die $!;
my @base = <$fh>;
pop @base;
shift @base;

my @list = split /\n/, $list;
shift @list;

for (@list){
    s/\s+//g;
}

for my $i (0 .. $#base){
    chomp $base[$i];
    $base[$i] =~ s/\s+//g;
    is $list[$i], $base[$i], ">$list[$i]< :: >$base[$i]< ok"
        or die "!\n";
}

done_testing();
