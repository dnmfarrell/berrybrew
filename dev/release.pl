use warnings;
use strict;

use Archive::Zip qw(:ERROR_CODES :CONSTANTS);
use Digest::SHA qw(sha1);
use File::Find::Rule;
use File::Copy;
use JSON::PP;

use constant {
    INSTALLER_SCRIPT => 'dev/create_installer.nsi',
    EXE_FILE         => 'download/berrybrewInstaller.exe',
    ZIP_FILE         => 'download/berrybrew.zip',
};

# run checks

if (! grep { -x "$_/makensis.exe" } split /;/, $ENV{PATH}){
    die "makensis.exe not found, check your PATH. Can't build installer...";
}

my $data_dir = 'data';
my $bak_dir = 'bak';
my $defaults_dir = 'dev/data';

backup_configs();
compile();
create_zip();
create_changes();
update_installer_script();
create_installer();
update_readme();
finish();

sub backup_configs {

    if (!-d $bak_dir) {
        mkdir $bak_dir or die $!;
        print "created backup dir, $bak_dir\n";
    }

    my @files = glob "$data_dir/*";

    for (@files) {
        copy $_, $bak_dir or die $!;
        print "copied $_ to $bak_dir\n";
    }

    @files = glob "$defaults_dir/*";

    for (@files) {
        copy $_, $data_dir or die $!;
        print "copied $_ to $data_dir\n";
    }
}
sub compile {
    print "\ncompiling the API library...\n\n";

    my $api_build = "" .
        "mcs " .
        "src/berrybrew.cs " .
        "-lib:bin " .
        "-t:library " .
        "-r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll " .
        "-out:bin/bbapi.dll";

    system $api_build;

    print "\ncompiling the berrybrew binary...\n";

    my $bin_build = "" .
        "mcs " .
        "src/bbconsole.cs " .
        "-lib:bin  " .
        "-r:bbapi.dll  " .
        "-out:bin/berrybrew.exe " .
        "-win32icon:inc/berrybrew.ico";

    system $bin_build;
    
    print "\ncompiling the berrybrew UI...\n";
 
    my $ui_build = "" .
        "mcs " .
        "-lib:build " .
        "-r:bbapi.dll " .
        "-r:System.Drawing " .
        "-r:System.Windows.Forms " .
        "-win32icon:inc/berrybrew.ico " .
        "-out:bin/berrybrew-ui.exe " .
        "src\berrybrew-ui.cs";
        
    system $ui_build;        
}

sub create_zip {
    print "\npackaging pre-built zipfile...\n";

    my $zip = Archive::Zip->new;

    chdir ".." or die $!;

    $zip->addTree('berrybrew/bin', 'bin', sub {!/Debug/});
    $zip->addTree("berrybrew/$defaults_dir", 'data');
    $zip->writeToFileNamed('berrybrew/download/berrybrew.zip');

    chdir "berrybrew" or die $!;
}
sub create_changes {
    print "\nGenerating a Changes markdown file...\n";

    my $changes = 'Changes';
    my $changes_md = 'Changes.md';

    copy($changes, $changes_md) or die $!;

    open my $changes_fh, '<', $changes or die $!;
    open my $changes_md_wfh, '>', $changes_md or die $!;

    while (<$changes_fh>) {
        if ($_ !~ /^$/ && $_ !~ /^\s+$/) {
            s/^\s+//;
        }
        print $changes_md_wfh $_;
    }
}
sub create_installer {
    system("makensis", INSTALLER_SCRIPT);
}
sub _generate_shasum {
    my ($file) = @_;

    if (! defined $file){
        die "_generate_shasum() requres a filename sent in";
    }

    print "\ncalculating SHA1 for $file...\n";

    my $digest = `shasum $file`;
    $digest = (split /\s+/, $digest)[0];

    return $digest;
}
sub _berrybrew_version {
    open my $fh, '<', 'src/berrybrew.cs' or die $!;

    my $c = 0;
    my $ver;

    while (<$fh>) {

        if (/public string Version\(\)\{/) {
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
        if (/PRODUCT_VERSION "(\d+\.\d+)"$/) {
            s/$1/$bb_ver/;
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
sub update_readme {
    print "\nupdating README with new SHA1 sums and version...\n";

    open my $fh, '<', 'README.md' or die $!;
    my @contents = <$fh>;
    close $fh or die $!;

    my $bb_ver = _berrybrew_version();
    my $zip_sha = _generate_shasum(ZIP_FILE);
    my $exe_sha = _generate_shasum(EXE_FILE);

    my $c = 0;

    for (@contents) {
        if (/^\[berrybrew\.zip.*(`SHA1: \w+`)/) {
            s/$1/`SHA1: $zip_sha`/;
        }
        if (/^\[berrybrewInstaller\.exe.*(`SHA1: \w+`)/) {
            s/$1/`SHA1: $exe_sha`/;
        }
        if (/## Version/) {
            $c++;
            next;
        }
        if ($c == 1) {
            $c++;
            next;
        }
        if ($c == 2) {
            s/\d+\.\d+/$bb_ver/;
            $c++;
        }
    }

    open my $wfh, '>', 'README.md' or die $!;

    for (@contents) {
        print $wfh $_;
    }
}
sub finish {
    print "\nDone!\n";
}
