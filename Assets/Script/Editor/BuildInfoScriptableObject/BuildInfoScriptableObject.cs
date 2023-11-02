using UnityEngine;
using Common;
using UnityEditor;
using System.IO;

using System.Collections.Generic;

namespace Audition_M
{
    [CreateAssetMenu(menuName = "ScriptableObjectCreate/Create BuildInfoScriptableObject", fileName = "BuildInfo")]
    public class BuildInfoScriptableObject : ScriptableObject
    {
        #region Sub Class
        [System.Serializable]
        public class AOS_Icons
        {
            public AOS_Adaptive[] Adaptive;
        }

        [System.Serializable]
        public class AOS_Adaptive
        {
            public bool IsFolded;

            public Texture2D BackgroundIcon;
            public Texture2D ForegroundIcon;
        }

        [System.Serializable]
        public class AOS_Round
        {
            public bool IsFolded;

            public Texture2D Icon;
        }

        [System.Serializable]
        public class AOS_Legacy
        {
            public bool IsFolded;

            public Texture2D Icon;
        }

        [System.Serializable]
        public class IOS_Icons
        {
            public IOS_icon[] Application;
        }

        [System.Serializable]
        public class IOS_icon
        {
            public bool IsFolded;

            public Texture2D Icon;
        }
        #endregion

        // BuildData
        public string RootFolderName = "Root";
        public string SubFolderName = "Sub";

        // Player Settings - Common    
        public string ProductName;
        public Texture2D DefaultIcon;
        public string PackageName = "PackageName";
        public string AOS_Version;
        public string IOS_Version;
        public int AOS_BundleVersionCode;
        public string IOS_BuildNumber;

        // Player Settings - Build Target
        public BuildTargetGroup BuildTargetGroup;
        public AOS_Icons Android_Icon = new AOS_Icons();
        public IOS_Icons Apple_Icon = new IOS_Icons();

        public bool ChangeSettingForAOS()
        {
            ChangePlayerSettingsForAOS();
            return true;
        }

        public bool ChangeSettingForIOS()
        {
            ChangePlayerSettingsForIOS();
            return true;
        }

        #region Private Method

        private void OnEnable()
        {
            if (BuildTargetGroup == BuildTargetGroup.Android)
            {
                // Adaptive
                var kind = UnityEditor.Android.AndroidPlatformIconKind.Adaptive;
                var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, kind);
                if (Android_Icon.Adaptive == null || Android_Icon.Adaptive.Length < icons.Length)
                {
                    Android_Icon.Adaptive = new AOS_Adaptive[icons.Length];
                }
            }
#if UNITY_IOS
            else
            if (BuildTargetGroup == BuildTargetGroup.iOS)
            {
                // Application
                var kind = UnityEditor.iOS.iOSPlatformIconKind.Application;
                var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, kind);
                if (Apple_Icon.Application == null || Apple_Icon.Application.Length < icons.Length)
                {
                    Apple_Icon.Application = new IOS_icon[icons.Length];
                }
            }
#endif
        }

        #region PlayerSettings
        void ChangePlayerSettingsForAOS()
        {
            var buildTargetGroup = BuildTargetGroup.Android;
            ChangePlayerSettingForCommon(buildTargetGroup);
            SetAOSIcons();
        }

        void ChangePlayerSettingsForIOS()
        {
            var buildTargetGroup = BuildTargetGroup.iOS;
            ChangePlayerSettingForCommon(buildTargetGroup);

            SetIOSIcons();
        }

        void ChangePlayerSettingForCommon(BuildTargetGroup buildTargetGroup)
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { DefaultIcon });
            PlayerSettings.SetApplicationIdentifier(buildTargetGroup, PackageName);

            PlayerSettings.SplashScreen.showUnityLogo = false;

            if (buildTargetGroup == BuildTargetGroup.Android)
            {
                PlayerSettings.bundleVersion = AOS_Version;
                PlayerSettings.Android.bundleVersionCode = AOS_BundleVersionCode;
            }
            else if (buildTargetGroup == BuildTargetGroup.iOS)
            {
                PlayerSettings.bundleVersion = IOS_Version;
                PlayerSettings.iOS.buildNumber = IOS_BuildNumber;
            }
        }

        void SetAOSIcons()
        {
            BuildTargetGroup platform = BuildTargetGroup.Android;

            var kind = UnityEditor.Android.AndroidPlatformIconKind.Adaptive;
            var icons = PlayerSettings.GetPlatformIcons(platform, kind);
            for (int i = 0; i < icons.Length; i++)
            {
                var textures = new Texture2D[] { Android_Icon.Adaptive[i].BackgroundIcon, Android_Icon.Adaptive[i].ForegroundIcon };
                icons[i].SetTextures(textures);
                PlayerSettings.SetPlatformIcons(platform, kind, icons);
            }
        }

        void SetIOSIcons()
        {
#if UNITY_IOS

            var kind = UnityEditor.iOS.iOSPlatformIconKind.Application;
            var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, kind);
            for (int i = 0; i < icons.Length; i++)
            {
                var textures = new Texture2D[] { Apple_Icon.Application[i].Icon };
                icons[i].SetTextures(textures);
                PlayerSettings.SetPlatformIcons(BuildTargetGroup.iOS, kind, icons);
            }
#endif

        }
        #endregion

        bool DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                RefreshEditorProjectWindow();

                Debug.Log($"Complete Delete Directory : Directory Path({path})");
                return true;
            }
            else
            {
                Debug.LogError($"Failed Delete Directory : Directory Path({path})");
                return false;
            }
        }

        bool CopyDirectory(string sourcePath, string destPath)
        {
            DirectoryInfo dir = new DirectoryInfo(sourcePath);
            if (dir.Exists == false)
            {
                Debug.LogError("Source directory does not exist or could not be found: " + sourcePath);
                return false;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destPath);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destPath, file.Name);
                file.CopyTo(tempPath, true);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string tempPath = Path.Combine(destPath, subDir.Name);
                CopyDirectory(subDir.FullName, tempPath);
            }

            Debug.Log($"Complete Copy Directory : Directory Source Path({sourcePath}) / Directory Dest Path({destPath})");
            return true;
        }
        #endregion

        void RefreshEditorProjectWindow()
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
    }
}