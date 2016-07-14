use warnings;
use strict;

$ENV{PATH} 
  = 'C:\Strawberry\c\bin;C:\Strawberry\perl\site\bin;C:\Strawberry\perl\bin;' . $ENV{PATH};

system "prove", "t/*.t";
