namespace NuSave.Tests
{
  using System;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Core;

  [TestClass]
  public class UnitTests
  {
    [TestMethod]
    public void TestDependencyResolver()
    {
      var downloader = new Downloader(null, Environment.CurrentDirectory, "System.Collections", "4.3.0");
      downloader.ResolveDependencies();
      var deps = downloader.GetDependencies();
      Assert.IsTrue(deps.Count > 0);
    }
  }
}