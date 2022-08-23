// namespace Chickensoft.GoDotAddon.Tests {
//   using System.Collections.Generic;
//   using System.Threading.Tasks;
//   using Moq;
//   using Xunit;

//   public class AddonManagerTest {
//     [Fact]
//     public async Task InstallsAddonsInProject() {
//       var projectPath = "/";
//       var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);
//       var configFileRepo = new Mock<IConfigFileRepo>(MockBehavior.Strict);
//       var reporter = new Mock<IReporter>(MockBehavior.Strict);
//       var dependencyGraph = new Mock<IDependencyGraph>(MockBehavior.Strict);

//       var addon1 = new RequiredAddon(
//           name: "addon1",
//           configFilePath: "/addons.json",
//           url: "http://example.com/addon1.git",
//           checkout: "master",
//           subfolder: "/"
//         );

//       dependencyGraph.Setup(dg => dg.Add(addon1)).Returns(
//         new DependencyInstalledEvent()
//       );

//       var manager = new AddonManager(
//         addonRepo: addonRepo.Object,
//         configFileRepo: configFileRepo.Object,
//         reporter: reporter.Object,
//         dependencyGraph: dependencyGraph.Object
//       );

//       var projectConfigFile = new ConfigFile(
//         addons: new Dictionary<string, AddonConfig>() {
//           { "addon1", new AddonConfig(
//             url: "http://example.com/addon1.git",
//             checkout: "master",
//             subfolder: "addon1"
//           )},
//           { "addon2", new AddonConfig(
//             url: "http://example.com/addon2.git",
//             checkout: "master",
//             subfolder: "addon2"
//           )},
//         },
//         cachePath: ".addons",
//         addonsPath: "addons"
//       );

//       configFileRepo.Setup(repo => repo.LoadOrCreateConfigFile(projectPath))
//         .Returns(projectConfigFile);

//       addonRepo.Setup(repo => repo.LoadCache(
//         new Config(
//           projectPath,
//           "/.addons",
//           "/addons"
//         )
//       )).Returns(Task.FromResult(new Dictionary<string, string>() {
//         { "http://example.com/addon1.git", "addon1" }
//       }));

//       var addon1ConfigFile = new ConfigFile(
//         addons: new Dictionary<string, AddonConfig>(),
//         cachePath: null,
//         addonsPath: null
//       );
//       configFileRepo.Setup(
//         repo => repo.LoadOrCreateConfigFile("/addons/addon1")
//       ).Returns(addon1ConfigFile);

//       await manager.InstallAddons(projectPath);
//     }
//   }
// }
