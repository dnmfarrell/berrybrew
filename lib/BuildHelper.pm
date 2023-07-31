package BuildHelper;
asdf;
use warnings;
use strict;

# This library contains functions that both the staging and production build
# environments have in common.

use Exporter qw(import);

our @EXPORT_OK = qw(
    check_installer_manifest
    update_installer_script
    create_installer
);
our %EXPORT_TAGS = (
    all => \@EXPORT_OK,
);
sub create_installer {
    my ($installer_script) = @_;

    if (! $installer_script) {
        die "create_installer(): need installer script sent in";
    }

    system("makensis", $installer_script);
}
sub check_installer_manifest {
    my ($installer_script) = @_;
    
    if (! $installer_script) {
        die "check_installer_manifest() requires the installer script sent in";
    }

    print "Validating the installer MANIFEST...\n";
    
    my $env = $installer_script =~ /staging/
        ? 'staging'
        : 'prod';
   
    my $manifest_file = $env eq 'prod'
        ? 'MANIFEST'
        : 'MANIFEST.STAGING';
    
    my $manifest_skip_file = $env eq 'prod'
        ? 'MANIFEST.SKIP'
        : 'MANIFEST.STAGING.SKIP';
    
    open my $fh_manifest, '<', $manifest_file or die $!;
    open my $fh_manifest_skip, '<', $manifest_skip_file or die $!;

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

    for my $repo_file (@files) {
        my $include_file = 1;

        for my $skip_dir (keys %{ $skip{dirs} }) {
            if ($repo_file =~ /^$skip_dir/) {
                $include_file = 0;
                next;
            }
        }

        for my $skip_file (keys %{ $skip{files} }) {
            if ($repo_file =~ /$skip_file$/) {
                $include_file = 0;
                next;
            }
        }

        if ($include_file) {
            if ($env eq 'staging') {
                $repo_file =~ s|staging/||;
            }
            $filtered_files{$repo_file} = 1;
        }
    }

    my %manifest_files;

    while (<$fh_manifest>) {
        chomp;
        $manifest_files{$_} = 1;
    }

    # Compare repo to manifest

    for my $repo_file (keys %filtered_files) {
        if (! exists $manifest_files{$repo_file}) {
            $halt = 1;
            print "'$repo_file' is in REPO but isn't in the MANIFEST.\n";
        }
    }

    # Compare manifest to repo 

    for my $manifest_file (keys %manifest_files) {
        if (! exists $filtered_files{$manifest_file}) {
            $halt = 1;
            print "'$manifest_file' is in the MANIFEST but isn't in the REPO.\n";
        }
    }

    open my $fh, '<', $installer_script or die "Can't open installer script: $!";

    my $instdir;
    my %installer_files;
    my %uninstall_files;

    while (my $line = <$fh>) {
        chomp $line;

        if ($line =~ /^InstallDir\s+"\$PROGRAMFILES\\(.*)"/) {
            $instdir = $1;
            $instdir =~ s|\\+|/|g;
            $instdir = "C:/Program Files (x86)/$instdir";
        }

        if ($line =~ /\s+File\s+"\.\.\\(.*)"/) {
            my $file = $1;
            $file =~ s|\\+|/|g;

            if ($env eq 'staging') {
                $file =~ s|staging/||;
            }

            $installer_files{$file} = 1;
            next;
        }
        if ($line =~ /\s+Delete\s+"\$INSTDIR\\(.*)"/) {
            my $file = $1;
            next if $file =~ /\.url$/;
            $file =~ s|\\+|/|g;
            $file = "$file";
            $uninstall_files{$file} = 1;
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

    # Compare install files to uninstall files

    for my $installer_file (keys %installer_files) {
        if ($env eq 'staging' && $installer_file =~ /(?:\.exe|\.dll|\.bat)$/) {
            $installer_file = "bin/$installer_file";
        }
        
        if (! exists $uninstall_files{$installer_file}) {
            $halt = 1;
            print "'$installer_file' is in INSTALLER but isn't in UNINSTALL.\n";
        }
    }

    if ($halt) {
        print "\nFix the above file discrepancies and run the script again...\n\n";
        exit;
    }   
}
sub update_installer_script {
    my ($installer_script, $env) = @_;
  
    if (! $installer_script || ! $env) {
        die "update_installer_script() needs installer_script and env";
    }
  
    my $perls_file = $env eq 'prod'
        ? "data/perls.json"
        : "$env/data/perls.json";
    
    print "\nupdating installer script with version information\n";

    my $bb_ver = _berrybrew_version();

    open my $pfh, '<', $perls_file or die $!;

    my $most_recent_perl_ver;

    while (<$pfh>){
        if (/"name": "(5\.\d+\.\d+_64)"/){
            $most_recent_perl_ver = $1;
            last;
        }
    }
    close $pfh;

    open my $fh, '<', $installer_script or die $!;
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

    open my $wfh, '>',  $installer_script or die $!;

    for (@contents) {
        print $wfh $_;
    }

    close $wfh;
}