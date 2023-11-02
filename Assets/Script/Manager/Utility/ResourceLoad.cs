using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

using Common;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

public enum AssetType
{
    None,
}

public static class ResourceLoad
{
    static Dictionary<AssetType, List<AsyncOperationHandle>> _handleDic = new Dictionary<AssetType, List<AsyncOperationHandle>>();

    static AsyncOperationHandle _op = new AsyncOperationHandle();

    public static AsyncOperationHandle<IResourceLocator> _handle = new AsyncOperationHandle<IResourceLocator>();
    static ResourceLoad()
    {
        Initialize();
    }

    public static void Initialize()
    {
        _handleDic.Clear();
    }

    public static void Destroy()
    {

    }

    public static Object Load(string path)
    {
        return Resources.Load(path);
    }

    public static T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public static IEnumerator InitAddressable()
    {
        bool isSimulate = false;
        string catalogPath = "catalogPath";

#if UNITY_EDITOR
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (null != settings && settings.ActivePlayModeDataBuilderIndex == 2)
            isSimulate = true;
#else
             isSimulate = true;
#endif
        yield return null;
        bool isDone = false;
        if (isSimulate)
        {
            Addressables.LoadContentCatalogAsync(catalogPath).Completed +=
                 (AsyncOperationHandle<IResourceLocator> obj) =>
                 {
                     _handle = obj;
                     isDone = true;
                 };
        }
        else
            isDone = true;

        while (!isDone) yield return null;
    }

    public static IEnumerator LoadAsync<T>(string path, System.Action<T> loadedCallback, AssetType type = AssetType.None, bool isBundle = false) where T : Object
    {
        if (isBundle)
        {
            if (_handle.IsValid())
            {
                IList<IResourceLocation> locations;
                if (_handle.Result.Locate(path, typeof(T), out locations))
                {
                    var resourceLocation = locations[0];
                    CommonDebug.Logf("LoadFileName: {0}", path);
                    _op = Addressables.LoadAssetAsync<T>(resourceLocation);
                }
            }
            else
            {
                _op = Addressables.LoadAssetAsync<T>(path);
            }

            yield return _op;


            if (_op.IsValid())
            {
                try
                {
                    _op.Completed += (AsyncOperationHandle handle) =>
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            _op = handle;

                            if (_handleDic.ContainsKey(type) == false)
                            {
                                _handleDic.Add(type, new List<AsyncOperationHandle>());
                            }
                            _handleDic[type].Add(_op);

                            loadedCallback(_op.Result as T);
                            CommonDebug.Log(_op.DebugName + "/" + _op.Status);
                        }
                        else
                        {
                            CommonDebug.LogError(path + "/" + _op.Status);
                        }
                    };
                }
                catch (Exception ex)
                {
                    CommonDebug.Log("Exception: " + path + "/" + _op.OperationException);
                    throw _op.OperationException;
                }
            }

        }
        else
        {
            loadedCallback(Resources.Load<T>(path) as T);
        }
    }

    public static T LoadResource<T>(string path, AssetType type = AssetType.None) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public static IEnumerator DownLoadAsync(string path, AssetType assetType, System.Action loadedCallback)
    {

        if (_handle.IsValid())
        {
            IList<IResourceLocation> locations;
            Type type = GetAssetType(assetType);
            if (_handle.Result.Locate(path, type, out locations))
            {
                var resourceLocation = locations[0];
                CommonDebug.Logf("LoadFileName: {0}", path);

                _op = Addressables.DownloadDependenciesAsync(resourceLocation.PrimaryKey);
            }
        }
        else
        {
            _op = Addressables.DownloadDependenciesAsync(path);
        }


        yield return _op;

        if (_op.IsValid())
        {
            try
            {
                _op.Completed += (AsyncOperationHandle handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        loadedCallback();
                    }
                    else
                    {
                        CommonDebug.LogError(path + "/" + _op.Status);
                    }
                    ReleaseAsset(_op);
                };
            }
            catch (Exception ex)
            {
                CommonDebug.Log("Exception: " + path + "/" + _op.OperationException);
                throw _op.OperationException;
            }
        }
    }

    public static void CheckDownLoadFileSize(IEnumerable<object> keys, System.Action<long> onComplete)
    {
        Addressables.GetDownloadSizeAsync(keys).Completed +=
            (AsyncOperationHandle<long> sizeHandle) =>
            {
                string sizeText = GetMBSize(sizeHandle.Result);

                foreach (var key in keys)
                {
                    CommonDebug.Logf("CheckList: {0}", key);
                }
                CommonDebug.Logf("Total FileSize: {0}", sizeText);
                onComplete?.Invoke(sizeHandle.Result);
                Addressables.Release(sizeHandle);
            };
    }

    public static void ClearDependecyCacheAsync(string key)
    {
        Addressables.ClearDependencyCacheAsync(key);
    }

    public static void ReleaseAsset<T>(Object obj)
    {
        Addressables.Release(obj);
    }

    public static void ReleaseAsset(AsyncOperationHandle op)
    {
        Addressables.Release(op);
    }

    public static void ReleaseInstance(AsyncOperationHandle op)
    {
        Addressables.ReleaseInstance(op);
    }

    public static void ReleaseAsset(AssetType type)
    {
        if (null != _handleDic)
        {
            if (_handleDic.ContainsKey(type))
            {
                for (int n = 0; n < _handleDic[type].Count; ++n)
                {
                    ReleaseAsset(_handleDic[type][n]);
                }
                _handleDic[type].Clear();
            }
        }
    }

    public static void ReleaseAllAssets()
    {
        if (null != _handleDic)
        {
            foreach (var item in _handleDic)
            {
                if (_handleDic.ContainsKey(item.Key))
                {
                    for (int n = 0; n < item.Value.Count; ++n)
                    {
                        ReleaseAsset(item.Value[n]);
                    }
                    _handleDic[item.Key].Clear();
                }
            }

            _handleDic.Clear();
            if (_handle.IsValid())
            {
                ReleaseAsset(_handle);
                Addressables.ClearResourceLocators();
            }
        }
    }

    public static string GetMBSize(long count)
    {
        double value = Math.Truncate((count / 1048576.0) * 100) / 100;
        return value + " MB";
    }

    static Type GetAssetType(AssetType type)
    {
        switch (type)
        {
            default:
                return typeof(GameObject);
        }
    }

}
