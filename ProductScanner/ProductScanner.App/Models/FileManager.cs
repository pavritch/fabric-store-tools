using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{

    /// <summary>
    /// Concrete class for all disk/file based operations. Singleton.
    /// </summary>
    public class FileManager : IFileManager
    {
        public class FileStorageMetricsGeneratorInputs : IPooledTaskRunnerInputs<FileStorageMetrics>
        {
            public string RootFolder { get; set; }

            public FileStorageMetricsGeneratorInputs()
            {

            }

            public FileStorageMetricsGeneratorInputs(string rootFolder)
            {
                this.RootFolder = rootFolder;
            }

            /// <summary>
            /// Key used to uniquely identify the question to be answered.
            /// Should be the same for identical root folders, etc.
            /// </summary>
            public string Key
            {
                get
                {
                    return RootFolder.ToLower();
                }
            }

            /// <summary>
            /// Calculate the metrics for the specified folder tree.
            /// </summary>
            /// <remarks>
            /// Called on its own thread from the pool.
            /// </remarks>
            /// <returns></returns>
            public FileStorageMetrics GenerateAnswer()
            {
                if (!Directory.Exists(RootFolder)) return new FileStorageMetrics();

                var allFiles = Directory.GetFiles(RootFolder, "*.*", SearchOption.AllDirectories);
                if (allFiles.Length == 0) return new FileStorageMetrics();

                var total = allFiles.Sum(x => new FileInfo(x).Length);
                var newestFile = allFiles.Max(x => File.GetCreationTime(x));
                var oldestFile = allFiles.Min(x => File.GetCreationTime(x));

                return new FileStorageMetrics
                {
                    TotalFiles = allFiles.Count(),
                    TotalSize = total,
                    Oldest = oldestFile,
                    Newest = newestFile
                };
            }
        }

        private static PooledTaskRunner<FileStorageMetricsGeneratorInputs, FileStorageMetrics> pooledFolderMetricsGenerator;

        static FileManager()
        {
            pooledFolderMetricsGenerator = new PooledTaskRunner<FileStorageMetricsGeneratorInputs, FileStorageMetrics>();
        }


        public FileManager()
        {

        }

        public Task<FileStorageMetrics> GetFileStoreMetricsAsync(string rootFolder)
        {
            TaskCompletionSource<FileStorageMetrics> tcs = new TaskCompletionSource<FileStorageMetrics>();

            Task.Factory.StartNew(() =>
            {
                var input = new FileStorageMetricsGeneratorInputs(rootFolder);
                var answer = pooledFolderMetricsGenerator.GenerateResult(input);
                // thread will block here until answer is ready
                tcs.SetResult(answer);
            }, TaskCreationOptions.LongRunning);

            return tcs.Task;
        }
    }
}
