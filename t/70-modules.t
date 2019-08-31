use warnings;
use strict;

use lib 't/';
use BB;
use Test::More;
use Win32::TieRegistry;

my $operation_dir = 'test';

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';
#my $c = "$ENV{BBTEST_REPO}/test/berrybrew";

my $install_ok = `$c install 5.10.1_32`;

like $install_ok, qr/5\.10\.1_32.*installed/, "5.10.1_32 installed ok";

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

{
    my $ver = '5.10.1_32';

    my $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\$operation_dir\\$ver/, "PATH set ok for $ver";

    $o = `$c modules export`;

    like $o, qr/successfully.*5\.10\.1_32/, "export routine finished successfuly";

    if ($o =~ /(C:.*?)\s/) {
        my $file = $1;
        is -e $file, 1, "module list file created ok";
        like $file, qr/^C:\\berrybrew\\test\\modules\\5\.10\.1_32$/, "filename for module list ok";

        open my $fh, '<', $file or die "can't open the $file module list file!: $!";
        chomp(my @lines = <$fh>);

        my %file_hash = map {$_ => 1} @lines;

        is exists $file_hash{LWP}, 1, "LWP exists in export";
        is exists $file_hash{'Data::Dumper'}, 1, "Data::Dumper exists in export";
        is exists $file_hash{'JSON'}, 1, "JSON exists in export";
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
