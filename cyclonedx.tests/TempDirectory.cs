using System;
using System.IO;

namespace CycloneDX.CLI.Tests
{
    class TempDirectory : IDisposable
    {
        private string tempPath;
        private string tempDirName;

        public TempDirectory()
        {
            tempPath = Path.GetTempPath();
            tempDirName = Path.GetRandomFileName();
            Directory.CreateDirectory(DirectoryPath);
        }

        public void Dispose()
        {
            Directory.Delete(DirectoryPath, true);
        }

        public string DirectoryPath
        {
            get => Path.Join(tempPath, tempDirName);
        }
    }
}
