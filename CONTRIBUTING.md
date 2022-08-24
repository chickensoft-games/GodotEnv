# Contributing

Thank you for your interest!

The general development flow consists of opening the subfolder `Chicken` and changing the code up as needed, and then creating or updating tests in the `Chicken.Tests` folder. You can open the root of the repo in VSCode to debug the app from the command line, but most test runs will likely be for running tests, which VSCode will allow you to do from the test file itself (make sure you have the code lens testing features enabled).

For checking test coverage (we require 100%), make sure you have [reportgenerator] installed:

```sh
dotnet tool install -g dotnet-reportgenerator-globaltool
```

From the `Chicken.Tests` folder you can then run the following (in bash) to generate coverage.

```sh
./coverage.sh
```

To run (but not debug) the tool locally, run the following.

```sh
dotnet build
dotnet run --framework=net6.0 -- --help
```

Make sure you substitute your local framework version (both `net5.0` or `net6.0` are supported). You can pass command line flags to Chicken after the double dashes `--`.

[reportgenerator]: https://github.com/danielpalme/ReportGenerator
