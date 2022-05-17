namespace GoDotAddon {
  public record RequiredAddon(
    // first package to require an addon gets to set its name
    // (url's and subfolders together make up the id of an addon)
    string Name,
    string Url,
    string Subfolder,
    string Checkout
  ) {
    public override string ToString() =>
      $"Addon **{Name}** to subfolder `{Subfolder}` of `{Url}` from" +
      $" branch `{Checkout}`";
  }
}
