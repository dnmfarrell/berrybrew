use warnings;
use strict;

use File::Copy;

my $data_dir = 'data';
my $bak_dir = 'bak';

my @files = glob "$bak_dir/*";

for (@files){
    copy $_, $data_dir or die $!;
    print "copied $_ to $data_dir\n";
}
