# Add a new exit code

- Look at the existing codes in the `ErrorCodes` `enum` in the `berrybrew.cs`
source file
- Create a reasonable name relative to the usage of the code (again, use your
best judgement along with the existing examples)
- Place the name alphabetically in the existing list, and choose the exit code
that's mid way between the two you're placing this one
- Add the same entry to the `t\BB.pm` unit test helper library, within the
`error_codes()` function
- Execute `perl t\12-exit_codes.t` unit test script, and ensure it passes

&copy; 2016-2023 by Steve Bertrand
