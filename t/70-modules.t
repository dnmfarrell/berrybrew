use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

$ENV{BERRYBREW_ENV} = "test";

my $operation_dir = 'test';

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';
#my $c = "$ENV{BBTEST_REPO}/test/berrybrew";

print "Installing Perl 5.16.3_64\n";

my $install_ok = `$c install 5.16.3_64`;

like $install_ok, qr/5\.16\.3_64.*installed/, "5.16.3_64 installed ok";

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

{
    my $ver = '5.16.3_64';

    my $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\$operation_dir\\$ver/, "PATH set ok for $ver";

    print "\nInstalling Mock::Sub\n";
    $o = `cpanm Mock::Sub`;
    
    like $o, qr/(?:Successfully|up to date)/, "successfully installed Mock::Sub";
    
    $o = `$c modules export`;

    like $o, qr/successfully.*5\.16\.3_64/, "export routine finished successfuly";

    if ($o =~ /(C:.*?)\s/) {
        my $file = $1;
        is -e $file, 1, "module list file created ok";
        like $file, qr/^C:\\berrybrew\\test\\modules\\5\.16\.3_64$/, "filename for module list ok";

        open my $fh, '<', $file or die "can't open the $file module list file!: $!";
        my %file_hash;
        
        while (my $line = <$fh>){
            chomp $line;
            $file_hash{$line} = 1;
        }
        
        is exists $file_hash{'JSON::XS'}, 1, "LWP exists in export";
        is exists $file_hash{JSON}, 1, "JSON exists in export";
        is exists $file_hash{'Mock::Sub'}, 1, "Mock::Sub exists in export";
    }
    else {
        is $o, 1, "couln't find module file name in output";
    }
}    

{
    my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
    
    my $o = `$c off`;
    like $o, qr/berrybrew perl disabled/, "off ok";

    my $path = $Registry->{$path_key};
    unlike $path, qr/^C:\\berrybrew\\test/, "PATH set ok for 'off'";
    unlike $path, qr/^C:\\berrybrew\\build/, "PATH set ok for 'off'";
}

done_testing();
