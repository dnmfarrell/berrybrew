use warnings;
use strict;

die "You must set \$ENV{BBTEST_PERLROOT} before running $0\n" unless exists $ENV{BBTEST_PERLROOT};

my $sff = (@ARGV && (($ARGV[0] eq '--stopfirstfail')||($ARGV[0] eq '--sff')));

my @paths = split /;/, $ENV{PATH};
for (reverse 0 .. $#paths) {
    splice @paths, $_, 1    if $paths[$_] =~ /\b(?:perl|strawberry)\b/i;
}
for (qw/perl\\bin perl\\site\\bin c\\bin bin/) {
    my $path = $ENV{BBTEST_PERLROOT} . $_;
    unshift @paths, $path   if -d $path;
}
$ENV{PATH} = join ';', @paths;

if(!$sff) {
    system "prove", "t/*.t";
} else {
    foreach (glob("t/*.t")) {
        print "before `prove $_`: " . `ls -latr \\berrybrew\\test`;
        system "prove", $_;
        if($? == -1) {
            die sprintf "`prove %s` failed to execute: %s\n", $_, $!;
        } elsif ($? & 127) {
            die sprintf "`prove %s` died with signal %d, %s coredump\n",
                $_, ($? & 127), ($? & 128) ? 'with' : 'without';
        } else {
            my $exit = $? >> 8;
            die sprintf "`prove %s` exited with value %d\n",
                $_, $exit   if $exit;
        }
    }
}