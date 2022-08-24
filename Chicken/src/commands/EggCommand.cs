namespace Chickensoft.Chicken {
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Infrastructure;

  [Command("egg", Description = "Manage addons.")]
  public class EggCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) => new();
  }
}
