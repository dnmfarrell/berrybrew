use warnings;
use strict;

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use File::Path qw(rmtree);
use Test::More;
use Win32::TieRegistry;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $operation_dir = 'testing';

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my @installed = BB::get_installed;

if (! grep {'5.16.3_64' eq $_} @installed) {
    print "Installing Perl 5.16.3_64\n";
    my $install_ok = `$c install 5.16.3_64`;
    like $install_ok, qr/5\.16\.3_64.*installed/, "5.10.1_32 installed ok";
}
else {
    print "Perl 5.16.3_64 already installed\n";
}

if (! grep {'5.10.1_32' eq $_} @installed) {
    print "Installing Perl 5.10.1_32\n";
    my $install_ok = `$c install 5.10.1_32`;
    like $install_ok, qr/5\.10\.1_32.*installed/, "5.10.1_32 installed ok";
}
else {
    print "Perl 5.10.1_32 already installed\n";
}

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

{
    `$c off`;

    my $err = BB::trap("$c modules export");
    is $? >> 8, BB::err_code('PERL_NONE_IN_USE'), "exit status ok on export if no perl in use";
    like $err, qr/no Perl is in use/, "export errmsg ok if no perl in use";

    my $o = `$c switch 5.10.1_32`;
    like $o, qr/Switched to Perl version 5.10.1_32/, "switch to 5.10.1_32 ok";

    $err = BB::trap("$c modules export");
    is $? >> 8, BB::err_code('PERL_MIN_VER_GREATER_510'), "exit status ok on export if perl not > 5.10";
    like $err, qr/modules command requires/, "export errmsg ok if perl < 5.10";   

    my $ver = '5.16.3_64';

    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew-$operation_dir\\instance\\$ver/, "PATH set ok for $ver";

    print "\nInstalling Mock::Sub\n";
    $o = `cpanm Mock::Sub`;
    
    like $o, qr/(?:Successfully|up to date)/, "successfully installed Mock::Sub";
    
    $o = `$c modules export`;

    like $o, qr/successfully.*5\.16\.3_64/, "export routine finished successfuly";

    if ($o =~ /(C:.*?)\s/) {
        my $file = $1;
        is -e $file, 1, "module list file created ok";
        like $file, qr/^C:\\berrybrew-testing\\modules\\5\.16\.3_64$/, "filename for module list ok";

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
    unlike $path, qr/^C:\\berrybrew-testing/, "PATH set ok for 'off'";
    unlike $path, qr/^C:\\berrybrew-staging/, "PATH set ok for 'off'";
}

rmtree 'C:/berrybrew-testing/modules' or die $!;
isnt -e 'C:/berrybrew-testing/modules', 1, "removed modules dir ok";

done_testing();
