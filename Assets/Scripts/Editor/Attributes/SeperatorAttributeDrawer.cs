using UnityEditor;
using UnityEngine;

namespace NFramework.Editors
{
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    public class SeparatorAttributeDrawer : DecoratorDrawer
    {
        private SeparatorAttribute Separator => (SeparatorAttribute)attribute;

        public override float GetHeight() => Separator.withOffset ? 40 : string.IsNullOrEmpty(Separator.title) ? 28 : 32;

        public override void OnGUI(Rect position)
        {
            var title = Separator.title;
            if (string.IsNullOrEmpty(Separator.title))
            {
                position.height = 1;
                position.y += 14;
                GUI.Box(position, string.Empty);
            }
            else
            {
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(title));
                float separatorWidth = (position.width - textSize.x) / 2 - 5;
                position.y += 19;

                GUI.Box(new Rect(position.xMin, position.yMin, separatorWidth, 1), string.Empty);

                var style = EditorStyles.boldLabel;
                if (ColorHelper.TryGetColorAt(Separator.colorIndex, out UnityEngine.Color color))
                    style.normal.textColor = color;

                GUI.Label(new Rect(position.xMin + separatorWidth + 5, position.yMin - 10, textSize.x, 20), title, style);

                GUI.Box(new Rect(position.xMin + separatorWidth + 10 + textSize.x, position.yMin, separatorWidth, 1), string.Empty);
            }
        }
    }
}
