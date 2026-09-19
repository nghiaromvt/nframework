using System;
using UnityEngine;
using UnityEditor;

namespace vietlabs.fr2
{
    internal static class FR2_RectExtensions
    {
        internal static void SplitHorizontal(this Rect r, out Rect left, out Rect right, float space = 8f, float ratio = 0.5f)
        { 
            var w = (r.width - space) * ratio;
            left = new Rect(r.x, r.y, w, r.height);
            right = new Rect(r.x + w + space, r.y, r.width - w - space, r.height);
        }

        internal static (Rect left, Rect right) SplitHorizontal(this Rect r, float space = 8f, float ratio = 0.5f)
        {
            var w = (r.width - space) * ratio;
            var left = new Rect(r.x, r.y, w, r.height);
            var right = new Rect(r.x + w + space, r.y, r.width - w - space, r.height);
            return (left, right);
        }

        internal static void TakeLeft(this Rect r, out Rect extracted, out Rect remaining, float width = 16f, float space = 0f)
        {
            extracted = new Rect(r.x, r.y, width, r.height);
            remaining = new Rect(r.x + width + space, r.y, r.width - width - space, r.height);
        }

        internal static (Rect extracted, Rect remaining) TakeLeft(this Rect r, float width = 16f, float space = 0f)
        {
            var extracted = new Rect(r.x, r.y, width, r.height);
            var remaining = new Rect(r.x + width + space, r.y, r.width - width - space, r.height);
            return (extracted, remaining);
        }

        internal static void TakeRight(this Rect r, out Rect extracted, out Rect remaining, float width = 16f, float space = 0f)
        {
            extracted = new Rect(r.x + r.width - width, r.y, width, r.height);
            remaining = new Rect(r.x, r.y, r.width - width - space, r.height);
        }

        internal static (Rect extracted, Rect remaining) TakeRight(this Rect r, float width = 16f, float space = 0f)
        {
            var extracted = new Rect(r.x + r.width - width, r.y, width, r.height);
            var remaining = new Rect(r.x, r.y, r.width - width - space, r.height);
            return (extracted, remaining);
        }

        internal static Rect TakeLeftOnly(this Rect r, float width = 16f, float space = 0f)
        {
            return new Rect(r.x, r.y, width, r.height);
        }

        internal static Rect TakeRightOnly(this Rect r, float width = 16f, float space = 0f)
        {
            return new Rect(r.x + r.width - width, r.y, width, r.height);
        }

        internal static Rect PadLeft(this Rect r, float padding = 8f, bool condition = true)
        {
            return condition ? new Rect(r.x + padding, r.y, r.width - padding, r.height) : r;
        }

        internal static Rect PadRight(this Rect r, float padding = 8f, bool condition = true)
        {
            return condition ? new Rect(r.x, r.y, r.width - padding, r.height) : r;
        }

        internal static Rect PadHorizontal(this Rect r, float left = 8f, float right = 8f, bool condition = true)
        {
            return condition ? new Rect(r.x + left, r.y, r.width - left - right, r.height) : r;
        }

        internal static Rect WithPosition(this Rect r, float x, float y)
        {
            return new Rect(x, y, r.width, r.height);
        }
        
        internal static Rect WithSize(this Rect r, float width, float height)
        {
            return new Rect(r.x, r.y, width, height);
        }
        
        internal static Rect WithX(this Rect r, float x) => r.WithPosition(x, r.y);
        internal static Rect WithY(this Rect r, float y) => r.WithPosition(r.x, y);
        internal static Rect WithWidth(this Rect r, float w) => r.WithSize(w, r.height);
        internal static Rect WithHeight(this Rect r, float h) => r.WithSize(r.width, h);

        internal static Rect MoveBy(this Rect r, float dx = 0f, float dy = 0f)
        {
            return new Rect(r.x + dx, r.y + dy, r.width, r.height);
        }

        internal static Rect ExpandBy(this Rect r, float amount)
        {
            return new Rect(r.x - amount, r.y - amount, r.width + amount * 2f, r.height + amount * 2f);
        }

        internal static Rect ShrinkBy(this Rect r, float amount)
        {
            return new Rect(r.x + amount, r.y + amount, r.width - amount * 2f, r.height - amount * 2f);
        }

        internal static Vector2 Center(this Rect r)
        {
            return new Vector2(r.x + r.width * 0.5f, r.y + r.height * 0.5f);
        }

        internal static Rect CenteredRect(this Rect container, float width, float height)
        {
            var center = container.Center();
            return new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
        }

        internal static Rect AlignRight(this Rect container, float width)
        {
            return new Rect(container.xMax - width, container.y, width, container.height);
        }

        internal static Rect AlignBottom(this Rect container, float height)
        {
            return new Rect(container.x, container.yMax - height, container.width, height);
        }

        internal static Rect OnRightClick(this Rect rect, Action onRightClick, float padding = 0f)
        {
            var padRect = rect.PadHorizontal(padding, padding);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                if (padRect.Contains(Event.current.mousePosition))
                {
                    onRightClick?.Invoke();
                    Event.current.Use();
                }
            }
            return rect;
        }

        internal static Rect OnLeftClick(this Rect rect, Action onClick, float padding = 0f)
        {
            var padRect = rect.PadHorizontal(padding, padding);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (padRect.Contains(Event.current.mousePosition))
                {
                    onClick?.Invoke();
                    Event.current.Use();
                }
            }
            return rect;
        }

        internal static Rect OnDoubleClick(this Rect rect, Action onDoubleClick, float padding = 0f)
        {
            var padRect = rect.PadHorizontal(padding, padding);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2)
            {
                if (padRect.Contains(Event.current.mousePosition))
                {
                    onDoubleClick?.Invoke();
                    Event.current.Use();
                }
            }
            return rect;
        }

        // Edge manipulation methods - these preserve the exact logic of direct edge manipulation
        internal static Rect SetXMin(this Rect r, float xMin)
        {
            return new Rect(xMin, r.y, r.xMax - xMin, r.height);
        }

        internal static Rect SetXMax(this Rect r, float xMax)
        {
            return new Rect(r.x, r.y, xMax - r.x, r.height);
        }

        internal static Rect SetYMin(this Rect r, float yMin)
        {
            return new Rect(r.x, yMin, r.width, r.yMax - yMin);
        }

        internal static Rect SetYMax(this Rect r, float yMax)
        {
            return new Rect(r.x, r.y, r.width, yMax - r.y);
        }

        internal static Rect AddXMin(this Rect r, float delta)
        {
            return r.SetXMin(r.xMin + delta);
        }

        internal static Rect AddXMax(this Rect r, float delta)
        {
            return r.SetXMax(r.xMax + delta);
        }

        internal static Rect AddYMin(this Rect r, float delta)
        {
            return r.SetYMin(r.yMin + delta);
        }

        internal static Rect AddYMax(this Rect r, float delta)
        {
            return r.SetYMax(r.yMax + delta);
        }

    }
}