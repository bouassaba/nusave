namespace NuSave.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Core;

  [TestClass]
  public class UnitTests
  {
    [TestMethod]
    public void TestDependencyResolver()
    {
      const string source = "https://api.nuget.org/v3/index.json";

      var cache = new Cache(Environment.CurrentDirectory);

      var dependencyResolver = new DependencyResolver(new DependencyResolver.Options
      {
        Source = source,
        TargetFrameworks = new List<string> {".NETStandard@1.0", ".NETStandard@1.3"},
        AllowPreRelease = true,
        AllowUnlisted = false,
      }, cache);
      dependencyResolver.ResolveByIdAndVersion("Newtonsoft.Json", "12.0.3");

      Assert.IsTrue(dependencyResolver.Dependencies.Any());
    }
  }
}