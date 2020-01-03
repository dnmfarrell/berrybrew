use warnings;
use strict;
use feature 'say';

use lib 't/';

use BB;
use Capture::Tiny qw(:all);
use IPC::Run3;
use Test::More;
use Win32::TieRegistry;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

my $path_key = 'HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment\Path';
my $path = $Registry->{$path_key};

my ($in, $out, $err);
my $o;

$err = BB::trap("$c switch xx");
is $? >> 8, BB::err_code('PERL_UNKNOWN_VERSION'), "exit status ok for unknown perl ver";
like $err, qr/Unknown version of Perl/, "...and STDERR is sane";

my @installed = BB::get_installed();
my @avail = BB::get_avail();

while(@installed < 2) {
    note "\nInstalling $avail[-1] because only " .scalar(@installed). " test perl".(@installed==1?' was':'s were')." installed\n";
    `$c install $avail[-1]`;

    @installed = BB::get_installed();
    @avail = BB::get_avail();
}

{
    my $ver = $installed[-1];

    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

{
    my $ver = $installed[-2];

    $o = `$c switch $ver`;
    like $o, qr/Switched to Perl version $ver/, "switch to good $ver ok";

    $path = $Registry->{$path_key};
    like $path, qr/C:\\berrybrew\\test\\$ver/, "PATH set ok for $ver";
}

done_testing();
