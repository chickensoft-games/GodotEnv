# Contributing

Thank you for your interest!

The general development flow consists of opening the subfolder `GodotEnv` and changing the code up as needed, and then creating or updating tests in the `GodotEnv.Tests` folder. 

## Prerequisites

Ensure you have installed the [correct .NET version](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), which currently is .NET 8. If you have `winget` and are developing on Windows, you can install the `Microsoft.DotNet.SDK.8` package [via the command line](https://learn.microsoft.com/en-us/dotnet/core/install/windows?WT.mc_id=dotnet-35129-website#install-with-windows-package-manager-winget).

## Building the Project

To run (but not debug) the tool locally, you can run the following from the `GodotEnv` folder:

```sh
dotnet run --
dotnet run -- --help
```

You can pass command line flags to GodotEnv after the double dashes `--`.

If you use Visual Studio Community and have a .NET environment installed, you can run the tool using the green "Run" button at the top of the interface.

If you use VSCode, you can debug the command line tool via the `Debug GodotEnv CLI` debug configuration. An input will open in VSCode which will allow you to type in the command line args you'd like to run the application with before debugging, making it easy to test certain commands and inputs. Since this profile runs the app from it's project directory, it will look for an `addons.json` file in `GodotEnv/addons.json` and install addons to `GodotEnv/addons` when using addons-related commands.

## Running Test Cases

Most test runs will likely be for running tests, which VSCode will allow you to do from the test file itself (make sure you have the code lens testing features enabled). If you use Visual Studio Community, you can right-click the `GodotEnv.Tests` project and select "Run Tests" to see the results.

To collect test coverage, you can use [reportgenerator]:

```sh
dotnet tool install -g dotnet-reportgenerator-globaltool
```

From the `GodotEnv.Tests` folder you can then run the following (in bash) to generate coverage.

```sh
./coverage.sh
```

> `GodotEnv/addons`, `GodotEnv/.addons`, and `GodotEnv/addons.json` have been added to `.gitignore` so that you can create them and debug with them as needed.

[reportgenerator]: https://github.com/danielpalme/ReportGenerator
