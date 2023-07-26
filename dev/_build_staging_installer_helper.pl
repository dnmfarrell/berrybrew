use warnings;
use strict;

use File::Find::Rule;

use constant {
    INSTALLER_SCRIPT => 'dev/create_staging_installer.nsi',
};

my ($testing) = @ARGV;

if (! $testing && ! grep { -x "$_/makensis.exe" } split /;/, $ENV{PATH}){
    die "makensis.exe not found, check your PATH. Can't build installer...";
}

build();
update_installer_script();
check_installer_manifest();
create_installer();
finish();

sub build {
    system("dev\\build_staging.bat");
}
sub update_installer_script {
    print "\nupdating installer script with version information\n";

    my $bb_ver = _berrybrew_version();

    open my $pfh, '<', 'data/perls.json' or die $!;
    my $most_recent_perl_ver;
    while (<$pfh>){
        if (/"name": "(5\.\d+\.\d+_64)"/){
            $most_recent_perl_ver = $1;
            last;
        }
    }
    close $pfh;
    
    open my $fh, '<', INSTALLER_SCRIPT or die $!;
    my @contents = <$fh>;
    close $fh or die $!;

    for (@contents){
        if (/(PRODUCT_VERSION ".*")$/) {
            s/$1/PRODUCT_VERSION "$bb_ver"/;
        }
        if (/.*(5\.\d+\.\d+_64).*/){
            s/$1/$most_recent_perl_ver/;
        }       
    }

    open my $wfh, '>',  INSTALLER_SCRIPT or die $!;

    for (@contents) {
        print $wfh $_;
    }

    close $wfh;
}
sub check_installer_manifest {
    open my $fh_manifest, '<', 'MANIFEST.STAGING' or die $!;
    open my $fh_manifest_skip, '<', 'MANIFEST.STAGING.SKIP' or die $!;

    my $halt = 0;

    my %skip;

    while (my $entry = <$fh_manifest_skip>) {
        chomp $entry;

        if ($entry =~ m|/$|) {
            $skip{dirs}->{$entry} = 1;
        }
        else {
            $skip{files}->{$entry} = 1;
        }
    }

    my %filtered_files;;

    my @files = File::Find::Rule->file
        ->in('.');

    for (@files) {
        my $include_file = 1;

        for my $skip_dir (keys %{ $skip{dirs} }) {
            if ($_ =~ /^$skip_dir/) {
                $include_file = 0;
                next;
            }
        }

        for my $skip_file (keys %{ $skip{files} }) {
            if ($_ =~ /$skip_file$/) {
                $include_file = 0;
                next;
            }
        }

        if ($include_file) {
            $filtered_files{$_} = $_;
        }
    }

    my %manifest_files;

    while (<$fh_manifest>) {
        chomp;
        $manifest_files{$_} = 1;
    }

    # Compare directory structure to manifest

    for my $dir_file (keys %filtered_files) {
        if (! exists $manifest_files{$dir_file}) {
            $halt = 1;
            print "'$dir_file' is in REPO but isn't in the MANIFEST.\n";
        }
    }

    # Compare manifest to directory structure

    for my $manifest_file (keys %manifest_files) {
        if (! exists $filtered_files{$manifest_file}) {
            $halt = 1;
            print "'$manifest_file' is in the MANIFEST but isn't in the REPO.\n";
        }
    }

    open my $fh, '<', INSTALLER_SCRIPT or die "Can't open installer script: $!";

    my %installer_files;

    while (my $line = <$fh>) {
        if ($line =~ /\s+File\s+"\.\.\\(.*)"/) {
            my $file = $1;
            $file =~ s|\\+|/|g;
            $installer_files{$file} = 1;
            next;
        }
    }

    # Compare installer script to manifest

    for my $installer_file (keys %installer_files) {
        if (! exists $manifest_files{$installer_file}) {
            $halt = 1;
            print "'$installer_file' is in INSTALLER but isn't in the MANIFEST.\n";
        }
    }

    # Compare manifest to installer script

    for my $manifest_file (keys %manifest_files) {
        if (! exists $installer_files{$manifest_file}) {
            $halt = 1;
            print "'$manifest_file' is in MANIFEST but isn't in the INSTALLER.\n";
        }
    }

    if ($halt) {
        print "\nFix the above file discrepancies and run the script again...\n\n";
        exit;
    }
}
sub create_installer {
    system("makensis", INSTALLER_SCRIPT);
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
