{
  description = "GodotEnv: Manage Godot versions and addons from the command line on Windows, macOS, and Linux.";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs =
    { self, ... }@inputs:
    let
      systems = [
        "x86_64-linux"
        "aarch64-linux"
        "x86_64-darwin"
        "aarch64-darwin"
      ];
      forAllSystems = f: inputs.nixpkgs.lib.genAttrs systems (system: f system);

      # Define the godotenv package once for all systems
      godotenvPackage = forAllSystems (
        system:
        let
          pkgs = import inputs.nixpkgs { inherit system; };
        in
        pkgs.callPackage ./nix/godotenv.nix { }
      );
    in
    {
      defaultPackage = godotenvPackage;

      packages = forAllSystems (system: {
        godotenv = godotenvPackage.${system};
        default = godotenvPackage.${system};
      });
    };
}
