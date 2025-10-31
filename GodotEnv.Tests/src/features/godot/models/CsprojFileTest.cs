namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.IO;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Moq;
using Shouldly;
using Xunit;

public class CsprojFileTest
{
  [Fact]
  public void NewCsprojFileHasFilePath()
  {
    var path = "/test/path";
    var file = new CsprojFile(path);
    file.FilePath.ShouldBe(path);
  }

  [Fact]
  public void ParsedVersionIsGodotSdkIfPresent()
  {
    var contents =
        /*lang=xml,strict*/
        """
        <Project Sdk="Godot.NET.Sdk/4.4.1">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>disable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <EnableDynamicLoading>true</EnableDynamicLoading>
            <LangVersion>preview</LangVersion>
            <RootNamespace>TestProject</RootNamespace>
            <DebugType>full</DebugType>
            <DebugSymbols>true</DebugSymbols>

            <!-- Required for some nuget packages to work -->
            <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

            <!-- To show generated files -->
            <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
            <CompilerGeneratedFilesOutputPath>.generated</CompilerGeneratedFilesOutputPath>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="NugetPackage" Version="1.0.0" />
          </ItemGroup>

          <ItemGroup>
            <!-- Include the package to test. -->
            <ProjectReference Include="../SiblingProject/SiblingProject.csproj" />
          </ItemGroup>
        </Project>
        """;
    var reader = new StringReader(contents);
    var path = "/test/path";
    var file = new CsprojFile(path);
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader);
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeTrue();
    parsedVersion.Value.ShouldBe(version);
  }

  [Fact]
  public void ParsedVersionIsFailureIfGodotSdkVersionUnspecified()
  {
    var contents =
        /*lang=xml,strict*/
        """
        <Project Sdk="Godot.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>disable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <EnableDynamicLoading>true</EnableDynamicLoading>
            <LangVersion>preview</LangVersion>
            <RootNamespace>TestProject</RootNamespace>
            <DebugType>full</DebugType>
            <DebugSymbols>true</DebugSymbols>

            <!-- Required for some nuget packages to work -->
            <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

            <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
            <CompilerGeneratedFilesOutputPath>.generated</CompilerGeneratedFilesOutputPath>
          </PropertyGroup>

          <ItemGroup>
            <!-- Test executor. -->
            <PackageReference Include="NugetPackage" Version="1.0.0" />
          </ItemGroup>

          <ItemGroup>
            <!-- Include the package to test. -->
            <ProjectReference Include="../SiblingProject/SiblingProject.csproj" />
          </ItemGroup>
        </Project>
        """;
    var reader = new StringReader(contents);
    var path = "/test/path";
    var file = new CsprojFile(path);
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeFalse();
    parsedVersion.Error.ShouldBe($"csproj file {path} does not use a Godot SDK (found Godot.NET.Sdk)");
  }

  [Fact]
  public void ParsedVersionIsFailureIfGodotSdkNotUsed()
  {
    var contents =
        /*lang=xml,strict*/
        """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>disable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <EnableDynamicLoading>true</EnableDynamicLoading>
            <LangVersion>preview</LangVersion>
            <RootNamespace>TestProject</RootNamespace>
            <DebugType>full</DebugType>
            <DebugSymbols>true</DebugSymbols>

            <!-- Required for some nuget packages to work -->
            <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

            <!-- To show generated files -->
            <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
            <CompilerGeneratedFilesOutputPath>.generated</CompilerGeneratedFilesOutputPath>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="NugetPackage" Version="1.0.0" />
          </ItemGroup>

          <ItemGroup>
            <ProjectReference Include="../SiblingProject/SiblingProject.csproj" />
          </ItemGroup>
        </Project>
        """;
    var reader = new StringReader(contents);
    var path = "/test/path";
    var file = new CsprojFile(path);
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeFalse();
    parsedVersion.Error.ShouldBe($"csproj file {path} does not use a Godot SDK (found Microsoft.NET.Sdk)");
  }

  [Fact]
  public void ParsedVersionIsFailureIfGodotSdkVersionInvalid()
  {
    var contents =
        /*lang=xml,strict*/
        """
        <Project Sdk="Godot.NET.Sdk/not.a.version">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>disable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <EnableDynamicLoading>true</EnableDynamicLoading>
            <LangVersion>preview</LangVersion>
            <RootNamespace>TestProject</RootNamespace>
            <DebugType>full</DebugType>
            <DebugSymbols>true</DebugSymbols>

            <!-- Required for some nuget packages to work -->
            <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

            <!-- To show generated files -->
            <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
            <CompilerGeneratedFilesOutputPath>.generated</CompilerGeneratedFilesOutputPath>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="NugetPackage" Version="1.0.0" />
          </ItemGroup>

          <ItemGroup>
            <!-- Include the package to test. -->
            <ProjectReference Include="../SiblingProject/SiblingProject.csproj" />
          </ItemGroup>
        </Project>
        """;
    var reader = new StringReader(contents);
    var path = "/test/path";
    var file = new CsprojFile(path);
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeFalse();
    parsedVersion.Error.ShouldBe("Couldn't match \"not.a.version\" to known GodotSharp version patterns.");
  }

  [Fact]
  public void WriteGodotVersionThrowsNotSupported()
  {
    var path = "/test/path";
    var fileClient = new Mock<IFileClient>();
    var file = new CsprojFile(path);
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    Should.Throw<NotSupportedException>(() => file.WriteGodotVersion(version, fileClient.Object));
  }
}
