use warnings;
use strict;

use lib 't/';

use BB;
use IPC::Run3;
use Test::More;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my ($in, $list);

run3 "$c available", \$in, \$list;

is $? >> 8, 0, "success exit code";

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
