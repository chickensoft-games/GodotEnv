# Chicken

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord](https://img.shields.io/badge/Chickensoft%20Discord-%237289DA.svg?style=flat&logo=discord&logoColor=white)][discord] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Command line utility for C# Godot game development and addon management. Written in C# and provided as a dotnet tool for .NET 6.

## Installation

Chicken uses the local `git` installation available from the shell, so make sure you've installed `git` and configured your local shell environment to your liking.

Use the `dotnet` CLI to install Chicken as a global tool:

```sh
dotnet tool install -g Chickensoft.Chicken
```

Run Chicken:

```sh
chicken --help
```

## Addon Management

Chicken can be used to manage Godot addons (or "eggs") in your game project.

### How Addons Are Managed

Chicken looks for an `addons.json` file in the folder that you execute it from. **You should always run chicken from your project's root folder** (where the `project.godot` file is).

The `addons.json` file allows you to declare which addons you'd like to include in your project, along with options for using just a subfolder of an addon and/or a particular branch or tag, known as the `checkout`.

Chicken uses a cache to download addons you want to include in your project. The cache is stored in a `.addons` folder in your project root. You can change the cache directory by setting the `cache` in your `addons.json` file.

Default settings for `addons.json`:

```json
{
  "path": "addons",
  "cache": ".addons"
}
```

### Setting Up .gitignore 

Add the following to your `.gitignore`:

```gitignore
.addons/
addons/
```

To avoid issues when changing branches which use different addons (or different versions), it is recommended to ignore the entire `addons` folder and use Chicken to install all of your addons. Otherwise, add each folder Chicken creates for the addons it installs to your `.gitignore`.

```gitignore
.addons/
addons/addon_a
addons/addon_b
addons/username_addon_name
```

### Where Addons Are Installed

Addons will be installed to the folder named `addons` in your project root. You can change the installation directory by setting the `path` in your `addons.json` file.

If you're using Chicken to manage all your Godot addons, you can put the `addons/` folder in your `.gitignore` file, as well. Whenever you clone a repo or checkout a different branch with different addons, you can use chicken to reinstall the addons.

Here's an example of an `addons.json` file you might include in the root of your Godot project:

```json
{
  "path": "addons",
  "cache": ".addons",
  "addons": {
    "addon_a": {
      "url": "git@github.com:chickensoft-games/addon_a.git",
      "subfolder": "some-subfolder",
      "checkout": "tags/v1.0.0"
    },
    "addon_b": {
      "url": "git@github.com/chickensoft-games/addon_b.git"
    },
    "addon_c": {
      "url": "git@github.com/chickensoft-games/addon_c.git",
      "checkout": "some-feature"
    }
  }
}
```

The first addon required by the project, `addon_a`, will be pinned to the `v1.0.0` tag and will copy only the files in `some-subfolder`. The second addon, `addon_b`, will use the latest from `main`. The last addon, `addon_c`, installs the addon using the `some-feature` branch of the addon repository.

> Note: You can install two different folders from the same repository url, as long as you give them unique addon names.
>
> ```json
> {
>   "addons": {
>     "addon_a_1": {
>       "url": "git@github.com:chickensoft-games/addon_a.git",
>       "subfolder": "subfolder_a",
>     },
>     "addon_a_2": {
>       "url": "git@github.com:chickensoft-games/addon_a.git",
>       "subfolder": "subfolder_b",
>     },
>   }
> }
> ```

### Installing Addons

Use Chicken to install (or reinstall) all of the addons mentioned in `addons.json`:

```sh
chicken egg install
```

> If your `addons.json` file changes (perhaps because you check out a different branch of your project), you can just run `chicken egg install` again to reinstall the addons.

Under the hood, Chicken runs `git` from the system shell to clone addon repositories to the cache. If you've properly configured git with [ssh keys][ssh-github], Chicken can use git to clone any repo you already have access to.

Once an addon is installed, Chicken creates a temporary git repository in the folder the addon was copied to and makes a single commit. Chicken checks to make sure there aren't any uncommitted changes if it needs to reinstall the addon to avoid accidentally overwriting any changes you might have made.

> Chicken attempts to be as non-destructive as possible, but you should be careful when using any automated tooling as a risk of data loss is always possible.

You can delete the addons cache any time you want. Chicken will just re-download all of the addons it needs to the cache next time you install.

> There's no particular reason for calling addons "eggs." It just makes the command line interface more fun to use.

### When To Use Addons And Nuget Packages

In general, if you are creating a reusable library of C# code, create a nuget package. If you need to create a reusable group of scenes that may (or may not) have scripts attached, use an addon.

### Addons With Dependencies

An addon can itself contain an `addons.json` file. When it is installed, Chicken will also put it in a queue and download any addons it needs. If Chicken detects a potential conflict, it will balk and you will end up with an incomplete addons folder.

Chicken uses a flat dependency graph that harkens back to tools like [bower]. It tries to be forgiving, but if it encounters a scenario it can't support, it tries to display the error as clearly as possible.
### Git Hooks

You can optionally reinstall addons automatically after a git checkout. See [post-checkout].

[chickensoft-badge]: https://chickensoft.games/images/chickensoft/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord]: https://discord.gg/gSjaPgMmYW
[line-coverage]: https://raw.githubusercontent.com/chickensoft-games/Chicken/main/Chicken.Tests/reports/line_coverage.svg
[branch-coverage]: https://raw.githubusercontent.com/chickensoft-games/Chicken/main/Chicken.Tests/reports/branch_coverage.svg

[ssh-github]: https://docs.github.com/en/authentication/connecting-to-github-with-ssh
[bower]: https://bower.io
[post-checkout]: https://git-scm.com/docs/githooks#_post_checkout
[go_dot_dep]: https://github.com/chickensoft-games/go_dot_dep
