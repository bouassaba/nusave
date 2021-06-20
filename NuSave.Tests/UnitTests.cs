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
      var cache = new Cache(Environment.CurrentDirectory);
      var dependencyResolver = new DependencyResolver(new DependencyResolver.Options
      {
        Sources = new List<string>{"https://api.nuget.org/v3/index.json"},
        TargetFrameworks = new List<string> {".NETStandard@1.0", ".NETStandard@1.3"},
        AllowPreRelease = true,
        AllowUnlisted = false,
      }, cache);
      dependencyResolver.ResolveByIdAndVersion("Newtonsoft.Json", "12.0.3");

      Assert.IsTrue(dependencyResolver.Dependencies.Any());
    }
  }
}