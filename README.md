# GoDotAddon

[![Discord](https://badgen.net/badge/icon/discord?icon=discord&label)]([https://https://discord.com/](https://discord.gg/gSjaPgMmYW)) ![line coverage](GoDotAddon.Tests/reports/line_coverage.svg) ![branch coverage](GoDotAddon.Tests/reports/branch_coverage.svg)

Command-line addon manager for Godot, written in C# and supplied as a dotnet tool for .NET 5 or 6. Uses a flat dependency graph and the system git installation to download and install dependencies from git url's.

## Installation

Coming soon!


## Usage

GoDotAddon uses an `addons.json` file to keep track of which addons you'd like to install. Each addon entry can contain optional information about how the addon should be used.

```json
{
  "addons:" {
    "git@github.com:chickensoft-games/go_dot_net.git": {
      "checkout": "tags/v1.0",
      "subfolder": "/"
    },
    "git@github.com/chickensoft-games/go_dot_test.git": {}
  }
}
```

All addons must have a `url` for the git repository which contains the addon.

Because GoDotAddon relies on the local shell to run git, you can clone from anywhere git has been configured with SSH.

> Not sure how to use SSH with GitHub? [Click here][ssh-github].

An optional `destination` folder allows you to specify where the addon should be copied to when it is installed. If the `destination` property is omitted, the addon will be installed inside the project's `addons` folder under the same name as its entry in `addons.json`.

Each addon can specify an optional `tag` property. When GoDotAddon installs addons, it will checkout the given tag on the addon before copying the addon to the destination folder, allowing you to use specific addon versions (if needed).

The `subfolder` property allows you to only use a certain subfolder from the addon's repository. Whatever is in this folder will be copied to the destination when the addon is installed.

## Ignoring

To properly use GoDotAddon, you should `.gitignore` the following:

```gitignore
.addons/
addons/
```

The `.addons` folder is used as a cache area to download addons before they are copied to the correct location. It can be deleted any time without hurting anything, but it shouldn't ever be committed to source control.

To avoid issues when changing branches which use different addons (or different versions), it is recommended to ignore the entire `addons` folder and use GoDotAddon to install all of your addons.

> If you choose not to let GoDotAddon install all of your addons, you should at least ignore the addons that GoDotAddon will be responsible for installing.

## Modifying Addons

After installing addons, GoDotAddon creates a temporary git repository in each of the addons that have been installed and makes a commit.

If you accidentally change any of the files in the addons folder, GoDotAddon will not proceed with installation (or reinstallation) for that particular addon. You can then use your preferred tooling to view the changes you've made and determine what to do with them.

> GoDotAddon attempts to be as non-destructive as possible, but you should be careful when using any automated tooling as a risk of data loss is always possible. That being said, GoDotAddon tries to be as safe as possible (but I cannot make any guarantees!) If you are worried about data integrity, you should look at the code carefully and make a determination about what is right for your project.

## Installing Addons

To install addons, run the following:

```sh
godotaddon install
```

The command above installs all addons in the `addons.json` file if they are not already present. If any addons are already installed, the command will fail with an error message.

If you have already installed addons, but you want to delete them and reinstall them again, you can run the following command:

```sh
godotaddon uninstall && godotaddon install
```

If you have made changes to any addons since they were installed, GoDotAddon will give you an error and those addons will not be removed. The `&&` bash operator prevents the second installation command from running if the previous command fails, so you can remove the addons you've modified and run the `godotaddon install` command when you are ready.

## Addons with Addons

If an installed addon has its own `addons.json` file, any addons it requires (that don't conflict with any of the other addons) will be downloaded into your project's `addons` folder as well.

**Important:** If more than one addon specifies a different tag for the same repository and subfolder, a conflict will occur and `godotaddon install` will fail with an error message. This is a fundamental limitation of flat dependency graphs.

## Git Hooks

You can optionally reinstall addons automatically after a git checkout. See [post-checkout].

[bower]: https://bower.io
[ssh-github]: https://docs.github.com/en/authentication/connecting-to-github-with-ssh
[post-checkout]: https://git-scm.com/docs/githooks#_post_checkout
