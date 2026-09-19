using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace NFramework
{
    public class NetworkChecker : SingletonMono<NetworkChecker>
    {
        private enum EAnchor { LeftTop, LeftBottom, RightTop, RightBottom }
        
        [Header("Settings")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _isSimple = true;
        [ShowIf(nameof(_isSimple)), SerializeField] private List<string> _targetIPs = new() { "8.8.8.8", "1.1.1.1" };
        [HideIf(nameof(_isSimple)), SerializeField] private List<string> _targetUrls = new() { "https://www.google.com/", "https://www.cloudflare.com/" };
        [SerializeField] private int _normalCheckIntervalMS = 10000;
        [SerializeField] private int _failCheckIntervalMS = 5000;
        [SerializeField] private int _pingTimeoutMS = 2500;
        
        [Header("GUI")]
        [SerializeField] private bool _showPingGUI;
        [ShowIf(nameof(_showPingGUI)), SerializeField] private bool _showPingGUIEditorOnly;
        [DisableInPlayMode, SerializeField] private EAnchor _anchor;
        [DisableInPlayMode, SerializeField] private int _xOffset;
        [DisableInPlayMode, SerializeField] private int _yOffset;

        [Header("Debug")]
        [ShowInInspector, ReadOnly] private int _lastPingTimeMS;
        [ShowIf(nameof(_isSimple)), ShowInInspector, ReadOnly] private string _lastSuccessfulIP;
        [HideIf(nameof(_isSimple)), ShowInInspector, ReadOnly] private string _lastSuccessfulUrl;
        
        private bool _isPingSuccess;
        private readonly Stopwatch _stopwatch = new();
        private Rect _rect;
        private GUIStyle _style;
        private readonly Color _goodColor = ColorHelper.Lime;
        private readonly Color _okColor = ColorHelper.Yellow;
        private readonly Color _badColor = ColorHelper.Red;
        
        public bool IsInitialized { get; private set; }
        public bool IsNetworkAvailable => Application.internetReachability != NetworkReachability.NotReachable && _isPingSuccess;

        protected override void Awake()
        {
            base.Awake();
            if (_initializeOnAwake) Initialize().Forget();
            
            if (_showPingGUI)
            {
                _style = new GUIStyle
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = Screen.height / 50
                };
                
                var xPos = 0;
                var yPos = 0;
                var linesHeight = 40;
                var linesWidth = 170;
                if (_anchor == EAnchor.LeftBottom || _anchor == EAnchor.RightBottom) yPos = Screen.height - linesHeight;
                if (_anchor == EAnchor.RightTop || _anchor == EAnchor.RightBottom) xPos = Screen.width - linesWidth;
                xPos += _xOffset;
                yPos += _yOffset;
                _rect = new Rect(xPos, yPos, linesWidth, linesHeight);
            }
        }

        public async UniTask Initialize()
        {
            if (IsInitialized) return;
            
            IsInitialized = true;
            await Ping();
            StartPingContinuously().Forget();
        }

        private async UniTask StartPingContinuously()
        {
            while (true)
            {
                await UniTask.Delay(_isPingSuccess ? _normalCheckIntervalMS : _failCheckIntervalMS, true, cancellationToken: destroyCancellationToken);
                await Ping().AttachExternalCancellation(destroyCancellationToken);
            }
        }

        private async UniTask Ping()
        {
            _isPingSuccess = false;
            _lastPingTimeMS = -1;

            if (Application.internetReachability == NetworkReachability.NotReachable)
                return;

            try
            {
                if (_isSimple)
                {
                    var ipList = Prioritize(_targetIPs, _lastSuccessfulIP);
                    foreach (var ip in ipList)
                    {
                        if (await PingIP(ip))
                        {
                            _isPingSuccess = true;
                            _lastSuccessfulIP = ip;
                            break;
                        }
                    }
                }
                else
                {
                    var urlList = Prioritize(_targetUrls, _lastSuccessfulUrl);
                    foreach (var url in urlList)
                    {
                        if (await PingURL(url))
                        {
                            _isPingSuccess = true;
                            _lastSuccessfulUrl = url;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async UniTask<bool> PingIP(string ip)
        {
            var ping = new Ping(ip);
            float timer = 0f;

            while (!ping.isDone && timer < _pingTimeoutMS / 1000f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
                timer += Time.deltaTime;
            }

            if (ping.isDone && ping.time >= 0)
            {
                _lastPingTimeMS = ping.time;
                return true;
            }

            return false;
        }

        private async UniTask<bool> PingURL(string url)
        {
            _stopwatch.Restart();
            using var request = UnityWebRequest.Head(url);
            request.timeout = _pingTimeoutMS / 1000;

            await request.SendWebRequest().WithCancellation(destroyCancellationToken);
            _stopwatch.Stop();

            if (request.result == UnityWebRequest.Result.Success)
            {
                _lastPingTimeMS = (int)_stopwatch.ElapsedMilliseconds;
                return true;
            }

            return false;
        }

        private List<string> Prioritize(List<string> originalList, string preferred)
        {
            if (string.IsNullOrEmpty(preferred) || !originalList.Contains(preferred))
                return originalList;

            return new List<string> { preferred }
                .Concat(originalList.Where(x => x != preferred))
                .ToList();
        }
        
#if DEVELOPMENT
        private void OnGUI()
        {
            if (!_showPingGUI || (_showPingGUIEditorOnly && !Application.isEditor)) return;

            var color = _lastPingTimeMS switch
            {
                < 20 => _goodColor,
                <= 50 => _okColor,
                _ => _badColor
            };
            
            _style.normal.textColor = color;
            GUI.Label(_rect, "Ping: " + _lastPingTimeMS, _style);
            _style.normal.textColor = GUI.color;
        }
#endif
    }
}