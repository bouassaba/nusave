using Newtonsoft.Json;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace NuSave.Core
{
    public class Downloader
    {
        readonly string _outputDirectory;
        readonly string _id;
        readonly string _version;
        readonly bool _allowPreRelease;
        readonly bool _allowUnlisted;
        readonly bool _silent;
        readonly bool _json;
        List<IPackage> _toDownload = new List<IPackage>();

        public Downloader(
            string outputDirectory,
            string id,
            string version,
            bool allowPreRelease = false,
            bool allowUnlisted = false,
            bool silent = false,
            bool json = false)
        {
            _outputDirectory = outputDirectory ?? throw new ArgumentException("outputDirectory cannot be null");
            _id = id ?? throw new ArgumentException("id cannot be null");
            _version = version;
            _allowPreRelease = allowPreRelease;
            _allowUnlisted = allowUnlisted;
            _json = json;
            _silent = _json ? true : silent;
        }

        public void Download()
        {
            if (!_silent)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Downloading");
                Console.ResetColor();
            }

            var webClient = new WebClient();

            foreach (var package in _toDownload)
            {
                if (File.Exists(package.GetFileName())) continue;
                if (Directory.Exists(package.GetHierarchialDirPath(_outputDirectory))) continue;

                if (!_silent)
                {
                    Console.WriteLine($"{package.Id} {package.Version}");
                }

                var dataServcePackage = (DataServicePackage)package;
                webClient.DownloadFile(dataServcePackage.DownloadUrl, Path.Combine(_outputDirectory, package.GetFileName()));
            }
        }

        public void ResolveDependencies()
        {
            if (_toDownload != null && _toDownload.Count > 1)
            {
                _toDownload = new List<IPackage>();
            }

            if (!_silent)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Resolving dependencies");
                Console.ResetColor();
            }

            ResolveDependencies(Package);

            if (_json)
            {
                Console.WriteLine(JsonConvert.SerializeObject(GetDependencies()));
            }
        }        

        void ResolveDependencies(IPackage package)
        {
            foreach (var set in package.DependencySets)
            {
                foreach (var dependency in set.Dependencies)
                {
                    var found = Repository.FindPackage(
                        dependency.Id,
                        dependency.VersionSpec,
                        _allowPreRelease,
                        _allowUnlisted);

                    if (!_toDownload.Any(p => p.Title == found.Title && p.Version == found.Version))
                    {
                        _toDownload.Add(found);
                        if (!_silent)
                        {
                            Console.WriteLine($"{found.Id} {found.Version}");
                        }
                        ResolveDependencies(found);
                    }
                }
            }
        }

        /// <summary>
        /// Convenience method that can be used in powershell in combination with Out-GridView
        /// </summary>
        /// <returns></returns>
        public List<SimplifiedPackageInfo> GetDependencies()
        {
            var list = new List<SimplifiedPackageInfo>();
            foreach (var p in _toDownload)
            {
                list.Add(new SimplifiedPackageInfo
                {
                    Id = p.Id,
                    Version = p.Version.ToString(),
                    Authors = string.Join(" ", p.Authors)
                });
            }
            return list;
        }

        IPackage _package;
        IPackage Package
        {
            get
            {
                if (_package == null)
                {
                    _package = string.IsNullOrWhiteSpace(_version) ?
                          Repository.FindPackage(_id) :
                          Repository.FindPackage(_id, SemanticVersion.Parse(_version), _allowPreRelease, _allowUnlisted);

                    if (_package == null) throw new Exception("Could not resolve package");

                    _toDownload.Add(_package);
                }
                return _package;
            }
        }

        IPackageRepository _repository;
        IPackageRepository Repository
        {
            get
            {
                if (_repository == null)
                {
                    _repository = PackageRepositoryFactory.Default.CreateRepository(
                        "https://packages.nuget.org/api/v2");
                }
                return _repository;
            }
        }
    }
}
