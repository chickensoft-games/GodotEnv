namespace Chickensoft.GodotEnv.Common.Clients;


public interface IEnvironmentClient {
  string? GetEnvironmentVariable(string name, System.EnvironmentVariableTarget user);

  void SetEnvironmentVariable(string name, string value, System.EnvironmentVariableTarget user);
}

public class EnvironmentClient : IEnvironmentClient {
  public string? GetEnvironmentVariable(string name, System.EnvironmentVariableTarget user) =>
    System.Environment.GetEnvironmentVariable(name, user);

  public void SetEnvironmentVariable(string name, string value, System.EnvironmentVariableTarget user) =>
    System.Environment.SetEnvironmentVariable(name, value, user);
}
