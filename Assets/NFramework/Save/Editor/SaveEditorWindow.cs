using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Editor
{
    public class SaveEditorWindow : OdinEditorWindow
    {
        [MenuItem("Tools/NFramework/Save Editor")]
        private static void OpenWindow()
        {
            GetWindow<SaveEditorWindow>("Save Editor").Show();
        }

        [TabGroup("Tabs", "Runtime", SdfIconType.PlayFill)]
        [ShowIf("IsPlaying")]
        [ShowInInspector]
        [ReadOnly]
        [TableList(AlwaysExpanded = true, IsReadOnly = true)]
        private List<SaveableRuntimeInfo> RuntimeEntries
        {
            get
            {
                List<SaveableRuntimeInfo> list = new List<SaveableRuntimeInfo>();
                if (!ServiceLocator.TryGet<ISaveService>(out ISaveService saveService))
                {
                    return list;
                }

                IReadOnlyList<ISaveable> saveables = saveService.RegisteredSaveables;
                if (saveables == null)
                {
                    return list;
                }

                for (int i = 0; i < saveables.Count; i++)
                {
                    ISaveable s = saveables[i];
                    list.Add(new SaveableRuntimeInfo
                    {
                        Key = s.SaveKey,
                        SchemaVersion = s.SchemaVersion,
                        IsDirty = s.IsDirty,
                        Type = s.GetType().Name
                    });
                }

                return list;
            }
        }

        [TabGroup("Tabs", "Runtime")]
        [HideIf("IsPlaying")]
        [InfoBox("Enter Play Mode to view registered ISaveable components and runtime save state.", InfoMessageType.Info)]
        [ShowInInspector]
        private string PlayModePrompt => "Play Mode inactive.";

        [TabGroup("Tabs", "Runtime")]
        [ShowIf("IsPlaying")]
        [ButtonGroup("Tabs/Runtime/Actions")]
        [Button(ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
        private void TriggerSave()
        {
            if (ServiceLocator.TryGet<ISaveService>(out ISaveService saveService))
            {
                saveService.Save();
                Debug.Log("[SaveEditorWindow] Save triggered manually.");
            }
        }

        [TabGroup("Tabs", "Runtime")]
        [ShowIf("IsPlaying")]
        [ButtonGroup("Tabs/Runtime/Actions")]
        [Button(ButtonSizes.Large), GUIColor(0.3f, 0.6f, 1.0f)]
        private void TriggerLoad()
        {
            if (ServiceLocator.TryGet<ISaveService>(out ISaveService saveService))
            {
                saveService.Load();
                Debug.Log("[SaveEditorWindow] Load triggered manually.");
            }
        }

        [TabGroup("Tabs", "File Management", SdfIconType.FolderFill)]
        [ShowInInspector]
        [ReadOnly]
        private string SaveFolder => GetSaveDirectory();

        [TabGroup("Tabs", "File Management")]
        [BoxGroup("Tabs/File Management/Backup Management")]
        [ShowInInspector]
        [LabelText("Backup Name")]
        private string _backupFileName;

        [TabGroup("Tabs", "File Management")]
        [BoxGroup("Tabs/File Management/Backup Management")]
        [Button("Capture Backup", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 0.4f)]
        private void CaptureBackup()
        {
            if (string.IsNullOrWhiteSpace(_backupFileName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a backup file name.", "OK");
                return;
            }

            string saveDir = GetSaveDirectory();
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            if (Application.isPlaying && ServiceLocator.TryGet<ISaveService>(out ISaveService saveService))
            {
                saveService.Save();
            }

            string mainSavePath = Path.Combine(saveDir, MAIN_SAVE_FILE_NAME);
            if (!File.Exists(mainSavePath))
            {
                EditorUtility.DisplayDialog("Error", $"Main save file '{MAIN_SAVE_FILE_NAME}' does not exist.", "OK");
                return;
            }

            string targetFileName = _backupFileName.Trim();
            if (!targetFileName.EndsWith(SAVE_EXTENSION, StringComparison.OrdinalIgnoreCase))
            {
                targetFileName += SAVE_EXTENSION;
            }

            if (string.Equals(targetFileName, MAIN_SAVE_FILE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Error", $"Backup file cannot have the same name as '{MAIN_SAVE_FILE_NAME}'.", "OK");
                return;
            }

            string targetPath = Path.Combine(saveDir, targetFileName);
            if (File.Exists(targetPath))
            {
                if (!EditorUtility.DisplayDialog("Confirm Overwrite", $"Backup file '{targetFileName}' already exists. Do you want to overwrite it?", "Overwrite", "Cancel"))
                {
                    return;
                }
            }

            File.Copy(mainSavePath, targetPath, true);
            _selectedBackupFile = targetFileName;
            Debug.Log($"[SaveEditorWindow] Backup '{targetFileName}' captured successfully.");
        }

        [TabGroup("Tabs", "File Management")]
        [BoxGroup("Tabs/File Management/Backup Management")]
        [ValueDropdown(nameof(GetBackupFiles))]
        [ShowInInspector]
        [LabelText("Select Backup")]
        private string _selectedBackupFile;

        [TabGroup("Tabs", "File Management")]
        [BoxGroup("Tabs/File Management/Backup Management")]
        [HorizontalGroup("Tabs/File Management/Backup Management/Actions")]
        [Button("Load Backup", ButtonSizes.Medium), GUIColor(0.3f, 0.6f, 1.0f)]
        [DisableIf(nameof(IsPlaying))]
        [EnableIf(nameof(HasSelectedBackup))]
        private void LoadBackup()
        {
            if (IsPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Cannot load backup while in Play Mode.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_selectedBackupFile))
            {
                EditorUtility.DisplayDialog("Error", "Please select a backup file to load.", "OK");
                return;
            }

            string saveDir = GetSaveDirectory();
            string backupPath = Path.Combine(saveDir, _selectedBackupFile);
            if (!File.Exists(backupPath))
            {
                EditorUtility.DisplayDialog("Error", $"Backup file '{_selectedBackupFile}' does not exist.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm Load Backup", $"Are you sure you want to load backup '{_selectedBackupFile}'? This will overwrite '{MAIN_SAVE_FILE_NAME}'.", "Load", "Cancel"))
            {
                return;
            }

            string mainSavePath = Path.Combine(saveDir, MAIN_SAVE_FILE_NAME);
            File.Copy(backupPath, mainSavePath, true);

            string systemBackupPath = mainSavePath + BACKUP_EXTENSION;
            if (File.Exists(systemBackupPath))
            {
                File.Delete(systemBackupPath);
            }

            Debug.Log($"[SaveEditorWindow] Backup '{_selectedBackupFile}' loaded successfully into '{MAIN_SAVE_FILE_NAME}'.");
        }

        [TabGroup("Tabs", "File Management")]
        [BoxGroup("Tabs/File Management/Backup Management")]
        [HorizontalGroup("Tabs/File Management/Backup Management/Actions")]
        [Button("Delete Backup", ButtonSizes.Medium), GUIColor(0.9f, 0.3f, 0.3f)]
        [EnableIf(nameof(HasSelectedBackup))]
        private void DeleteBackup()
        {
            if (string.IsNullOrEmpty(_selectedBackupFile))
            {
                EditorUtility.DisplayDialog("Error", "Please select a backup file to delete.", "OK");
                return;
            }

            string saveDir = GetSaveDirectory();
            string backupPath = Path.Combine(saveDir, _selectedBackupFile);
            if (!File.Exists(backupPath))
            {
                EditorUtility.DisplayDialog("Error", $"Backup file '{_selectedBackupFile}' does not exist.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm Delete Backup", $"Are you sure you want to delete backup '{_selectedBackupFile}'?", "Delete", "Cancel"))
            {
                return;
            }

            File.Delete(backupPath);
            Debug.Log($"[SaveEditorWindow] Backup '{_selectedBackupFile}' deleted.");
            _selectedBackupFile = null;
        }

        [TabGroup("Tabs", "File Management")]
        [ShowInInspector]
        [ReadOnly]
        [TableList(AlwaysExpanded = true, IsReadOnly = true)]
        private List<SaveFileInfo> SavedFiles
        {
            get
            {
                List<SaveFileInfo> list = new List<SaveFileInfo>();
                string dir = GetSaveDirectory();
                if (!Directory.Exists(dir))
                {
                    return list;
                }

                string[] files = Directory.GetFiles(dir);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);
                    list.Add(new SaveFileInfo
                    {
                        Name = fi.Name,
                        SizeBytes = fi.Length,
                        LastModified = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                return list;
            }
        }

        [TabGroup("Tabs", "File Management")]
        [ButtonGroup("Tabs/File Management/GlobalActions")]
        [Button(ButtonSizes.Large), GUIColor(0.2f, 0.7f, 0.9f)]
        private void OpenSaveFolder()
        {
            string dir = GetSaveDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            EditorUtility.RevealInFinder(dir);
        }

        [TabGroup("Tabs", "File Management")]
        [ButtonGroup("Tabs/File Management/GlobalActions")]
        [Button(ButtonSizes.Large), GUIColor(0.9f, 0.2f, 0.2f)]
        private void DeleteAllSaveFiles()
        {
            if (!EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete all save files?", "Delete", "Cancel"))
            {
                return;
            }

            if (ServiceLocator.TryGet<ISaveService>(out ISaveService saveService))
            {
                saveService.DeleteSave();
            }
            else
            {
                string dir = GetSaveDirectory();
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    Directory.CreateDirectory(dir);
                }
            }

            Debug.Log("[SaveEditorWindow] Save files deleted.");
        }

        private const string MAIN_SAVE_FILE_NAME = "gamedata.sav";
        private const string SAVE_EXTENSION = ".sav";
        private const string BACKUP_EXTENSION = ".backup";

        private bool IsPlaying => Application.isPlaying;
        private bool HasSelectedBackup => !string.IsNullOrEmpty(_selectedBackupFile);

        private static string GetSaveDirectory()
        {
            return PathHelper.GetSaveFolderPath(false);
        }

        private IEnumerable<string> GetBackupFiles()
        {
            string dir = GetSaveDirectory();
            if (!Directory.Exists(dir))
            {
                yield break;
            }

            string[] files = Directory.GetFiles(dir, "*" + SAVE_EXTENSION);
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileName(files[i]);
                if (string.Equals(fileName, MAIN_SAVE_FILE_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return fileName;
            }
        }

        [Serializable]
        private struct SaveableRuntimeInfo
        {
            public string Key;
            public int SchemaVersion;
            public bool IsDirty;
            public string Type;
        }

        [Serializable]
        private struct SaveFileInfo
        {
            public string Name;
            public long SizeBytes;
            public string LastModified;
        }
    }
}
