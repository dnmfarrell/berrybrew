use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $operation_dir = 'build';

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';
#my $c = "$ENV{BBTEST_REPO}/build/berrybrew";

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

{
    my $ver = '5.10.1_32';

    my $o = `$c switch $ver`;
    like $o, qr/Switched to $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\$operation_dir\\$ver/, "PATH set ok for $ver";
    
    $o = `$c modules export`;
   
    like $o, qr/successfully.*5\.10\.1_32/, "export routine finished successfuly";
   
    if ($o =~ /(C:.*?)\s/){
        my $file = $1;
        is -e $file, 1, "module list file created ok";
        like $file, qr/^C:\\berrybrew\\build\\modules\\5\.10\.1_32$/, "filename for module list ok";
        
        open my $fh, '<', $file or die "can't open the $file module list file!: $!";
        chomp (my @lines = <$fh>);
        
        my %file_hash = map {$_ => 1} @lines;

        is exists $file_hash{LWP}, 1, "LWP exists in export";
        is exists $file_hash{'Data::Dumper'}, 1, "LWP exists in export";
        is exists $file_hash{strictures}, 1, "strictures exists in export";
        is exists $file_hash{autodie}, 1, "autodie exists in export";
        is exists $file_hash{'JSON::PP'}, 1, "JSON::PP exists in export";
    }
    else {
       is $o, 1, "couln't find module file name in output"; 
    }
    
}

done_testing();
