{
  pkgs ? import <nixpkgs> { },
}:

pkgs.callPackage ./nix/godotenv.nix { }
