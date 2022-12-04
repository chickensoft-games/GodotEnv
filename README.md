# Chicken

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord](https://img.shields.io/badge/Chickensoft%20Discord-%237289DA.svg?style=flat&logo=discord&logoColor=white)][discord] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Chicken allows you to easily create new Godot projects from reusable templates and manage Godot addons using a simple, flat dependency graph. Chicken is written in C# and provided as a dotnet tool for Windows, macOS, and Linux.

<p align="center">
<img alt="Chicken CLI Logo" src="doc_assets/chicken_cli.svg" width="200">
</p>

## Installation

Chicken uses the local `git` installation available from the shell, so make sure you've installed `git` and [configured your local shell environment][ssh-github] to your liking.

Use the `dotnet` CLI to install Chicken as a global tool:

```shell
$ dotnet tool install -g Chickensoft.Chicken
```

Run Chicken:

```shell
$ chicken --help
```

You can get help for any command by passing the `--help` flag after a command sequence:

```shell
$ chicken addons install --help
```

## What can Chicken do?

Chicken helps with a couple of things: namely, [managing Godot addons](#managing-addons) and generating a group of files or folders from [reusable templates](#templates). 

> **IMPORTANT:** On Windows, Chicken may need to be run from a terminal that is running as an administrator to properly create symlinks.

## Managing Addons

At present, Godot provides two main methods of reuse for C# projects: **nuget packages** and **addons**.

**Nuget packages** allow C# code to be bundled into a library which can be used across multiple projects.

**Addons** allow scenes and scripts to be reused in multiple projects. If you need to make anything other than a code file reusable across Godot projects, you have to use Godot addons.

> If you're just sharing C# code between projects, use a nuget package. If you need to share scenes, resources, or scene scripts, use an addon. That's the general rule, anyways.

Typically, Godot addons are installed through the editor or downloaded manually. With Godot, the convention is to place each addon in its own folder inside a project's top level `addons` folder.

### Why use an addon manager for Godot?

Managing addons in Godot projects has historically been somewhat problematic:

- If you copy and paste an addon into multiple projects, and then modify the addon in one of the projects, the other projects won't get any updates you've made. Duplicated code across projects leads to code getting out of sync, developer frustration, and forgetting which one is most up-to-date.

- If you want to share addons between projects, you might be tempted to use git submodules. Unfortunately, git submodules can be very finnicky when switching branches, and you have to be mindful of which commit you've checked out. If you're careful, they can work out pretty well — but they are a bit fragile and limited in what they can do.

By using a file to require addons, like other dependency systems, Chicken allows addons to be resolved more declaratively and conveniently. Additionally, it's easy to see which addons have changed over time and across different branches.

### Getting Started with Addons

When you run an addon installation command in Chicken, it looks in the current directory for an `addons.json` or `addons.jsonc` file. The `addons.json` or `addons.jsonc` file tells Chicken what addons should be installed in a project.  

> Note that Chicken [supports json files with comments][jsonc], or `jsonc` files.

To get started, create an `addons.json` or `addons.jsonc` file in your game's project directory. If you are using `addons.json`, be sure to remove the comments below.

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

> Each key in the `addons` dictionary above will be the directory name of the installed addon inside the project addons path. 

Install addons:

```shell
$ chicken addons install
```

Chicken can install addons from local and remote git repositories (provided you have setup git in your shell environment), as well as create symlinks to addons.

Chicken caches local and remote git repositories in the cache folder, configured above with the `cache` property in the `addons.json` file (the default is `.addons/`. You can safely delete this folder at any time and Chicken will recreate it next time it installs addons. Deleting the cache forces Chicken to re-download or copy everything on the next install.

> **IMPORTANT:** Add the cache folder to your `.gitignore` file!

Chicken will install addons into the directory specified by the `path` key in the `addons.json` file (which defaults to just `addons/`.

> **IMPORTANT:** If you're using Chicken to install of your addons, you can safely add your `addons` folder to your `.gitignore` file.
>
> Just run `chicken addons install` after cloning your project or whenever your `addons.json` file changes!

### Remote Git Repositories

Chicken can install addons from remote git repositories. Below is the addon specification for an addon from a remote git repository. The url can be any valid git remote url.

```json
{
  "addons": {
    "my_remote_addon": {
      "url": "git@github.com:user/repo.git"
    }
  }
}
```

By default, Chicken assumes the addon `source` is `remote`, the `checkout` reference is `main`, and the `subfolder` to install is the root `/` of the repository. If you need to customize any of those fields, you can override the default values:

```json
{
  "addons": {
    "my_remote_addon": {
      "url": "git@github.com:user/repo.git",
      "source": "remote",
      "checkout": "master",
      "subfolder": "some/folder/inside/the/repo",
    }
  }
}
```

### Local Git Repositories

Chicken can install addons from local git repositories in exactly the same way. Simply provide an absolute or relative path for the url and specify the `source` as `local`:

```json
{
  "addons": {
    "my_remote_addon": {
      "url": "/Users/myself/Desktop/folder",
      "source": "local"
    }
  }
}
```

Just as with remote git repositories, you can override the `checkout` and `subfolder` properties, as well:

```json
{
  "addons": {
    "my_remote_addon": {
      "url": "/Users/myself/Desktop/folder",
      "source": "local",
      "checkout": "master",
      "subfolder": "some/subfolder"
    }
  }
}
```

### Symlink Addons

Finally, Chicken can "install" addons using symlinks. Addons installed with symlinks do not need to point to git repositories — instead, Chicken will create a folder which "points" to another folder on your file system using symbolic linking.

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

### Addons With Dependencies

An addon can itself contain an `addons.json` file. When it is installed, Chicken will also put it in a queue and download any addons it needs. If Chicken detects a potential conflict, it will balk and you will end up with an incomplete addons folder.

Chicken uses a flat dependency graph that is reminiscent of tools like [bower].

Chicken tries to be extremely forgiving and helpful, especially if you try to include the same addon in incompatible configurations (the same addon included under two different names, two different branches of the same repository, etc). Chicken will display warnings and errors as clearly as possible to help you resolve any potential conflicting scenarios that may arise.

### Suppressing Code Analysis of Addons

If you want your IDE to disregard code style warnings for C# code in your addons folder, you can create a `.editorconfig` in your `addons` folder with the following contents:

```editorconfig
[*.cs]
generated_code = true
```

## Templates

Chicken allows you to generate a group of files and folders from a template. A template is any git repository or folder that contains files you'd like to use in the template. Each template must contain an `EDIT_ACTIONS.json` (or `EDIT_ACTIONS.jsonc` file) that tells Chicken how to customize the template each time a new project or folder is generated based on that template.

Edit actions files specify what inputs the template requires and what actions Chicken should perform when generating a new project or folder based on that template.

Edit actions render their property strings before the action is performed by substituting the values received for the inputs and applying any transformations to them.

The following shows an example edit actions file from the [Godot 3 Game Template][godot-3-game-template].

```js
// Edit actions tell Chicken how to customize a folder generated from 
// a template.
// This edit actions file is a snippet of the EDIT_ACTIONS.jsonc file in
// https://github.com/chickensoft-games/godot_3_game.
{
  "inputs": [
    {
      "name": "title",
      "type": "string",
      "default": "My Game"
    }
  ],
  "actions": [
    // Edit game's .csproj file
    {
      // Find and replace text inside a file
      "type": "edit",
      "file": "MyGame.csproj",
      "find": "MyGame",
      "replace": "{title:PascalCase}"
    },
    {
      // Rename file.
      "type": "rename",
      "file": "MyGame.csproj",
      "to": "{title:PascalCase}.csproj"
    },
    {
      // Replace each instance of the text below with a generated guid
      "type": "guid",
      "file": "MyGame.sln",
      "replace": "GUID_PLACEHOLDER"
    },
  ]
}
```

The template only includes 1 input, `title`. The first edit action is simply an `edit` action which performs a find/replace on a file. It replaces each instance of "MyGame" in the `MyGame.csproj` file in the generated folder with the `title` variable in `PascalCase` using the `{variable:transformer}` syntax.

To generate the Godot 3 Game Template with Chicken, you can run the following shell command.

```sh
chicken egg crack ./MyGameName \
  --egg "git@github.com:chickensoft-games/godot_3_game.git" \
  -- --title "MyGameName"
```

Chicken will pass any arguments after `--` to the template itself.

Input variables and transformers are not case sensitive: `{title}`, `{Title}`, and `{TiTlE}` are equivalent.

Given an input string like `MyProject`, we can use transformers to change the case of the text. Chicken supports the following text transformers:

- `snake_case` -> `my_project`
- `pascalcase` -> `MyProject`
- `camelcase` -> `myProject`
- `lowercase` -> `myproject`
- `uppercase` -> `MYPROJECT`

### Edit Actions

Currently, Chicken only supports 3 simple edit actions. Each edit action needs a `file` property that specifies the file the edit action will be performed on. Edit actions can only refer to files in the template folder by relative path, relative to the template root.

```js
{
  "actions": [
    {
      "type": "edit",
      "file": "MyGame.csproj",
      "find": "MyGame",
      "replace": "{title:PascalCase}"
    }
  ]
}
```

In addition to the `file` property, each edit action has its own unique properties to help it accomplish its task.

#### Edit

The `edit` action instructs Chicken to read a file's text contents and run a find/replace on the file. Each instance of the `find` string will be replaced with the `replace` string.

```js
{
  "actions": [
    {
      // Find and replace
      "type": "edit",
      "file": "file.txt",
      "find": "hello, world!",
      "replace": "hello, chicken!"
    }
  ]
}
```

#### Rename

The `rename` action instructs Chicken to rename the `file` to the filename in `to`.

```js
{
  "actions": [
    {
      // Rename a file
      "type": "rename",
      "file": "file.txt",
      "to": "chicken.txt"
    }
  ]
}
```

#### Guid

The `guid` action instructs Chicken to search the contents of `file` and replace each instance of `replace` with a generated GUID (useful when creating templates that have Visual Studio Solution `.sln` files in them). 

```js
{
  "actions": [
    {
      // Replace a placeholder with a GUID
      "type": "guid",
      "file": "file.txt",
      "replace": "GUID_PLACEHOLDER"
    },
  ]
}
```

## Contribution

If you want to contribute, please check out [`CONTRIBUTING.md`](/CONTRIBUTING.md)!


[chickensoft-badge]: https://chickensoft.games/images/chickensoft/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord]: https://discord.gg/gSjaPgMmYW
[line-coverage]: https://raw.githubusercontent.com/chickensoft-games/Chicken/main/Chicken.Tests/reports/line_coverage.svg
[branch-coverage]: https://raw.githubusercontent.com/chickensoft-games/Chicken/main/Chicken.Tests/reports/branch_coverage.svg
[jsonc]: https://code.visualstudio.com/docs/languages/json#_json-with-comments
[ssh-github]: https://docs.github.com/en/authentication/connecting-to-github-with-ssh
[bower]: https://bower.io
[godot-3-game-template]: https://github.com/chickensoft-games/godot_3_game
