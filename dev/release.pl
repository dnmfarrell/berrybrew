use warnings;
use strict;

use Archive::Zip qw(:ERROR_CODES :CONSTANTS);
use Digest::SHA qw(sha1);

# compile

print "compiling the API library...\n\n";

my $api_build = "" . 
    "mcs " .
    "-lib:bin " .
    "-t:library " .
    "-r:ICSharpCode.SharpZipLib.dll,Newtonsoft.Json.dll " .
    "-out:bin/bbapi.dll " .
    "src/berrybrew.cs";

system $api_build;

print "\ncompiling the berrybrew binary...\n";

my $bin_build = "" .
    "mcs " .
    "-lib:bin  " .
    "-r:bbapi.dll  " .
    "-out:bin/berrybrew.exe " .
    "-win32icon:inc/berrybrew.ico " .
    "src/bbconsole.cs";

# zip

print "packaging pre-built zipfile...\n";

my $zip = Archive::Zip->new;

chdir ".." or die $!;

$zip->addDirectory('berrybrew/bin', 'berrybrew/data');
$zip->writeToFileNamed('berrybrew/download/berrybrew.zip');

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

