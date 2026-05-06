using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    public static partial class StaticUtils
    {
        #region get paths

        private static string GetDataPath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.dataPath;
#else
		return Application.persistentDataPath;
#endif
        }

        public static string GetAbsolutePath(string path, bool isAbsolutePath)
        {
            if (isAbsolutePath)
            {
                return path;
            }
            else
            {
                return $"{GetDataPath()}/{path}";
            }
        }

        public static string GetProjectPath()
        {
            return Path.GetDirectoryName(Application.dataPath);
        }

        public static string GetStreamingFullPath(string path)
        {
            return $"{Application.streamingAssetsPath}/{path}";
        }

        #endregion

        #region read from resource folder

        public static string GetResourceFileText(string path)
        {
            return Resources.Load<TextAsset>(path).text;
        }

        #endregion

        #region open for read

        public static void OpenFileForRead(string path, UnityAction<Stream> callback, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            callback?.Invoke(stream);
            stream.Close();
        }

        public static void OpenFileForRead(string path, UnityAction<StreamReader> callback, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            var reader = new StreamReader(path);
            callback?.Invoke(reader);
            reader.Close();
        }

        public static void OpenFileForRead(string path, UnityAction<BinaryReader> callback, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fileStream);
            callback?.Invoke(reader);
            reader.Close();
            fileStream.Close();
        }

        #endregion

        #region open for write

        public static void OpenFileForWrite(string path, UnityAction<Stream> callback, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);

            CreateFolder(Path.GetDirectoryName(path), isAbsolutePath: true);

            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            callback?.Invoke(fileStream);
            fileStream.Close();
        }

        public static void OpenFileForWrite(string path, UnityAction<StreamWriter> callback, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);

            CreateFolder(Path.GetDirectoryName(path), isAbsolutePath: true);

            var writer = new StreamWriter(path, append: false);
            callback?.Invoke(writer);
            writer.Close();
        }

        public static void OpenFileForWrite(string path, UnityAction<BinaryWriter> callback, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);

            CreateFolder(Path.GetDirectoryName(path), isAbsolutePath: true);

            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new BinaryWriter(fileStream);
            callback?.Invoke(writer);
            writer.Close();
            fileStream.Close();
        }

        #endregion

        #region read file

        public static string ReadTextFile(string path, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            var reader = new StreamReader(path);
            var fileContent = reader.ReadToEnd();
            reader.Close();
            return fileContent;
        }

        public static byte[] ReadBinaryFile(string path, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();
            return buffer;
        }

        public static void ReadTextFileIntoLines(string path, UnityAction<string, int> lineCallback, bool isAbsolutePath = false)
        {
            OpenFileForRead(path, streamReader =>
            {
                var lineNumber = 0;
                while (!streamReader.EndOfStream)
                {
                    lineNumber++;
                    var line = streamReader.ReadLine();
                    lineCallback?.Invoke(line, lineNumber);
                }
            }, isAbsolutePath);
        }

        #endregion

        #region write file

        public static void WriteTextFile(string path, string fileContent, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);

            CreateFolder(Path.GetDirectoryName(path), isAbsolutePath: true);

            var writer = new StreamWriter(path, append: false);
            writer.Write(fileContent);
            writer.Close();
        }

        public static void WriteBinaryFile(string path, byte[] data, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);

            CreateFolder(Path.GetDirectoryName(path), isAbsolutePath: true);

            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
        }

        public static void WriteStreamToFile(string path, Stream inputStream, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);

            CreateFolder(Path.GetDirectoryName(path), isAbsolutePath: true);

            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            if (inputStream.CanSeek)
            {
                inputStream.Seek(0, SeekOrigin.Begin);
            }
            inputStream.CopyTo(fileStream);
            fileStream.Close();
        }

        #endregion

        #region list

        public static List<string> GetFilesInFolder(string path, bool subFolderToo, string filterFilename,
            string filterExtension, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);

            var pattern = !string.IsNullOrEmpty(filterFilename) ? filterFilename : $"*.{filterExtension}";
            var option = subFolderToo ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var result = Directory.GetFiles(path, pattern, option);
            return new List<string>(result);
        }

        #endregion

        #region manipulate file

        public static bool CheckFileExist(string path, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            return File.Exists(path);
        }

        public static void CopyFile(string sourceFile, string targetFolder, bool isAbsolutePath = false)
        {
            sourceFile = GetAbsolutePath(sourceFile, isAbsolutePath);
            targetFolder = GetAbsolutePath(targetFolder, isAbsolutePath);

            var filename = Path.GetFileName(sourceFile);
            var targetFile = $"{targetFolder}/{filename}";

            if (!CheckFolderExist(targetFolder, isAbsolutePath: true))
            {
                CreateFolder(targetFolder, isAbsolutePath: true);
            }
            File.Copy(sourceFile, targetFile, overwrite: true);
        }

        public static void RenameFile(string oldPath, string newName, bool isAbsolutePath = false)
        {
            oldPath = GetAbsolutePath(oldPath, isAbsolutePath);
            var newPath = $"{Path.GetDirectoryName(oldPath)}/{newName}";
            File.Move(oldPath, newPath);
        }

        public static void DeleteFile(string path, bool isAbsolutePath = false)
        {
            if (CheckFileExist(path, isAbsolutePath))
            {
                path = GetAbsolutePath(path, isAbsolutePath);
                File.Delete(path);
            }
        }

        #endregion

        #region manipulate folder

        public static bool CheckFolderExist(string path, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            return Directory.Exists(path);
        }

        public static void CreateFolder(string path, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            if (!CheckFolderExist(path, isAbsolutePath: true))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteFolder(string path, bool isAbsolutePath = false)
        {
            path = GetAbsolutePath(path, isAbsolutePath);
            if (CheckFolderExist(path, isAbsolutePath: true))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        public static void CopyFolder(string sourcePath, string targetPath, bool isAbsolutePath = false)
        {
            sourcePath = GetAbsolutePath(sourcePath, isAbsolutePath);
            targetPath = GetAbsolutePath(targetPath, isAbsolutePath);

            if (!CheckFolderExist(targetPath, isAbsolutePath: true))
            {
                CreateFolder(targetPath, isAbsolutePath: true);
            }

            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        #endregion
    }
}