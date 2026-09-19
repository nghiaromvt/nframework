using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public class TsvManagerEditorWindow : OdinEditorWindow
    {
        public enum FetchStatus
        {
            Idle,
            Fetching,
            Success,
            Failed
        }

        public enum LogLevel
        {
            Info,
            Success,
            Error
        }

        [Serializable]
        public struct FetchLogEntry
        {
            public DateTime Timestamp;
            public LogLevel Level;
            public string Message;
        }

        [Serializable]
        public class TsvTableEntry
        {
            [TableColumnWidth(30, Resizable = false)]
            public bool Selected = true;

            [ShowInInspector, LabelText("Status"), TableColumnWidth(45, Resizable = false)]
            [CustomValueDrawer(nameof(DrawStatusIcon))]
            public FetchStatus Status = FetchStatus.Idle;

            [ReadOnly, TableColumnWidth(160)]
            public string Name;

            [ReadOnly, TableColumnWidth(100)]
            public string Category;

            [TableColumnWidth(300)]
            [OnValueChanged(nameof(OnPathChanged))]
            public string Path;

            [ReadOnly, TableColumnWidth(140)]
            public string LastSynced;

            [HideInInspector]
            public TsvConfigSOBase Target;

            [HideInInspector]
            public string ErrorMessage;

            [HideInInspector]
            public TsvManagerEditorWindow Window;

            private bool IsDisabled => Window != null && Window.IsFetching;

            [Button("Ping"), TableColumnWidth(50, Resizable = false)]
            public void Ping()
            {
                if (Target != null)
                {
                    Selection.activeObject = Target;
                    EditorGUIUtility.PingObject(Target);
                }
            }

            [Button("Fetch"), TableColumnWidth(55, Resizable = false), DisableIf(nameof(IsDisabled))]
            public async void Fetch()
            {
                if (Target == null)
                {
                    return;
                }

                Status = FetchStatus.Fetching;
                ErrorMessage = null;
                Window?.Repaint();
                Window?.AddLog(LogLevel.Info, $"Fetching {Name}...");

                try
                {
                    await Target.FetchAsync();
                    LastSynced = Target.LastSyncedTime;
                    Status = FetchStatus.Success;
                    Window?.AddLog(LogLevel.Success, $"Successfully fetched {Name}.");
                }
                catch (Exception ex)
                {
                    Status = FetchStatus.Failed;
                    ErrorMessage = ex.Message;
                    Window?.AddLog(LogLevel.Error, $"Failed to fetch {Name}: {ex.Message}");
                    NLogger.LogError($"[TsvManager] Failed to fetch {Target.name}: {ex.Message}");
                }
                finally
                {
                    Window?.Repaint();
                }
            }

            private FetchStatus DrawStatusIcon(FetchStatus status, GUIContent label)
            {
                var rect = EditorGUILayout.GetControlRect(false, 18, GUILayout.Width(24));
                var iconRect = new Rect(rect.x + (rect.width - 16) * 0.5f, rect.y + (rect.height - 16) * 0.5f, 16, 16);

                switch (status)
                {
                    case FetchStatus.Idle:
                        SdfIcons.DrawIcon(iconRect, SdfIconType.DashCircle, new Color(0.6f, 0.6f, 0.6f));
                        GUI.Label(iconRect, new GUIContent("", "Idle - Not fetched yet"));
                        break;
                    case FetchStatus.Fetching:
                        SdfIcons.DrawIcon(iconRect, SdfIconType.ArrowRepeat, new Color(0.25f, 0.65f, 1f));
                        GUI.Label(iconRect, new GUIContent("", "Fetching in progress..."));
                        break;
                    case FetchStatus.Success:
                        SdfIcons.DrawIcon(iconRect, SdfIconType.CheckCircleFill, new Color(0.3f, 0.85f, 0.35f));
                        GUI.Label(iconRect, new GUIContent("", "Fetched successfully"));
                        break;
                    case FetchStatus.Failed:
                        SdfIcons.DrawIcon(iconRect, SdfIconType.XCircleFill, new Color(0.95f, 0.3f, 0.3f));
                        GUI.Label(iconRect, new GUIContent("", string.IsNullOrWhiteSpace(ErrorMessage) ? "Fetch failed" : $"Fetch failed: {ErrorMessage}"));
                        break;
                }

                return status;
            }

            private void OnPathChanged()
            {
                if (Target != null && Target.TsvPath != Path)
                {
                    Target.TsvPath = Path;
                    EditorUtility.SetDirty(Target);
                }
            }
        }

        private const string TAB_ALL = "All";
        private const string TAB_DEFAULT = "Default / Uncategorized";

        [SerializeField, HideInInspector]
        private List<TsvTableEntry> _allEntries = new();

        [ShowInInspector, LabelText("Search"), PropertyOrder(-8)]
        [OnValueChanged(nameof(ApplyFilter))]
        private string _searchFilter = "";

        private string[] _categories = { TAB_ALL };
        private int _selectedCategoryIndex = 0;

        [ShowInInspector, TableList(ShowPaging = false, IsReadOnly = false, AlwaysExpanded = true), PropertyOrder(1)]
        private List<TsvTableEntry> _filteredEntries = new();

        [SerializeField, HideInInspector]
        private List<FetchLogEntry> _logs = new();

        private Vector2 _logScrollPosition;
        private bool _autoScrollLogs = true;
        private bool _isFetching = false;
        private float _progress = 0f;
        private string _progressLabel = "";
        private CancellationTokenSource _cts;

        private GUIStyle _infoStyle;
        private GUIStyle _successStyle;
        private GUIStyle _errorStyle;

        public bool IsFetching => _isFetching;

        [MenuItem("Tools/NFramework/TSV Table Manager")]
        private static void OpenWindow()
        {
            var window = GetWindow<TsvManagerEditorWindow>("TSV Table Manager");
            window.Refresh();
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        [OnInspectorGUI, PropertyOrder(-10)]
        private void DrawCategoryToolbar()
        {
            if (_categories == null || _categories.Length == 0)
            {
                UpdateCategoryTabs();
            }

            EditorGUILayout.Space(4);
            int newIndex = GUILayout.Toolbar(_selectedCategoryIndex, _categories, EditorStyles.toolbarButton);
            if (newIndex != _selectedCategoryIndex)
            {
                _selectedCategoryIndex = newIndex;
                ApplyFilter();
            }
            EditorGUILayout.Space(4);
        }

        [ButtonGroup("Actions", -5), Button("Select All")]
        public void SelectAll()
        {
            foreach (var entry in _filteredEntries)
            {
                entry.Selected = true;
            }
        }

        [ButtonGroup("Actions", -5), Button("Deselect All")]
        public void DeselectAll()
        {
            foreach (var entry in _filteredEntries)
            {
                entry.Selected = false;
            }
        }

        [ButtonGroup("Actions", -5), Button("Refresh Assets")]
        public void Refresh()
        {
            var assets = FileHelper.LoadAssetsWithType<TsvConfigSOBase>();
            _allEntries.Clear();

            foreach (var asset in assets)
            {
                _allEntries.Add(new TsvTableEntry
                {
                    Selected = true,
                    Name = asset.name,
                    Category = string.IsNullOrWhiteSpace(asset.Category) ? "Default" : asset.Category,
                    Path = asset.TsvPath,
                    LastSynced = asset.LastSyncedTime,
                    Target = asset,
                    Window = this,
                    Status = FetchStatus.Idle
                });
            }

            UpdateCategoryTabs();
            ApplyFilter();
        }

        [OnInspectorGUI, PropertyOrder(4)]
        private void DrawProgressSection()
        {
            if (_isFetching || !string.IsNullOrEmpty(_progressLabel))
            {
                EditorGUILayout.Space(6);
                var rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, _progress, _progressLabel);
                EditorGUILayout.Space(4);
            }
        }

        [Button("Fetch Selected", ButtonSizes.Large), GUIColor(0.2f, 0.8f, 0.4f), PropertyOrder(5)]
        [DisableIf(nameof(_isFetching))]
        public async void FetchSelected()
        {
            var selected = _filteredEntries.Where(e => e.Selected && e.Target != null).ToList();
            if (selected.Count == 0)
            {
                AddLog(LogLevel.Info, "No tables selected to fetch.");
                return;
            }

            _isFetching = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            int successCount = 0;
            int failCount = 0;

            AddLog(LogLevel.Info, $"Starting batch fetch for {selected.Count} selected tables...");

            for (int i = 0; i < selected.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    AddLog(LogLevel.Info, "Batch fetch cancelled by user.");
                    break;
                }

                var entry = selected[i];
                entry.Status = FetchStatus.Fetching;
                entry.ErrorMessage = null;
                _progress = (float)i / selected.Count;
                _progressLabel = $"Fetching {entry.Name} ({i + 1}/{selected.Count})...";
                Repaint();

                try
                {
                    await entry.Target.FetchAsync();
                    entry.LastSynced = entry.Target.LastSyncedTime;
                    entry.Status = FetchStatus.Success;
                    successCount++;
                    AddLog(LogLevel.Success, $"[{i + 1}/{selected.Count}] Successfully fetched {entry.Name}.");
                }
                catch (Exception ex)
                {
                    failCount++;
                    entry.Status = FetchStatus.Failed;
                    entry.ErrorMessage = ex.Message;
                    AddLog(LogLevel.Error, $"[{i + 1}/{selected.Count}] Failed to fetch {entry.Name}: {ex.Message}");
                    NLogger.LogError($"[TsvManager] Failed to fetch {entry.Name}: {ex.Message}");
                }

                _progress = (float)(i + 1) / selected.Count;
                Repaint();
            }

            _isFetching = false;
            _progress = 1f;
            _progressLabel = $"Completed: {successCount} succeeded, {failCount} failed.";
            AddLog(LogLevel.Info, $"Batch fetch finished. Succeeded: {successCount}, Failed: {failCount}.");

            _cts?.Dispose();
            _cts = null;
            Repaint();
        }

        [Button("Cancel Fetch", ButtonSizes.Large), GUIColor(0.9f, 0.3f, 0.3f), PropertyOrder(6)]
        [ShowIf(nameof(_isFetching))]
        public void CancelFetch()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                AddLog(LogLevel.Info, "Cancelling fetch...");
            }
        }

        [OnInspectorGUI, PropertyOrder(10)]
        private void DrawLogSection()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Fetch Logs ({_logs.Count})", EditorStyles.boldLabel);

            _autoScrollLogs = GUILayout.Toggle(_autoScrollLogs, "Auto-scroll", EditorStyles.miniButton, GUILayout.Width(75));

            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                _logs.Clear();
            }

            if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                var sb = new StringBuilder();
                foreach (var log in _logs)
                {
                    sb.AppendLine($"[{log.Timestamp:HH:mm:ss}] [{log.Level}] {log.Message}");
                }
                EditorGUIUtility.systemCopyBuffer = sb.ToString();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
            _logScrollPosition = EditorGUILayout.BeginScrollView(_logScrollPosition, GUILayout.Height(130));

            if (_logs.Count == 0)
            {
                EditorGUILayout.LabelField("No logs yet.", EditorStyles.miniLabel);
            }
            else
            {
                for (int i = 0; i < _logs.Count; i++)
                {
                    var log = _logs[i];
                    var style = GetLogStyle(log.Level);
                    EditorGUILayout.LabelField($"[{log.Timestamp:HH:mm:ss}] {log.Message}", style);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        public void AddLog(LogLevel level, string message)
        {
            _logs.Add(new FetchLogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            });

            if (_autoScrollLogs)
            {
                _logScrollPosition.y = float.MaxValue;
            }

            Repaint();
        }

        private GUIStyle GetLogStyle(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Success:
                    if (_successStyle == null)
                    {
                        _successStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            normal = { textColor = new Color(0.3f, 0.85f, 0.35f) },
                            wordWrap = true
                        };
                    }
                    return _successStyle;

                case LogLevel.Error:
                    if (_errorStyle == null)
                    {
                        _errorStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            normal = { textColor = new Color(0.95f, 0.35f, 0.35f) },
                            wordWrap = true
                        };
                    }
                    return _errorStyle;

                default:
                    if (_infoStyle == null)
                    {
                        _infoStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.2f, 0.2f, 0.2f) },
                            wordWrap = true
                        };
                    }
                    return _infoStyle;
            }
        }

        private void UpdateCategoryTabs()
        {
            var categorySet = new HashSet<string>();
            foreach (var entry in _allEntries)
            {
                var category = entry.Category;
                if (string.IsNullOrWhiteSpace(category) || category.Equals("Default", StringComparison.OrdinalIgnoreCase))
                {
                    categorySet.Add(TAB_DEFAULT);
                }
                else
                {
                    categorySet.Add(category);
                }
            }

            var list = new List<string> { TAB_ALL };
            list.AddRange(categorySet.OrderBy(c => c));
            _categories = list.ToArray();

            if (_selectedCategoryIndex >= _categories.Length)
            {
                _selectedCategoryIndex = 0;
            }
        }

        private void ApplyFilter()
        {
            string selectedCategory = _categories != null && _selectedCategoryIndex < _categories.Length
                ? _categories[_selectedCategoryIndex]
                : TAB_ALL;

            IEnumerable<TsvTableEntry> query = _allEntries;

            if (selectedCategory != TAB_ALL)
            {
                if (selectedCategory == TAB_DEFAULT)
                {
                    query = query.Where(e => string.IsNullOrWhiteSpace(e.Category) || e.Category.Equals("Default", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    query = query.Where(e => e.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(_searchFilter))
            {
                var lower = _searchFilter.ToLowerInvariant();
                query = query.Where(e =>
                    (e.Name != null && e.Name.ToLowerInvariant().Contains(lower)) ||
                    (e.Path != null && e.Path.ToLowerInvariant().Contains(lower)));
            }

            _filteredEntries = query.ToList();
        }
    }
}
