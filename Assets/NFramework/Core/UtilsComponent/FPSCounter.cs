using UnityEngine;

namespace NFramework
{
    public class FPSCounter : MonoBehaviour
    {
        private enum Anchor { LeftTop, LeftBottom, RightTop, RightBottom }

        [SerializeField] private bool _editorOnly;
        
        [Space]
        [SerializeField] private float _updateInterval = 1f;
        [SerializeField] private int _targetFrameRate = 60;

        [Space]
        [SerializeField] private Anchor _anchor;
        [SerializeField] private int _xOffset;
        [SerializeField] private int _yOffset;

        /// <summary>
        /// Skip some time at start to skip performance drop on game start
        /// and produce more accurate Avg FPS
        /// </summary>
        private float _idleTime = 2f;
        private float _elapsed;
        private int _frames;
        private float _fps;
        private float _okFps;
        private float _badFps;
        private Rect _rect;
        private GUIStyle _style;
        private readonly Color _goodColor = ColorHelper.Lime;
        private readonly Color _okColor = ColorHelper.Yellow;
        private readonly Color _badColor = ColorHelper.Red;

        private void Awake()
        {
            if (_editorOnly && !Application.isEditor)
            {
                Destroy(this);
                return;
            }

            _style = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = Screen.height / 50
            };

            var percent = _targetFrameRate / 100f;
            var percent10 = percent * 10;
            var percent40 = percent * 40;
            _okFps = _targetFrameRate - percent10;
            _badFps = _targetFrameRate - percent40;

            var xPos = 0;
            var yPos = 0;
            var linesHeight = 40;
            var linesWidth = 170;
            if (_anchor == Anchor.LeftBottom || _anchor == Anchor.RightBottom) yPos = Screen.height - linesHeight;
            if (_anchor == Anchor.RightTop || _anchor == Anchor.RightBottom) xPos = Screen.width - linesWidth;
            xPos += _xOffset;
            yPos += _yOffset;
            _rect = new Rect(xPos, yPos, linesWidth, linesHeight);

            _elapsed = _updateInterval;
        }

        private void Update()
        {
            if (_editorOnly && !Application.isEditor) return;

            if (_idleTime > 0)
            {
                _idleTime -= Time.deltaTime;
                return;
            }

            _elapsed += Time.deltaTime;
            ++_frames;

            if (_elapsed >= _updateInterval)
            {
                _fps = Mathf.Round(_frames / _elapsed);
                _elapsed = 0;
                _frames = 0;
            }
        }
        
#if DEVELOPMENT
        private void OnGUI()
        {
            if (_editorOnly && !Application.isEditor) return;

            var color = _goodColor;
            if (_fps <= _okFps) color = _okColor;
            if (_fps <= _badFps) color = _badColor;
            _style.normal.textColor = color;
            GUI.Label(_rect, "FPS: " + (int)_fps, _style);
            _style.normal.textColor = GUI.color;
        }
#endif
    }
}