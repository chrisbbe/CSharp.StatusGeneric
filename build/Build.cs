using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
  "Build",
  GitHubActionsImage.UbuntuLatest,
  On = new[] {GitHubActionsTrigger.Push},
  InvokedTargets = new[] {nameof(Test)}
)]
[GitHubActions(
  "Release",
  GitHubActionsImage.UbuntuLatest,
  OnPushTags = new[] {"v*"},
  InvokedTargets = new[] {nameof(Release)},
  ImportSecrets = new[] {"NUGET_API_KEY"}
)]
[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
sealed class Build : NukeBuild
{
  /// Support plugins are available for:
  ///   - JetBrains ReSharper        https://nuke.build/resharper
  ///   - JetBrains Rider            https://nuke.build/rider
  ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
  ///   - Microsoft VSCode           https://nuke.build/vscode
  public static int Main() => Execute<Build>(x => x.Compile);

  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Parameter] readonly string NugetApiUrl = "https://api.nuget.org/v3/index.json";
  [Parameter] [Secret] readonly string NugetApiKey;

  [Solution] readonly Solution Solution;
  [GitRepository] readonly GitRepository GitRepository;
  [GitVersion] readonly GitVersion GitVersion;

  AbsolutePath SourceDirectory => RootDirectory / "src";
  AbsolutePath TestsDirectory => RootDirectory / "tests";
  AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
  AbsolutePath NugetDirectory => ArtifactsDirectory / "nuget";

  Target Clean => _ => _
    .Before(Restore)
    .Executes(() =>
    {
      SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
      TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
      EnsureCleanDirectory(ArtifactsDirectory);
      EnsureCleanDirectory(NugetDirectory);
    });

  Target Restore => _ => _
    .Executes(() =>
    {
      DotNetRestore(s => s
        .SetProjectFile(Solution));
    });

  Target Compile => _ => _
    .DependsOn(Restore)
    .Executes(() =>
    {
      DotNetBuild(s => s
        .SetProjectFile(Solution)
        .SetConfiguration(Configuration)
        .SetAssemblyVersion(GitVersion.AssemblySemVer)
        .SetFileVersion(GitVersion.AssemblySemFileVer)
        .SetInformationalVersion(GitVersion.InformationalVersion)
        .EnableNoRestore());
    });

  Target Test => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
      DotNetTest(s =>
        s.SetProjectFile(Solution));
    });

  Target NugetPack => _ => _
    .DependsOn(Clean)
    .DependsOn(Test)
    .Executes(() =>
    {
      DotNetPack(s =>
        s.SetProject(Solution)
          .SetConfiguration(Configuration)
          .EnableNoBuild()
          .EnableNoRestore()
          .EnableNoDependencies()
          .SetVersion(GitVersion.NuGetVersionV2)
          .SetOutputDirectory(NugetDirectory));
    });

  Target Release => _ => _
    .DependsOn(NugetPack)
    .Requires(() => NugetApiUrl)
    .Requires(() => NugetApiKey)
    .Requires(() => Configuration.Equals(Configuration.Release))
    .Executes(() =>
    {
      NugetDirectory.GlobFiles("*.nupkg")
        .NotEmpty()
        .Where(x => x.Name.StartsWith("CSharp.StatusGeneric"))
        .ForEach(x =>
        {
          DotNetNuGetPush(s => s
            .SetTargetPath(x)
            .SetSource(NugetApiUrl)
            .SetApiKey(NugetApiKey));
        });
    });
}