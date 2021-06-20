namespace NuSave.Core
{
  using Newtonsoft.Json;
  using NuGet.Protocol.Core.Types;
  using NuGet.Versioning;

  public class Dependency
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("version")]
    public NuGetVersion Version { get; set; }
    
    [JsonIgnore]
    public SourceRepository SourceRepository { get; set; }
  }
}