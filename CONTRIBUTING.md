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

To run (but not debug) the tool locally, run the following from the `Chicken` folder.

```sh
dotnet build
dotnet run -- --help
```

You can pass command line flags to Chicken after the double dashes `--`.

Lastly, you can debug the command line tool via the `Debug Chicken CLI` debug configuration in VSCode. Since this profile runs Chicken from it's project directory, it will look for an `addons.json` file in `Chicken/addons.json` and install addons to `Chicken/addons`.

> `Chicken/addons`, `Chicken/.addons`, and `Chicken/addons.json` have been added to `.gitignore` so that you can create them and debug with them as needed.

[reportgenerator]: https://github.com/danielpalme/ReportGenerator
