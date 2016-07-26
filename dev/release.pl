use warnings;
use strict;

use Archive::Zip qw(:ERROR_CODES :CONSTANTS);
use Digest::SHA qw(sha1);
use File::Copy;

# backup configs

my $data_dir = 'data/';
my $bak_dir = 'bak/';
my $defaults_dir = 'dev/data/';

if (! -d $bak_dir){
    mkdir $bak_dir or die $!;
    print "created backup dir, $bak_dir\n";
}

my @files = glob "$data_dir/*";

for (@files){
    copy $_, $bak_dir or die $!;
    print "copied $_ to $bak_dir\n";
}

# copy in defaults

my @files = glob "$defaults_dir/*";

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
    "-r:ICSharpCode.SharpZipLib.dll,Newtonsoft.Json.dll " .
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

#$zip->addDirectory('berrybrew/bin/*', 'berrybrew/data/*');
#$zip->writeToFileNamed('berrybrew/download/berrybrew.zip');

system "zip berrybrew.zip berrybrew/data/* berrybrew/bin/*";
system "mv berrybrew.zip berrybrew/download";

chdir "berrybrew" or die $!;

# sha1

print "\ncalculating SHA1 for zipfile...\n";

my $sha1 = Digest::SHA->new('sha1');

my $file = 'download/berrybrew.zip';

my $digest = `shasum $file`;
$digest = (split /\s+/, $digest)[0];

# update README with SHA1

print "updating README with new SHA1 sum $digest...\n";

open my $fh, '<', 'README.md' or die $!;
my @contents = <$fh>;
close $fh or die $!;

for (@contents){
    if (/.*(`SHA1: \w+`)/){
        s/$1/`SHA1: $digest`/;
    }
};

open my $wfh, '>', 'README.md' or die $!;

for (@contents){
    print $wfh $_;
};



