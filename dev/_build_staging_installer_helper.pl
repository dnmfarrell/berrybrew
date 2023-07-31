use warnings;
use strict;

use FindBin qw($RealBin);

use lib "$RealBin/../lib";

use BuildHelper qw(:all);
use Data::Dumper;
use File::Find::Rule;

use constant {
    INSTALLER_SCRIPT => 'dev/create_staging_installer.nsi',
};

my ($testing) = @ARGV;

if (! $testing && ! grep { -x "$_/makensis.exe" } split /;/, $ENV{PATH}){
    die "makensis.exe not found, check your PATH. Can't build installer...";
}

build();
BuildHelper::update_installer_script(INSTALLER_SCRIPT);
BuildHelper::check_installer_manifest(INSTALLER_SCRIPT);
BuildHelper::create_installer(INSTALLER_SCRIPT);
finish();

sub build {
    system("dev\\build_staging.bat");
}
sub finish {
    print "\nDone!\n";
}

sub _berrybrew_version {
    open my $fh, '<', 'src/berrybrew.cs' or die $!;

    my $c = 0;
    my $ver;

    while (<$fh>) {

        if (/public string Version\(\)\s+\{/) {
            $c = 1;
            next;
        }
        if ($c == 1) {
            ($ver) = $_ =~ /(\d+\.\d+)/;
            last;
        }
    }

    close $fh;

    return $ver;
}
