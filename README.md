# GodotEnv

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord](https://img.shields.io/badge/Chickensoft%20Discord-%237289DA.svg?style=flat&logo=discord&logoColor=white)][discord]

<!-- ![line coverage][line-coverage] ![branch coverage][branch-coverage] -->

GodotEnv is a command-line tool that makes it easy to switch between Godot versions and manage addons in your projects.

---

<p align="center">
<img alt="GodotEnv" src="GodotEnv/icon.png" width="200">
</p>

---

GodotEnv can do the following:

- ✅ Download, extract, and install Godot 3.0/4.0+ versions from the command line on Windows, macOS, and Linux (similar to tools like [NVM], [FVM], [asdf], etc.
- ✅ Switch the active version of Godot by updating a symlink.
- ✅ Automatically setup a user `GODOT` environment variable that always points to the active version of Godot.
- ✅ Install addons in a Godot project from local paths, remote git repositories, or symlinks using an easy-to-understand `addons.json` file. No more fighting with git submodules! Just run `godotenv addons install` whenever your `addons.json` file changes.
- ✅ Automatically create and configure a `.gitignore`, `addons.json`, and `addons/.editorconfig` in your project to make it easy to manage addons.
- ✅ Allow addons to declare dependencies on other addons using a flat dependency graph.

## 📦 Installation

GodotEnv is a .NET command line tool that runs on Windows, macOS, and Linux.

```sh
dotnet tool install --global Chickensoft.GodotEnv
```

GodotEnv uses the local `git` installation and other processes available from the shell, so make sure you've installed `git` and [configured your local shell environment][ssh-github] correctly.

> ⧉ On Windows, certain operations may require administrator privileges, such as managing symlinks or editing certain files. GodotEnv should prompt you in these cases for your approval, and certain operations will cause a command line window to pop open for a moment before disappearing — this is normal.

## Quick Start

We'll walk through the commands in depth below, but if you prefer to get started right away you can use the `--help` flag with any command to get more information.

```shell
# Overall help
godotenv --help

# Help for entire categories of commands
godotenv godot --help
godotenv addons --help

# Help for a specific godot management command
godotenv godot install --help

# etc...
```

## 🤖 Godot Version Management

GodotEnv can automatically manage Godot versions on your local machine for you.

> 🙋‍♀️ Using GodotEnv to install Godot works best for local development. If you want to install Godot directly on a GitHub actions runner for CI/CD purposes, consider using Chickensoft's [setup-godot] action — it caches Godot installations between runs, installs the Godot export templates, and also works on Windows, macOS, and Ubuntu GitHub runners.

### Installing Godot

To get started managing Godot versions with GodotEnv, you'll need to first instruct GodotEnv to install a version of Godot.

```sh
godotenv godot install 4.0.1
# or a non-stable version:
godotenv godot install 4.1.1-rc.1
```

Versions should match the format of the versions shown on the [GodotSharp nuget package][godot-sharp-nuget]. Downloads are made from [GitHub Release Builds][github-release-downloads].

By default, GodotEnv installs .NET-enabled versions of Godot.

If you really must install the boring, non-.NET version of Godot, you may do so 😢.

```sh
godotenv godot install 4.0.1 --no-dotnet
```

When installing a version of Godot, GodotEnv performs the following steps:

- 📦 Downloads Godot installation zip archive (if not already downloaded).
- 🤐 Extracts Godot installation zip archive.
- 📂 Activates the newly installed version by updating the symlink.
- 🏝 Makes sure the user `GODOT` environment variable points to the active Godot version symlink.

### Listing Godot Versions

GodotEnv can show you a list of the Godot versions you have installed.

```sh
godotenv godot list
```

Which might produce something like the following, depending on what you have installed:

```text
4.0.1
4.0.1 *dotnet
4.1.1-rc.1
4.1.1-rc.1 *dotnet
```

### Listing Available Godot Versions

GodotEnv also supports showing a list of remote Godot versions available to install using the `-r` option.

```sh
godotenv godot list -r
```

### Using a Different Godot Version

You can change the active version of Godot by instructing GodotEnv to update the symlink to one of the installed versions. By default, it only looks for the .NET-enabled version of Godot. To use a non-.NET version of Godot, specify `--no-dotnet`.

```sh
# uses dotnet version
godotenv godot use 4.0.1

# uses non-dotnet version
godotenv godot use 4.0.1 --no-dotnet
```

### Uninstalling a Godot Version

Uninstalling works the same way as installing and switching versions does.

```sh
# uninstalls .NET version
godotenv godot uninstall 4.0.1

# uninstalls not-dotnet version
godotenv godot uninstall 4.0.1 --no-dotnet
```

### Getting the Symlink Path

GodotEnv can provide the path to the symlink that always points to the active version of Godot.

```sh
godotenv godot env path
```

### Getting the Active Godot Version Path

GodotEnv will provide you with the path to the active version of Godot that the symlink it uses is currently pointing to.

```sh
godotenv godot env target
```

### Getting and Setting the GODOT Environment Variable

You can use GodotEnv to set the `GODOT` user environment variable to the symlink that always points to the active version of Godot.

```sh
# Set the GODOT environment variable to the symlink that GodotEnv maintains.
godotenv godot env setup

# Print the value of the GODOT environment variable.
godotenv godot env get
```

> On Windows, this adds the `GODOT` environment variable to the current user's environment variable.
>
> On macOS, this adds the `GODOT` environment variable to the current user's `.zshrc` file.
>
> On Linux, this adds the `GODOT` environment variable to the current user's `.bashrc` file.
>
> After making changes to environment variables on any system, be sure to close any open terminals and open a new one to ensure the changes are picked up. If changes are not picked up across other applications, you may have to log out and log back in. Fortunately, since the environment variable points to a symlink which points to the active Godot version, you only have to do this once! Afterwards, you are free to switch Godot versions without any further headache as often as you like.

### 🧼 Clearing the Godot Installers Download Cache

GodotEnv caches the Godot installation zip archives it downloads in a cache folder. You can ask GodotEnv to clear the cache folder for you.

```sh
godotenv cache clear
```

## 🔌 Addon Management

GodotEnv allows you to install [Godot addons][asset-library]. A Godot addon is a collection of Godot assets and/or scripts that can be copied into a project. [By convention][godot-addons-structure], these are stored in a folder named `addons` relative to your Godot project. Check out the [Dialogue Manager][godot-dialogue-manager] addon to see how a Godot addon itself is structured.

Besides copying addons from remote sources, GodotEnv allows you to install addons from a local git repository or symlink to local directories on your machine so that you can develop an addon across multiple Godot projects.

Using GodotEnv to manage addons can prevent some of the headaches that occur when using git submodules or manually managing symlinks.

> Additionally, GodotEnv will check for accidental modifications made to addon content files before re-installing addons in your project to prevent overwriting changes you have made. It does this by turning non-symlinked addons into their own temporary git repositories and checking for changes before uninstalling them and reinstalling them.

### When to Use Godot Addons

If you're using C#, you have two ways of sharing code: Godot addons and nuget packages. Each should be used in different scenarios.

- 🔌 **Addons** allow scenes, scripts, or any other Godot assets and files to be reused in multiple Godot projects.

- 📦 **Nuget packages** only allow C# code to be bundled into a library which can be used across multiple Godot projects.

> If you're just sharing C# code between projects, you should use a nuget package or reference another .csproj locally. If you need to share scenes, resources, or any other type of files, use a Godot addon.

#### Why use an addon manager for Godot?

Managing addons in Godot projects has historically been somewhat problematic:

- If you copy and paste an addon into multiple projects, and then modify the addon in one of the projects, the other projects won't get any updates you've made. Duplicated code across projects leads to code getting out of sync, developer frustration, and forgetting which one is most up-to-date.

- If you want to share addons between projects, you might be tempted to use git submodules. Unfortunately, git submodules can be very finnicky when switching branches, and you have to be mindful of which commit you've checked out. Submodules are not known for being friendly to use and can be extremely fragile, even when used by experienced developers.

- GodotEnv allows addons to declare dependencies on other addons. While this isn't a common use case, it will still check for various types of conflicts when resolving addons in a flat dependency graph and warn you if it detects any potential issues.

Using an `addons.json` file allows developers to declare which addons their project needs, and then forget about how to get them. Whenever the addons.json file changes across branches, you can just simply reinstall the addons by running `godotenv addons install` and everything will "just work." Additionally, it's easy to see which addons have changed over time and across different branches — just check the git diff for the `addons.json` file.

### Initializing GodotEnv in a Project

GodotEnv needs to tell git to ignore your addons directory so that it can manage addons instead. Additionally, it will place a `.editorconfig` in your addons directory that will suppress C# code analysis warnings, since C# styles tend to vary drastically.

```sh
godotenv addons init
```

This will add something like the following to your .gitignore file:

```gitignore
# Ignore all addons since they are managed by GodotEnv:
addons/*

# Don't ignore the editorconfig file in the addons directory.
!addons/.editorconfig
```

The `addons init` command will also create a `.editorconfig` in your `addons` directory with the following contents:

```toml
[*.cs]
generated_code = true
```

Finally, GodotEnv will create an example `addons.jsonc` file with the following contents to get you started:

```jsonc
// Godot addons configuration file for use with the GodotEnv tool.
// See https://github.com/chickensoft-games/GodotEnv for more info.
// -------------------------------------------------------------------- //
// Note: this is a JSONC file, so you can use comments!
// If using Rider, see https://youtrack.jetbrains.com/issue/RIDER-41716
// for any issues with JSONC.
// -------------------------------------------------------------------- //
{
  "$schema": "https://chickensoft.games/schemas/addons.schema.json",
  // "path": "addons", // default
  // "cache": ".addons", // default
  "addons": {
    "imrp": { // name must match the folder name in the repository
      "url": "https://github.com/MakovWait/improved_resource_picker",
      // "source": "remote", // default
      // "checkout": "main", // default
      "subfolder": "addons/imrp"
    }
  }
}
```

### Installing Addons

GodotEnv will install addons from symlinks, local paths, or remote git url's using the system shell. Please make sure you've configured git in your shell environment to use any desired credentials, since git will be used to clone local and remote repositories.

```shell
godotenv addons install
```

When you run the addon installation command in GodotEnv, it looks in the **current working directory of your shell** for an `addons.json` or [`addons.jsonc`[jsonc] file. The addons file tells GodotEnv what addons should be installed in a project.  

Here's an example addons file that installs 3 addons, each from a different source (remote git repository, local git repository, and symlink).

```javascript
{
  "path": "addons", // optional — this is the default
  "cache": ".addons", // optional — this is the default
  "addons": {
    "godot_dialogue_manager": {
      "url": "https://github.com/nathanhoad/godot_dialogue_manager.git",
      "source": "remote", // optional — this is the default
      "checkout": "main", // optional — this is the default
      "subfolder": "addons/dialogue_manager" // optional — defaults to "/"
    },
    "my_local_addon_repo": {
      "url": "../my_addons/my_local_addon_repo",
      "source": "local"
    },
    "my_symlinked_addon": {
      "url": "/drive/path/to/addon",
      "source": "symlink"
    }
  }
}
```

> ❗️ Each key in the `addons` dictionary above must be the directory name of the installed addon inside the project addons path. That is, if an addon repository contains its addon contents inside `addons/my_addon`, the name of the key for the addon in the addons file must be `my_addon`.

### Local Addons

If you want to install an addon from a local path on your machine, your local addon must be a git repository. You can specify the `url` as a relative or absolute file path.

```json
{
  "addons": {
    "local_addon": {
      "url": "../my_addons/local_addon",
      "checkout": "main",
      "subfolder": "/",
      "source": "local"
    },
    "other_local_addon": {
      "url": "/Users/me/my_addons/other_local_addon",
      "source": "local"
    },
  }
}
```

### Remote Addons

GodotEnv can install addons from remote git repositories. Below is the addon specification for an addon from a remote git repository. The url can be any valid git remote url.

```json
{
  "addons": {
    "remote_addon": {
      "url": "git@github.com:user/remote_addon.git",
      "subfolder": "addons/remote_addon"
    }
  }
}
```

By default, GodotEnv assumes the addon `source` is `remote`, the `checkout` reference is `main`, and the `subfolder` to install is the root `/` of the repository. If you need to customize any of those fields, you can override the default values:

```json
{
  "addons": {
    "remote_addon": {
      "url": "git@github.com:user/remote_addon.git",
      "source": "remote",
      "checkout": "master",
      "subfolder": "subfolder/inside/repo",
    }
  }
}
```

### Symlink Addons

Finally, GodotEnv can "install" addons using symlinks. Addons installed with symlinks do not need to point to git repositories — instead, GodotEnv will create a folder which "points" to another folder on your file system using symbolic linking.

```json
  "addons": {
    "my_symlink_addon": {
      "url": "/Users/myself/Desktop/folder",
      "source": "symlink"
    },
    "my_second_symlink_addon": {
      "url": "../../some/other/folder",
      "source": "symlink",
      "subfolder": "some_subfolder"
    }
  }
```

> *Note*: The `checkout` reference is ignored when using symlinks.

Whenever a symlinked addon is modified, the changes will immediately appear in the project, unlike addons included with git repositories. Additionally, if you change the addon from your game project, it updates the addon source where the symbolic link is pointing.

> Using symlinks is a great way to include addons that are still in development across one or more projects.

### Addons Configuration

GodotEnv caches local and remote addons in the cache folder, configured above with the `cache` property in the `addons.json` file (the default is `.addons/`, relative to your project). You can safely delete this folder and GodotEnv will recreate it the next time it installs addons. Deleting the cache forces GodotEnv to re-download or copy everything on the next install.

> **IMPORTANT:** Be sure to add the `.addons/` cache folder to your `.gitignore` file!

GodotEnv will install addons into the directory specified by the `path` key in the `addons.json` file (which defaults to just `addons/`).

> Addons should be omitted from source control. If you need to work on an addon at the same time you are working on your Godot project, use GodotEnv to symlink the addon. By omitting the addons folder from source control, you are able to effectively treat addons as immutable packages, like NPM does for JavaScript.
>
> Just run `godotenv addons install` after cloning your project or whenever your `addons.json` file changes!

### Addons for Addons

An addon can itself contain an `addons.json` file that declares dependencies on other addons. When the addon is cached during addon resolution, GodotEnv checks to see if it also contains an `addons.json` file. If it does, GodotEnv will add its dependencies to the queue and continue addon resolution. If GodotEnv detects a potential conflict, it will output warnings that explain any potential pitfalls that might occur with the current configuration.

GodotEnv uses a flat dependency graph that is reminiscent of tools like [bower]. In general, GodotEnv tries to be extremely forgiving and helpful, especially if you try to include addons in incompatible configurations. GodotEnv will display warnings and errors as clearly as possible to help you resolve any potential conflicting scenarios that may arise.

## Contribution

If you want to contribute, please check out [`CONTRIBUTING.md`](/CONTRIBUTING.md)!

While the addons installation logic is well-tested, the Godot version management feature is new and still needs tests. Currently, a GitHub workflow tests it end-to-end. As I have time, I will add more unit tests.

---

🐣 Made with love by 🐤 Chickensoft — <https://chickensoft.games>

[chickensoft-badge]: https://chickensoft.games/images/chickensoft/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord]: https://discord.gg/gSjaPgMmYW
[jsonc]: https://code.visualstudio.com/docs/languages/json#_json-with-comments
[ssh-github]: https://docs.github.com/en/authentication/connecting-to-github-with-ssh
[bower]: https://bower.io
[godot-sharp-nuget]: https://www.nuget.org/packages/GodotSharp/
[github-release-downloads]: https://github.com/godotengine/godot-builds/releases
[NVM]: https://github.com/nvm-sh/nvm
[FVM]: https://github.com/leoafarias/fvm
[asdf]: https://asdf-vm.com/guide/introduction.html
[setup-godot]: https://github.com/chickensoft-games/setup-godot
[godot-addons-structure]: https://docs.godotengine.org/en/stable/tutorials/best_practices/project_organization.html#style-guide
[godot-dialogue-manager]: https://github.com/nathanhoad/godot_dialogue_manager
[asset-library]: https://godotengine.org/asset-library/asset
<!-- [branch-coverage]: ./GodotEnv.Tests/reports/branch_coverage.svg
[line-coverage]: ./GodotEnv.Tests/reports/line_coverage.svg -->
