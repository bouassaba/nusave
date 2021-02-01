namespace NuSave.Core
{
  using Newtonsoft.Json;

  public class NuGetPackage
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("authors")]
    public string Authors { get; set; }
  }
}