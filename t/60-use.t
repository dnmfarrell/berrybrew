use warnings;
use strict;

use Test::More;
use IPC::Open3;
use Symbol 'gensym';
use POSIX ':sys_wait_h';
use lib 't/';
use BB;

my $c = $ENV{BBTEST_REPO} ? "$ENV{BBTEST_REPO}/build/berrybrew" : 'c:/repos/berrybrew/build/berrybrew';
my @installed = BB::get_installed();
my @avail = BB::get_avail();

if (! @installed){
    note "\nInstalling $avail[-1] because none were installed\n";
    `$c install $avail[-1]`;
    push @installed, $avail[-1];    # [pryrt] needed, otherwise next block would be skipped
}
note join("\0", '', @installed, ''), $/;
if( join("\0", '', @installed, '') !~ /\0myclone\0/ ) {
    note "\nCloning $installed[-1] to myclone\n";
    `$c clone $installed[-1] myclone`;
    push @installed, 'myclone';    # [pryrt] for consistency
} else {
    note "\nmyclone already exists\n";
}
note "made it beyond";
exit;

{   # testing 'berrybrew use' inside the same "window"
    my $tout = `$c use myclone`;
    note "TOUT = >>$tout<<\n";
    exit;

    my $pid = open3( my $pinn, my $pout, my $perr = gensym, $c, 'use', 'myclone' );
    ok $pinn, 'stdin :'.$pinn;
    ok $pout, 'stdout:'.$pout;
    ok $perr, 'stderr:'.$perr;
    print {$pinn} "where perl\n";
    print {$pinn} "exit\n";
    note join ' ', 'STDOUT: ', <$pout>;
    note join ' ', 'STDERR: ', <$perr>;
    my $t0 = time; note $t0;
    while ( (time() - $t0) < 10 ) { # wait no more than 10s
        my $kid = waitpid($pid, WNOHANG);
        note "waitpid($pid)=$kid";
        last if $kid < 0;
        sleep(1);
    }
    note time;
    note join ' ', 'STDOUT: ', <$pout>;
    note join ' ', 'STDERR: ', <$perr>;
    ok 'hmmm', 'what happened';
}

done_testing();
