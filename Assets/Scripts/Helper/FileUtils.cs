using System.IO;
using System.Text.RegularExpressions;

namespace NFramework
{
    public static class FileUtils
    {
        public static bool IsValidFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists && fileInfo.Length > 0;
        }

        public static string GetValidFileName(string fileName)
        {
            // remove any invalid character from the filename.
            string ret = Regex.Replace(fileName.Trim(), "[^A-Za-z0-9_. ]+", "");
            return ret.Replace(" ", string.Empty);
        }

        /// <summary>
        /// Deletes the specified directory
        /// </summary>
        public static void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        public static long GetDirectorySize(string target_dir)
        {
            if (!Directory.Exists(target_dir))
                return 0;

            long totalSize = 0;
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }

            foreach (string dir in dirs)
            {
                totalSize += GetDirectorySize(dir);
            }

            return totalSize;
        }
    }
}
