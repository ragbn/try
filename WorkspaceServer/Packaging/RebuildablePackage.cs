﻿using System.IO;
using System.Linq;
using System.Reactive.Concurrency;

namespace WorkspaceServer.Packaging
{
    public class RebuildablePackage : Package
    {
        private FileSystemWatcher _fileSystemWatcher;

        public RebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) 
            : base(name, initializer, directory, buildThrottleScheduler)
        {

            _fileSystemWatcher = new FileSystemWatcher(Directory.FullName)
            {
                EnableRaisingEvents = true
            };

            _fileSystemWatcher.Changed += FileSystemWatcherOnChangedOrDeleted;
            _fileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;
            _fileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;
            _fileSystemWatcher.Created += FileSystemWatcherOnCreated;
        }

        private bool IsProjectFile(string fileName)
        {
            return fileName.EndsWith(".csproj");
        }

        private bool IsCodeFile(string fileName)
        {
            return fileName.EndsWith(".cs");
        }

        private bool IsBuildLogFile(string fileName)
        {
            return fileName.EndsWith(".binlog");
        }


        private void FileSystemWatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            var fileName = e.Name;
            if (DesignTimeBuildResult != null)
            {
                if (IsProjectFile(fileName) || IsBuildLogFile(fileName))
                {
                    Reset();
                }
                else if (IsCodeFile(fileName))
                {
                    var analyzerInputs = DesignTimeBuildResult.GetCompileInputs();
                    if (analyzerInputs.Any(sourceFile => sourceFile.EndsWith(fileName)))
                    {
                        Reset();
                    }
                }
            }
        }

        private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            var fileName = e.Name;
            if (IsProjectFile(fileName) || IsCodeFile(fileName))
            {
                Reset();
            }
        }

        private void FileSystemWatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            HandleFileChanges(e.OldName);
        }

        private void HandleFileChanges(string fileName)
        {
            if (DesignTimeBuildResult != null)
            {
                if (IsProjectFile(fileName))
                {
                    Reset();
                }
                else if (IsCodeFile(fileName))
                {
                    var analyzerInputs = DesignTimeBuildResult.GetCompileInputs();
                    if (analyzerInputs.Any(sourceFile => sourceFile.EndsWith(fileName)))
                    {
                        Reset();
                    }
                }
            }
        }

        private void Reset()
        {
            DesignTimeBuildResult = null;
            RoslynWorkspace = null;
        }

        private void FileSystemWatcherOnChangedOrDeleted(object sender, FileSystemEventArgs e)
        {
            HandleFileChanges(e.Name);
        }
    }
}
