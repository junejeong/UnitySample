using UnityEngine;
using UnityEditor;

namespace Audition_M
{
    [CustomEditor(typeof(BuildInfoScriptableObject))]
    public class BuildInfoEditor : Editor
    {
        BuildInfoScriptableObject buildInfo;
                
        public void OnEnable()
        {
            buildInfo = (BuildInfoScriptableObject)target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();

            DrawBuildInfoPath();

            DrawCommonPlayerSettingsGUI();

            DrawBuildTargetPlayerSettingGUI();

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(buildInfo);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        void DrawBuildInfoPath()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("[Build Info Path]", EditorStyles.boldLabel);

                buildInfo.RootFolderName = EditorGUILayout.TextField("RootFolderName", buildInfo.RootFolderName);
                buildInfo.SubFolderName = EditorGUILayout.TextField("SubFolderName", buildInfo.SubFolderName);
            }
            GUILayout.EndVertical();
        }

        void DrawCommonPlayerSettingsGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("[Player Settings - Common]", EditorStyles.boldLabel);

                buildInfo.ProductName = EditorGUILayout.TextField("ProductName", buildInfo.PackageName);
                buildInfo.DefaultIcon = (Texture2D)EditorGUILayout.ObjectField("DefaultIcon", buildInfo.DefaultIcon, typeof(Texture2D), false);
                buildInfo.PackageName = EditorGUILayout.TextField("PackageName", buildInfo.PackageName);
                buildInfo.AOS_Version = EditorGUILayout.TextField("AOS_Version", buildInfo.AOS_Version);
                buildInfo.AOS_BundleVersionCode = EditorGUILayout.IntField("AOS_BundleVersionCode", buildInfo.AOS_BundleVersionCode);
                buildInfo.IOS_Version = EditorGUILayout.TextField("IOS_Version", buildInfo.IOS_Version);
                buildInfo.IOS_BuildNumber = EditorGUILayout.TextField("IOS_BuildNumber", buildInfo.IOS_BuildNumber);
            }
            GUILayout.EndVertical();
        }

        void DrawBuildTargetPlayerSettingGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("[Player Settings - Build Target]", EditorStyles.boldLabel);

                buildInfo.BuildTargetGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup("BuildTargetGroup", buildInfo.BuildTargetGroup);

                GUILayout.BeginVertical(GUI.skin.box);
                {
                    if (buildInfo.BuildTargetGroup == BuildTargetGroup.Android)
                    {
                        DrawAOSPlayerSettingsGUI();
                    }
                    else
                    if (buildInfo.BuildTargetGroup == BuildTargetGroup.iOS)
                    {
                        DrawIOSPlayerSettingsGUI();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        bool _isFolded_AOS;
        void DrawAOSPlayerSettingsGUI()
        {
            GUILayout.Space(10f);
            GUILayout.BeginVertical();
            {
                if (_isFolded_AOS = EditorGUILayout.Foldout(_isFolded_AOS, "[Icon]"))
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        for (int i = 0; i < buildInfo.Android_Icon.Adaptive.Length; i++)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            {
                                var adaptive = buildInfo.Android_Icon.Adaptive[i];
                                if (adaptive.IsFolded = EditorGUILayout.Foldout(adaptive.IsFolded, "Adaptive"))
                                {
                                    adaptive.BackgroundIcon = (Texture2D)EditorGUILayout.ObjectField("AdaptiveIcon_Background", adaptive.BackgroundIcon, typeof(Texture2D), false);
                                    adaptive.ForegroundIcon = (Texture2D)EditorGUILayout.ObjectField("AdaptiveIcon_Foreground", adaptive.ForegroundIcon, typeof(Texture2D), false);
                                }
                            }
                            GUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();

            }
        }

        bool _isFolded_IOS;
        void DrawIOSPlayerSettingsGUI()
        {
            if (_isFolded_IOS = EditorGUILayout.Foldout(_isFolded_IOS, "[Icon]"))
            {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    for (int i = 0; i < buildInfo.Apple_Icon.Application.Length; i++)
                    {
                        GUILayout.BeginVertical(GUI.skin.box);
                        {
                            var application = buildInfo.Apple_Icon.Application[i];
                            if (application.IsFolded = EditorGUILayout.Foldout(application.IsFolded, "Application"))
                            {
                                application.Icon = (Texture2D)EditorGUILayout.ObjectField("Application icon", application.Icon, typeof(Texture2D), false);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}