use warnings;
use strict;

use Test::More;
use lib 't/';
use BB;

$ENV{BERRYBREW_ENV} = "test";

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/berrybrew" : 'c:/repos/berrybrew/test/berrybrew';
my $customfile = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/test/data/perls_custom.json" : 'c:/repos/berrybrew/test/data/perls_custom.json';

my $o;

my @avail = BB::get_avail();
my @installed = BB::get_installed();
while(@installed < 1) {
    note "\nInstalling $avail[-1] because only " .scalar(@installed). " test perl".(@installed==1?' was':'s were')." installed\n";
    `$c install $avail[-1]`;

    @installed = BB::get_installed();
    @avail = BB::get_avail();
}

# clone custom

note "\nCloning $installed[-1] to custom\n";
$o = `$c clone $installed[-1] custom`;
ok -s $customfile > 5, "custom perls file size ok after add";

@installed = BB::get_installed();

$o = `$c exec perl -e ''`;
unlike $o, qr/custom/, "custom clone not included in exec";

# manipulate the config

`$c options custom_exec true`;

$o = `$c exec perl -e ''`;
like $o, qr/custom/, "custom clone included in exec when set in conf";

`$c options custom_exec false`;

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
