use warnings;
use strict;

use Test::More;
use lib 't/';
use BB;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';
my $customfile = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/data/perls_custom.json" : 'c:/repos/berrybrew/build/data/perls_custom.json';

my $o;

my @avail = BB::get_avail();
my @installed = BB::get_installed();

if (! @installed){
    note "\nInstalling $avail[-1] because none were installed\n";
    `$c install $avail[-1]`;
    push @installed, $avail[-1];    # [pryrt] needed, otherwise cloning $installed[-1]
}

# template

note "\nCloning $installed[-1] to template\n";
$o = `$c clone $installed[-1] template`;
ok -s $customfile > 5, "template perls file size ok after add";

@installed = BB::get_installed();

$o = `$c exec perl -e ''`;
unlike $o, qr/template/, "template clone not included in exec";

$o = `$c remove template`;
like $o, qr/Successfully/, "remove template install ok";

@avail = BB::get_avail();
ok !(grep {'template' eq $_} @avail), "template not in avail";

@installed = BB::get_installed();
ok !(grep {'template' eq $_} @installed), "template not in installed";

is -s $customfile, 2, "custom perls file size ok after remove";

# tmpl

note "\nCloning $installed[-1] to tmpl\n";
$o = `$c clone $installed[-1] tmpl`;
ok -s $customfile > 5, "tmpl perls file size ok after add";

@installed = BB::get_installed();

$o = `$c exec perl -e ''`;
unlike $o, qr/tmpl/, "tmpl clone not included in exec";

$o = `$c remove tmpl`;
like $o, qr/Successfully/, "remove tmpl install ok";

@avail = BB::get_avail();
ok !(grep {'tmpl' eq $_} @avail), "tmpl not in avail";

@installed = BB::get_installed();
ok !(grep {'tmpl' eq $_} @installed), "tmpl not in installed";

is -s $customfile, 2, "custom perls file size ok after remove";

done_testing();
