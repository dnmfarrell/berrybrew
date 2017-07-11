use warnings;
use strict;

use Test::More;
use lib 't/';
use BB;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';
my $customfile = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/data/perls_custom.json" : 'c:/repos/berrybrew/build/data/perls_custom.json';
my $conf = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/data/config.json" : 'c:/repos/berrybrew/build/data/config.json';

my $o;

my @avail = BB::get_avail();
my @installed = BB::get_installed();

if (! @installed){
    note "\nInstalling $avail[-1] because none were installed\n";
    `$c install $avail[-1]`;
    push @installed, $avail[-1];    # [pryrt] needed, otherwise cloning $installed[-1]
}

# clone custom

note "\nCloning $installed[-1] to custom\n";
$o = `$c clone $installed[-1] custom`;
ok -s $customfile > 5, "custom perls file size ok after add";

@installed = BB::get_installed();

$o = `$c exec perl -e ''`;
unlike $o, qr/custom/, "custom clone not included in exec";

# manipulate the config file

open my $fh, '<', $conf or die $!;
my @config = <$fh>;
close $fh;

for (@config){
    if (/custom_exec/){
        s/false/true/;
    }
}

open my $wfh, '>', $conf or die $!;

for (@config){
    print $wfh $_;
}
close $wfh;

$o = `$c exec perl -e ''`;
like $o, qr/custom/, "custom clone included in exec when set in conf";

for (@config){
    if (/custom_exec/){
        s/true/false/;
    }
}

open $wfh, '>', $conf or die $!;

for (@config){
    print $wfh $_;
}
close $wfh;

$o = `$c exec perl -e ''`;
unlike $o, qr/custom/, "custom clone not included in exec after setting config to false";

$o = `$c remove custom`;
like $o, qr/Successfully/, "remove custom install ok";

@avail = BB::get_avail();
ok !(grep {'custom' eq $_} @avail), "custom not in avail";

@installed = BB::get_installed();
ok !(grep {'custom' eq $_} @installed), "custom not in installed";

is -s $customfile, 2, "custom perls file size ok after remove";

done_testing();
