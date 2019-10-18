#if NET_4_6
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace ClusterVRSDK.Editor
{
    public class PreviewLauncherWindow : EditorWindow
    {
        void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                try
                {
#if UNITY_EDITOR_WIN
                    if (GUILayout.Button("for Desktop"))
                    {
                        Process.Start("C:\\Program Files (x86)\\cluster_desktop_dev\\cluster.exe");
                    }

                    if (GUILayout.Button("for HTCVive"))
                    {
                        Process.Start("C:\\Program Files (x86)\\cluster_vive_dev\\cluster.exe");
                    }

                    if (GUILayout.Button("for Oculus"))
                    {
                        Process.Start("C:\\Program Files (x86)\\cluster_oculus_dev\\cluster.exe");
                    }

#elif UNITY_EDITOR_OSX
                    if (GUILayout.Button("for Desktop"))
                    {
                        Process.Start("/Applications/cluster.app");
                    }
#endif
                }
                catch (Exception)
                {
                    EditorUtility.DisplayDialog("エラー", "cluster.がインストールされていません\nプレビューにはcluster.のインストールが必要です", "OK");
                }
            }
        }
    }
}
#endif
