use warnings;
use strict;
use feature 'say';

use FindBin qw($RealBin);
use lib $RealBin;
use BB;

use Capture::Tiny qw(:all);
use IPC::Run3;
use Test::More;
use Win32::TieRegistry;

BB::check_test_platform();

$ENV{BERRYBREW_ENV} = "testing";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/testing/berrybrew" : 'c:/repos/berrybrew/testing/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

my ($in, $out, $err);
my $o;

$err = BB::trap("$c switch xx");
is $? >> 8, BB::err_code('PERL_UNKNOWN_VERSION'), "exit status ok for unknown perl ver";
like $err, qr/Unknown version of Perl/, "...and STDERR is sane";

{
    my $ver = '5.8.9_32';
    
    if (! installed($ver)) {
        note "# installing $ver";
        `$c install $ver`;
    }

    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew-testing\\instance\\$ver/, "PATH set ok for $ver";

    `$c remove $ver`;
    
    is installed($ver), 0, "$ver removed ok";
}

{
    my $ver = '5.10.1_32';
    
    if (! installed($ver)) {
        note "# installing $ver";
        `$c install $ver`;
    }
    
    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew-testing\\instance\\$ver/, "PATH set ok for $ver";

    `$c remove $ver`;
    is installed($ver), 0, "$ver removed ok";
}

{
    my $ver = '5.16.3';
    
    if (! installed($ver)) {
        note "# installing $ver";
        `$c install $ver`;
    }
    
    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version 5\.16\.3_64/, "switch to 5.16.3_64 without suffix ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew-testing\\instance\\5\.16\.3_64/, "PATH set ok for 5.16.3_64";

    $o = `$c remove $ver`;
    like $o, qr/Successfully removed.*5\.16\.3_64/, "removed 5.16.3_64 without suffix ok";

    is installed($ver), 0, "$ver removed ok";
}

done_testing();

sub installed {
    my ($ver) = @_;
    
    my %installed = map { $_ => 1 } BB::get_installed();
    
    return exists $installed{$ver}
        ? 1
        : 0;
}
