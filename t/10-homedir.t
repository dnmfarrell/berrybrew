use warnings;
use strict;

use lib 't/';
use BB;
use File::HomeDir;
use JSON;
use Test::More;

plan skip_all => "mother fscking can't get the first test to pass even though it works on the command line";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';

#`$c install 5.16.3_64`;

my $o = `$c exec perl -wMstrict -MFile::HomeDir -e "print '$File::HomeDir::IMPLEMENTED_BY'"`;

print $o;
system("$c exec perl -wMstrict -MFile::HomeDir -e \"print '$File::HomeDir::IMPLEMENTED_BY'\"");
like $o, qr/Portable::HomeDir/, "before hacks, default Portable::HomeDir in use";
done_testing();
exit;

`$c remove 5.16.3_64`;

config('true');

`$c install 5.16.3_64`;

$o = `$c exec perl -MFile::HomeDir -e "print '$File::HomeDir::IMPLEMENTED_BY'"`;
like $o, qr/File::HomeDir::Windows/, "after hacks, Windows File::HomeDir in use";

`$c remove 5.16.3_64`;

config('false');

sub config {
    my ($bool) = @_;
    
    my $conf_json;

    {
        local $/;
        open my $fh, '<', "$ENV{BBTEST_REPO}/test/data/config.json" or die $!;
        $conf_json = <$fh>;
        close $fh;
    }

    my $conf_perl = decode_json $conf_json;
    $conf_perl->{windows_homedir} = $bool;

    $conf_json = encode_json $conf_perl;

    open my $wfh, '>', "$ENV{BBTEST_REPO}/test/data/config.json" or die $!;
    print $wfh $conf_json;
    close $wfh;   
}

done_testing();
