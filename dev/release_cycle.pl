use warnings;
use strict;
use version;

# This script cycles version numbers and the Changes file after a
# release has been performed, and prepares the next version branch

use Data::Dumper;
use Dist::Mgr qw(changes_bump);

my $new_version = calculate_new_version();

checkout_master_branch();
pull_master_branch();
create_version_branch($new_version);
changes_bump($new_version);
update_version($new_version);
commit_version_branch($new_version);
push_version_branch($new_version);

sub calculate_new_version {
    my $version_current = _fetch_current_version();
    return sprintf("%.2f", $version_current + '0.01');
}
sub checkout_master_branch {
    my $output = `git checkout master`;

    if ($output !~ /switched to branch 'master'/) {
        die "Couldn't switch to master branch";
    }
}
sub commit_version_branch {
    my ($bb_ver) = @_;
    my $output = `git commit -am "Bumped to ver $bb_ver"`;

    if ($output !~ /changed/) {
        die "Failed to commit changes to new branch"
    }
}
sub create_version_branch {
    my ($new_version) = @_;

    my $output = `git checkout -b v$new_version`;

    if ($output !~ /Switched to a new branch 'v$new_version'/) {
        die "Couldn't create the new 'v$new_version' branch";
    }
}
sub pull_master_branch {
    my $output = `git pull`;

    if ($output !~ /origin\/master/ || $output !~ /Already up to date/) {
        die "Coudn't pull from master branch" ;
    }
}
sub push_version_branch {
    my ($new_version) = @_;
    my $output = `git push -u origin v$new_version`;

    print "$output\n";

    warn "NO push CHECKS in push_version_branch()!!";
}
sub update_version {
    my ($new_version) = @_;
    _update_src_version(_fetch_current_version(), $new_version);
}

sub _fetch_current_version {
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
sub _update_src_version {
    my ($version_current, $version_new) = @_;

    if (! $version_current || ! $version_new) {
        die "_update_bb_version() requires a current and new version";
    }

    if (version->parse($version_current) >= version->parse($version_new)) {
        die "New version $version_new must be greater than the current one, $version_current";
    }

    open my $fh, '<', 'src/berrybrew.cs' or die $!;
    my @src_lines = <$fh>;
    close $fh;

    for my $line (@src_lines) {
        if ($line =~ /\s+return \@"($version_current)";/) {
            $line =~ s/$1/$version_new/;
        }
    }

    open my $wfh, '>', 'src/berrybrew.cs' or die $!;

    for (@src_lines) {
        print $wfh $_;
    }
}
