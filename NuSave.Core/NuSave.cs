using Newtonsoft.Json;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace NuSave.Core
{
    public class Downloader
    {
        const string DefaultSource = "https://packages.nuget.org/api/v2";
        readonly string _source;
        readonly string _outputDirectory;
        readonly string _id;
        readonly string _version;
        readonly bool _allowPreRelease;
        readonly bool _allowUnlisted;
        readonly bool _silent;
        readonly bool _json;
        List<IPackage> _toDownload = new List<IPackage>();

        public Downloader(
            string source,
            string outputDirectory,
            string id,
            string version,
            bool allowPreRelease = false,
            bool allowUnlisted = false,
            bool silent = false,
            bool json = false)
        {
            _source = source;
            _outputDirectory = outputDirectory;
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
                if (PackageExists(package.Id, package.Version.ToString())) continue;

                if (!_silent)
                {
                    Console.WriteLine($"{package.Id} {package.Version}");
                }

                var dataServcePackage = (DataServicePackage)package;
                // We keep retrying forever until the user will press Ctrl-C
                // This lets the user decide when to stop retrying.
                // The reason for this is that building the dependencies list is expensive
                // on slow internet connection, when the CLI crashes because of a WebException
                // the user has to wait for the dependencies list to build rebuild again.
                while (true)
                {
                    try
                    {
                        webClient.DownloadFile(dataServcePackage.DownloadUrl, Path.Combine(_outputDirectory, package.GetFileName()));
                        break;
                    }
                    catch (WebException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{e.Message}. Retrying in one second...");
                        Console.ResetColor();
                        Thread.Sleep(1000);
                    }
                }   
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
        
        bool PackageExists(string id, string version)
        {
            string nupkgFileName = $"{id}.{version}.nupkg".ToLower();

            if (File.Exists(Path.Combine(_outputDirectory, nupkgFileName))) return true;
            if (Directory.Exists(Path.Combine(_outputDirectory, id.ToLower(), version))) return true;

            return false;
        }    

        void ResolveDependencies(IPackage package)
        {
            foreach (var set in package.DependencySets)
            {
                foreach (var dependency in set.Dependencies)
                {
                    if (PackageExists(dependency.Id, dependency.VersionSpec.ToString())) continue;

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

        string GetSource()
        {
            if (_source == null)
            {
                return DefaultSource;
            }
            else
            {
                return _source;
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
                    _repository = PackageRepositoryFactory.Default.CreateRepository(GetSource());
                }
                return _repository;
            }
        }
    }
}
