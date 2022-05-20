namespace Chickensoft.GoDotAddon {
  using CliFx.Infrastructure;

  public interface IReporter {
    void DependencyEvent(ReportableDependencyEvent depEvent);
  }

  public class Reporter : IReporter {
    private readonly ConsoleWriter _output;
    public Reporter(ConsoleWriter output) => _output = output;

    public void DependencyEvent(ReportableDependencyEvent depEvent)
      => _output.Write(depEvent.ToString());
  }
}
