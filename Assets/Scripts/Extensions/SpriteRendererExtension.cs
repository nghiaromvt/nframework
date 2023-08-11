using UnityEngine;

namespace NFramework
{
    public static class SpriteRendererExtension
    {
        public static void SetAlpha(this SpriteRenderer sr, float alpha)
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }

        public static void FitCamera(this SpriteRenderer sr, Camera camera)
        {
            if (camera.orthographic)
            {
                float worldScreenHeight = camera.orthographicSize * 2;
                float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

                sr.transform.localScale = new Vector3(
                    worldScreenWidth / sr.sprite.bounds.size.x,
                    worldScreenHeight / sr.sprite.bounds.size.y, 1);
            }
            else
            {
                float spriteHeight = sr.sprite.bounds.size.y;
                float spriteWidth = sr.sprite.bounds.size.x;
                float distance = sr.transform.position.z - camera.transform.position.z;
                float screenHeight = 2 * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2) * distance;
                float screenWidth = screenHeight * camera.aspect;
                sr.transform.localScale = new Vector3(screenWidth / spriteWidth, screenHeight / spriteHeight, 1f);
            }
        }
    }
}