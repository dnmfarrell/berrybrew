#!/bin/sh

# compile

echo "compiling...\n"

mcs \
    -lib:lib \
    -r:ICSharpCode.SharpZipLib.dll,Newtonsoft.Json.dll \
    -out:bin/berrybrew.exe \
    -win32icon:berrybrew.ico src/berrybrew.cs

# zip

echo "packaging pre-built zipfile...\n"

cd ..
zip berrybrew.zip berrybrew/bin/* berrybrew/perls.json
mv berrybrew.zip berrybrew/
cd berrybrew

# sha1

echo "\ncalculating SHA1 for zipfile...\n"

sha_string=$(sha1sum berrybrew.zip)
export BB_SHA1="$sha_string"

# update README with SHA1

echo "updating README with new SHA1 sum...\n"

perl -i -pe 'if (/.*(`SHA1: \w+`)/){$csum = (split /\s+/, $ENV{BB_SHA1})[0];s/$1/`SHA1: $csum`/}' \
    README.md
