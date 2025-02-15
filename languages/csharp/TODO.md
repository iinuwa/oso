# Implementation
- [x] Search for occurences of 'Java' in C# code.
- [ ] Refactor exceptions to use more specific exceptions
- [ ] Review all NotImplementedExceptions
- [ ] Verify which methods need to be public and make all others internal/private
- [x] Query results are too lazy; query errors don't surface until you try to
enumerate. they should happen when the query is returned.
- [ ] An enumerator from a query result can only be enumerated once. Confirm
whether that's expected.
- [ ] Implement rest of tests:
  - [ ] test quickstart? (from test.yml)
  - [ ] top level integration test in /test
    - Probably need to create a csproj, since .NET Core doesn't build without a project file anymore.
  - [x] OsoTests.cs
  - [x] HostTests.cs
  - [x] ResourceBlocksTests.cs
  - [x] EnforcementTests.cs
  - [x] Find replacement for test external/internal unify
- [ ] ExternallCall only works on properties, not fields? 
- [ ] Use ILogger or some other stream for outputting warnings
- [ ] Implement kwargs support

# Refactor
- [ ] Change Assert.True(query.Results.Any()) to use Assert.NotEmpty or Assert.Empty
- [ ] Put exception strings in Resources
# Documentation
- [ ] Update languages/README.md with supported .NET features
- [ ] Document public methods

# Build
- [x] Figure how to package cross-platform
    - place files in 
    - https://medium.com/@toptensoftware/non-trivial-multi-targeting-with-net-7e112f6fd7f2
    - https://github.com/NuGet/docs.microsoft.com-nuget/issues/600
    - `<Content Include="Props\MBI.MSAPP.BuildTargets.NpmBuild.props" PackagePath="runtimes\linux-x64\lib\libpolar.so" />`

- [ ] Support .NET 3.1, 5, 6.0
  - Probably means downgrading to C# 8
- [ ] Support .NET 4.8, 4.7.2?, 4.7.1? 4.6.2?
- [ ] https://docs.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
- [x] Update GitHub Actions scripts