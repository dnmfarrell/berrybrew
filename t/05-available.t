use warnings;
use strict;

use Test::More;

my $p = 'c:/repos/berrybrew/perl/perl/bin';
my $c = 'c:/repos/berrybrew/build/berrybrew';

my $list = `$c available`;

#@avail = grep {s/\s+//g; $_ =~ /^5/} @avail;

$list =~ s/\[installed\]\s+\*?//g;

open my $fh, '<', 't/data/available.txt' or die $!;
my @base = <$fh>;
pop @base;

my @list = split /\n/, $list;

for (@list){
    s/\s+//g;
}

my $i = 0;

for (@base){
    chomp;
    s/\s+//g;
    is $list[$i], $_, ">$list[$i]< :: >$_< ok";
    $i++;
}

done_testing();
