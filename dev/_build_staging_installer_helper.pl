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

# Catch 22... we need to have the installer binary available
# before we can create one. We'll create a dummy

if (! -e 'staging/berrybrewInstaller.exe') {
    open my $fh, '>', 'staging/berrybrewInstaller.exe' or die $!;
    print $fh 'init';
    close $fh;
}

build();
BuildHelper::check_installer_manifest(INSTALLER_SCRIPT);
BuildHelper::update_installer_script(INSTALLER_SCRIPT);
BuildHelper::create_installer(INSTALLER_SCRIPT);
finish();

sub build {
    system("dev\\build_staging.bat");
}
sub finish {
    print "\nDone!\n";
}
