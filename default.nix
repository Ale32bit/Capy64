{ pkgs ? import <nixpkgs> { system = builtins.currentSystem; } }:
pkgs.buildDotnetModule rec {
  pname = "Capy64";
  version = "1.1.0-beta";

  src = ./.;

  projectFile = "Capy64/Capy64.csproj";
  nugetDeps = ./deps.nix;

  dotnet-sdk = pkgs.dotnetCorePackages.sdk_7_0;
  dotnet-runtime = pkgs.dotnetCorePackages.runtime_7_0;

  meta = with pkgs.lib; {
    homepage = "https://github.com/Ale32bit/Capy64";
    description = "Capy64";
    license = with licenses; [ asl20 ];
  };
}
