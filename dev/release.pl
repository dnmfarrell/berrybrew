use warnings;
use strict;

use Archive::Zip qw(:ERROR_CODES :CONSTANTS);
use Digest::SHA qw(sha1);
use File::Find::Rule;
use File::Copy;

use constant {
    INSTALLER_SCRIPT => 'dev/create_installer.nsi',
};

# run checks

if (! grep { -x "$_/makensis.exe" } split /;/, $ENV{PATH}){
    die "makensis.exe not found, check your PATH. Can't build installer...";
}

create_installer();
exit;

# backup configs

my $data_dir = 'data';
my $bak_dir = 'bak';
my $defaults_dir = 'dev/data';

if (! -d $bak_dir){
    mkdir $bak_dir or die $!;
    print "created backup dir, $bak_dir\n";
}

my @files = glob "$data_dir/*";

for (@files){
    copy $_, $bak_dir or die $!;
    print "copied $_ to $bak_dir\n";
}

@files = glob "$defaults_dir/*";

for (@files){
    copy $_, $data_dir or die $!;
    print "copied $_ to $data_dir\n";
}

# compile

print "compiling the API library...\n\n";

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

# zip

print "packaging pre-built zipfile...\n";

my $zip = Archive::Zip->new;

chdir ".." or die $!;

$zip->addTree('berrybrew/bin', 'bin', sub {! /Debug/});
$zip->addTree("berrybrew/$defaults_dir", 'data');
$zip->writeToFileNamed('berrybrew/download/berrybrew.zip');

chdir "berrybrew" or die $!;

# sha1

print "\ncalculating SHA1 for zipfile...\n";

my $sha1 = Digest::SHA->new('sha1');

my $file = 'download/berrybrew.zip';

my $digest = `shasum $file`;
$digest = (split /\s+/, $digest)[0];

# update README with SHA1, and version

print "updating README with new SHA1 sum $digest, and version...\n";

open my $fh, '<', 'src/berrybrew.cs' or die $!;

my $c = 0;
my $ver;

while (<$fh>){

    if (/public string Version\(\)\{/){
        $c = 1;
        next;
    }
    if ($c == 1){
        ($ver) = $_ =~ /(\d+\.\d+)/;
        last;
    }
}

close $fh;

open $fh, '<', 'README.md' or die $!;
my @contents = <$fh>;
close $fh or die $!;

$c = 0;

for (@contents){
    if (/.*(`SHA1: \w+`)/){
        s/$1/`SHA1: $digest`/;
    }
    if (/## Version/){
        $c++;
        next;
    }
    if ($c == 1){
        $c++;
        next;
    }
    if ($c == 2){
        s/\d+\.\d+/$ver/;
        $c++;
    }
}

open my $wfh, '>', 'README.md' or die $!;

for (@contents){
    print $wfh $_;
}

# create a Changes.md for Github viewing

print "\nGenerating a Changes markdown file...\n";

my $changes = 'Changes';
my $changes_md = 'Changes.md';

copy($changes, $changes_md) or die $!;

open my $changes_fh, '<', $changes or die $!;
open my $changes_md_wfh, '>', $changes_md or die $!;

while (<$changes_fh>){
    if ($_ !~ /^$/ && $_ !~ /^\s+$/){
        s/^\s+//;
    }
    print $changes_md_wfh $_;
} 

sub create_installer {
    system("makensis", INSTALLER_SCRIPT);
}

print "\nDone!\n";
