use warnings;
use strict;

use Archive::Zip qw(:ERROR_CODES :CONSTANTS);
use Digest::SHA qw(sha1);
use Dist::Mgr qw(changes_date);
use File::Copy;
use File::Find::Rule;
use JSON::PP;
use Test::More;

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
update_perls_available();
create_zip();
changes_date();
create_changes();
update_installer_script();
create_installer();
update_readme();
check_readme();
finish();
done_testing();

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
sub check_readme {
    open my $fh, '<', 'README.md' or die "Can't open README: $!";
    my ($bb_sha, $inst_sha, $readme_ver);
    my $ver = _berrybrew_version();
    my $c = 0;

    while (<$fh>) {

        if (/^\[berrybrew\.zip/) {
            if (/^\[berrybrew\.zip.*`SHA1:\s+(.*)`/) {
                $bb_sha = $1;
            }
            like $bb_sha, qr/[A-Fa-f0-9]{40}/, "berrybrew SHA1 ok";
        }
       
        if (/^\[berrybrewInstaller\.exe/) {
            if (/^\[berrybrewInstaller\.exe.*`SHA1:\s+(.*)`/) {
                $inst_sha = $1;
                print(length($1));
            }
            like $inst_sha, qr/[A-Fa-f0-9]{40}/, "berrybrew installer SHA1 ok";
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
            if (/(\d+\.\d+)/) {
                $readme_ver = $1;
            }
            is $readme_ver, $ver, "Version was updated ok";
            $c++;
        }
    }        
}
sub compile {
    print "\ncompiling the berrybrew API...\n";

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
        "csc " .
        "src/berrybrew-ui.cs " .
        "-lib:bin " .
        "-r:bbapi.dll " .
        "-r:System.Drawing.dll " .
        "-r:System.Windows.Forms.dll " .
        "-win32icon:inc/berrybrew.ico " .
        "-t:winexe " .
        "-win32manifest:berrybrew.manifest " .
        "-out:bin/berrybrew-ui.exe ";        

    system $ui_build;        
    
    print "\nCopying berrybrew.exe to bb.exe...\n";
    
    copy 'bin/berrybrew.exe', 'bin/bb.exe' or die $!;
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
sub finish {
    print "\nDone!\n";
}
sub update_perls_available {
    my $out = `bin/berrybrew.exe fetch`;
    like $out, qr/Successfully updated/, "available perls updated ok";
   
    is
        eval { copy 'data/perls.json', 'dev/data/perls.json' or die "can't copy perls.json: $!"; 1 },
        1,
        "data/perls.json copied to dev/data ok";
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
        if (/^\[berrybrew\.zip.*(`SHA1:.*`)/) {
            s/$1/`SHA1: $zip_sha`/;
        }
        if (/^\[berrybrewInstaller\.exe.*(`SHA1:.*`)/) {
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
