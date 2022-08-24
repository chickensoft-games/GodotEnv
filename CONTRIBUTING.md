# Contributing

Thank you for your interest!

## Code Coverage

The general flow is to open the subfolder `Chicken` and change the code up as needed, and then create or update tests in the `Chicken.Tests` folder. You can open the root of the repo in VSCode to debug the app from the command line, but most test runs will likely be for running tests, which VSCode will allow you to do from the test file itself. Make sure you have the code lens testing features enabled.

For checking test coverage (we require 100%), make sure you have [reportgenerator] installed:

```sh
dotnet tool install -g dotnet-reportgenerator-globaltool
```

From the `Chicken.Tests` folder you can then run the following (in bash) to generate coverage.

```sh
./coverage.sh
```

[reportgenerator]: https://github.com/danielpalme/ReportGenerator
