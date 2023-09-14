# Contributing

Thank you for your interest!

The general development flow consists of opening the subfolder `GodotEnv` and changing the code up as needed, and then creating or updating tests in the `GodotEnv.Tests` folder. You can open the root of the repo in VSCode to debug the app from the command line, but most test runs will likely be for running tests, which VSCode will allow you to do from the test file itself (make sure you have the code lens testing features enabled).

To collect test coverage, you can use [reportgenerator]:

```sh
dotnet tool install -g dotnet-reportgenerator-globaltool
```

From the `GodotEnv.Tests` folder you can then run the following (in bash) to generate coverage.

```sh
./coverage.sh
```

To run (but not debug) the tool locally, run the following from the `GodotEnv` folder.

```sh
dotnet build
dotnet run -- --help
```

You can pass command line flags to GodotEnv after the double dashes `--`.

Lastly, you can debug the command line tool via the `Debug GodotEnv CLI` debug configuration in VSCode. An input will open in VSCode which will allow you to type in the command line args you'd like to run the application with before debugging, making it easy to test certain commands and inputs. Since this profile runs the app from it's project directory, it will look for an `addons.json` file in `GodotEnv/addons.json` and install addons to `GodotEnv/addons` when using addons-related commands.

> `GodotEnv/addons`, `GodotEnv/.addons`, and `GodotEnv/addons.json` have been added to `.gitignore` so that you can create them and debug with them as needed.

[reportgenerator]: https://github.com/danielpalme/ReportGenerator
