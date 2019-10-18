using System.IO;
using ClusterVRSDK.Core.Editor.Venue;
using ClusterVRSDK.Core.Editor.Venue.Json;
using UnityEngine;

namespace ClusterVRSDK.Editor.Venue
{
    public class DrawThumbnailView
    {
        Texture2D imageTex;
        Texture2D savingBackgroundTex;
        string currentTexturePath;

        bool loadingImage;


        public DrawThumbnailView()
        {
            savingBackgroundTex = BlackWithAlpha(0.6f);
        }

        public void OverwriteDownloadUrl(ThumbnailUrl loadThumbnailUrl)
        {
            if (string.IsNullOrEmpty(loadThumbnailUrl.Url))
            {
                return;
            }

            if (imageTex != null && currentTexturePath == loadThumbnailUrl.Url)
            {
                return;
            }

            var downloadThumbnailService = new DownloadThumbnailService(
                loadThumbnailUrl,
                tex =>
                {
                    loadingImage = false;
                    imageTex = tex;
                });
            downloadThumbnailService.Run();
            loadingImage = true;
            imageTex = Texture2D.whiteTexture;
            currentTexturePath = loadThumbnailUrl.Url;
        }

        public void OverwriteFilePath(string loadTexturePath)
        {
            if (string.IsNullOrEmpty(loadTexturePath))
            {
                return;
            }

            if (imageTex != null && currentTexturePath == loadTexturePath)
            {
                return;
            }

            imageTex = new Texture2D(1, 1);
            imageTex.LoadImage(File.ReadAllBytes(loadTexturePath));
            imageTex.filterMode = FilterMode.Point;
            currentTexturePath = loadTexturePath;
        }

        public void DrawUI(bool saving)
        {
            if (imageTex == null)
            {
                return;
            }

            var options = new[]
            {
                GUILayout.Width(imageTex.width),
                GUILayout.MaxWidth(256),
                GUILayout.MinWidth(128),
                GUILayout.Height(imageTex.height),
                GUILayout.MaxHeight(256f * imageTex.height / imageTex.width),
                GUILayout.MinHeight(128f * imageTex.height / imageTex.width),
            };

            var rect = GUILayoutUtility.GetRect(128, 128, options);
            GUI.DrawTexture(rect, imageTex);

            if (loadingImage)
            {
                DrawOverlayWithString(rect, "読込中...");
            }
            else if (saving)
            {
                DrawOverlayWithString(rect, "保存中...");
            }
        }

        void DrawOverlayWithString(Rect rect, string message)
        {
            var boxStyle = new GUIStyle() {fontSize = 20, alignment = TextAnchor.MiddleCenter};
            boxStyle.normal.background = savingBackgroundTex;
            boxStyle.normal.textColor = Color.white;
            GUI.Box(rect, message, boxStyle);
        }

        Texture2D BlackWithAlpha(float alpha)
        {
            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixels(new Color[] {new Color(0, 0, 0, alpha)});
            tex.Apply();
            return tex;
        }
    }
}
