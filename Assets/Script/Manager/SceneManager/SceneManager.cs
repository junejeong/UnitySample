using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Common;

public enum SceneType
{
    None,
    Loading,
}

public class SceneControllManager
{
    public const string SCENE_BUNDLE_PATH = "Scenes";

    Dictionary<string, ISceneLoaded> _loadList;
    Dictionary<string, ISceneDestroy> _destroyList;
    Dictionary<string, ISceneProgress> _progressList;
    Dictionary<string, ISceneDone> _doneList;

    AsyncOperation _asyncOperation;

    SceneType SetPrecvSceneType
    {
        set
        {
            if (value != SceneType.Loading)
                _prevGeneralSceneType = value;
            _prevSceneType = value;
        }
    }

    SceneType _prevGeneralSceneType;
    SceneType _prevSceneType;
    SceneType _currentSceneType;

    List<SceneBase> _currentSceneList;

    Dictionary<int, string> _sceneInfo;
    Dictionary<int, int> _sceneUseCount;

    public SceneType NextSceneType { get; private set; }
    public SceneType CurrentScene { get { return _currentSceneType; } }
    public SceneType PrevScene { get { return _prevSceneType; } }
    public SceneType PrevGeneralScene { get { return _prevGeneralSceneType; } }

    public bool IsLoadingScene
    {
        get
        {
            return _currentSceneType == SceneType.Loading;
        }
    }

    public SceneControllManager()
    {
        InitializeSceneInfo();

        SceneManager.activeSceneChanged += SceneChanged;
        SceneManager.sceneLoaded += SceneLoaded;
        SceneManager.sceneUnloaded += SceneUnLoaded;

        _currentSceneType = GetSceneType(SceneManager.GetActiveScene().name);
        SetPrecvSceneType = _currentSceneType;
        NextSceneType = SceneType.None;

        _currentSceneList = new List<SceneBase>();
        _sceneUseCount = new Dictionary<int, int>();
    }

    void InitializeSceneInfo()
    {
        _sceneInfo = new Dictionary<int, string>();

        var types = from a in AppDomain.CurrentDomain.GetAssemblies()
                    from t in a.GetTypes()
                    where t.IsDefined(typeof(SceneInfoAttribute), false)
                    select t;

        foreach (var type in types)
        {
            var attrs = type.GetCustomAttributes(typeof(SceneInfoAttribute), false);
            if (attrs == null) continue;
            var attr = attrs[0] as SceneInfoAttribute;
            _sceneInfo.Add((int)attr._sceneType, attr._sceneName);
        }
    }

    SceneType GetSceneType(string sceneName)
    {
        var enumerator = _sceneInfo.GetEnumerator();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Value == sceneName)
                return (SceneType)enumerator.Current.Key;
        }
        return SceneType.None;
    }

    string GetSceneName(SceneType type)
    {
        int key = (int)type;
        if (_sceneInfo.ContainsKey(key))
            return _sceneInfo[key];
        return "";
    }

    public void AddScene(SceneBase scene)
    {
        if (!_currentSceneList.Contains(scene))
            _currentSceneList.Add(scene);
    }

    public void RemoveScene(SceneBase scene)
    {
        if (_currentSceneList.Contains(scene))
            _currentSceneList.Remove(scene);
    }

    public T GetScene<T>() where T : SceneBase
    {
        return _currentSceneList.Find(v => v is T) as T;
    }

    public T GetScene<T>(int idx) where T : SceneBase
    {
        if (_currentSceneList == null) return null;
        return (T)_currentSceneList[idx];
    }

    public bool IsGeneralScene()
    {
        return CurrentScene != SceneType.Loading && CurrentScene != SceneType.None;
    }

    #region Interface
    public void AddController(string sceneName, ISceneBase controllerBase)
    {
        AddLoad(sceneName, controllerBase);
        AddDestroy(sceneName, controllerBase);
        AddProgress(sceneName, controllerBase);
        AddDone(sceneName, controllerBase);
    }

    public void RemoveController(string sceneName, ISceneBase controllerBase)
    {
        RemoveLoad(sceneName, controllerBase);
        RemoveDestroy(sceneName, controllerBase);
        RemoveProgress(sceneName, controllerBase);
        RemoveDone(sceneName, controllerBase);
    }

    void AddLoad(string sceneName, ISceneBase controllerBase)
    {
        ISceneLoaded load = controllerBase as ISceneLoaded;
        if (load != null && !_loadList.ContainsKey(sceneName))
            _loadList.Add(sceneName, load);
    }

    void RemoveLoad(string sceneName, ISceneBase controllerBase)
    {
        ISceneLoaded load = controllerBase as ISceneLoaded;
        if (load != null && _loadList.ContainsKey(sceneName))
            _loadList.Remove(sceneName);
    }

    void AddDestroy(string sceneName, ISceneBase controllerBase)
    {
        ISceneDestroy destroy = controllerBase as ISceneDestroy;
        if (destroy != null && !_destroyList.ContainsKey(sceneName))
            _destroyList.Add(sceneName, destroy);
    }

    void RemoveDestroy(string sceneName, ISceneBase controllerBase)
    {
        ISceneDestroy destroy = controllerBase as ISceneDestroy;
        if (destroy != null && _destroyList.ContainsKey(sceneName))
            _destroyList.Remove(sceneName);
    }

    void AddProgress(string sceneName, ISceneBase controllerBase)
    {
        ISceneProgress progress = controllerBase as ISceneProgress;
        if (progress != null && !_progressList.ContainsKey(sceneName))
            _progressList.Add(sceneName, progress);
    }

    void RemoveProgress(string sceneName, ISceneBase controllerBase)
    {
        ISceneProgress progress = controllerBase as ISceneProgress;
        if (progress != null && _progressList.ContainsKey(sceneName))
            _progressList.Remove(sceneName);
    }

    void AddDone(string sceneName, ISceneBase controllerBase)
    {
        ISceneDone done = controllerBase as ISceneDone;
        if (done != null && !_doneList.ContainsKey(sceneName))
            _doneList.Add(sceneName, done);
    }

    void RemoveDone(string sceneName, ISceneBase controllerBase)
    {
        ISceneDone done = controllerBase as ISceneDone;
        if (done != null && _doneList.ContainsKey(sceneName))
            _doneList.Remove(sceneName);
    }
    #endregion

    void LoadExcute(string sceneName)
    {
        if (_loadList.ContainsKey(sceneName))
            _loadList[sceneName].SceneLoaded();
    }

    void DestroyExcute(string sceneName)
    {
        if (_destroyList.ContainsKey(sceneName))
            _destroyList[sceneName].SceneDestroy();
    }

    void ProgressExcute(string sceneName, float progress)
    {
        if (_progressList.ContainsKey(sceneName))
            _progressList[sceneName].NextSceneProgress(progress);
    }

    void DoneExcute(string sceneName)
    {
        if (_doneList.ContainsKey(sceneName))
            _doneList[sceneName].SceneDone();
    }

    bool GetLoadedDone(string sceneName)
    {
        if (_loadList.ContainsKey(sceneName))
            return _loadList[sceneName].LoadedDone;
        return false;
    }

    void SceneUnLoaded(Scene scene)
    {
        DestroyExcute(scene.name);
    }

    void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        LoadExcute(scene.name);
    }

    void SceneChanged(Scene prevScene, Scene currentScene)
    {
        DoneExcute(currentScene.name);
    }

    bool _isSceneLoading;

    public IEnumerator LoadScene(SceneType sceneType, bool async = false, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (sceneType == SceneType.None)
            yield break;

        _isSceneLoading = true;

        if (async)
        {
            yield return LoadScene(sceneType, loadSceneMode);
        }
        else
        {
            SceneManager.LoadScene(GetSceneName(sceneType), loadSceneMode);

            SetPrecvSceneType = _currentSceneType;
            _currentSceneType = sceneType;

            _isSceneLoading = false;
        }
    }

    public IEnumerator LoadLoadingScene(SceneType loadingSceneType, SceneType nextSceneType, bool async = false, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (nextSceneType == SceneType.None)
            yield break;

        NextSceneType = nextSceneType;

        yield return LoadScene(loadingSceneType, async, loadSceneMode);

        var currentScene = GetScene<SceneBase>();
        if (currentScene)
        {
            while (!currentScene.LoadedDone)
                yield return null;
        }

        yield return LoadScene(nextSceneType, async: true, loadSceneMode);
    }

    IEnumerator LoadScene(SceneType sceneType, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        _asyncOperation = SceneManager.LoadSceneAsync(GetSceneName(sceneType), loadSceneMode);
        if (_asyncOperation == null)
        {
            CommonDebug.LogError($"{sceneType} Scene Load Fail");
            yield break;
        }

        _asyncOperation.allowSceneActivation = false;

        float rate = 0.0f;

        while (true)
        {
            rate = _asyncOperation.progress;
            if (!_asyncOperation.isDone && _asyncOperation.progress >= 0.9f)
            {
                rate = 1.0f;
                _asyncOperation.allowSceneActivation = true;
            }

            ProgressExcute(GetSceneName(_prevSceneType), rate);

            if (_asyncOperation.isDone)
            {
                SetPrecvSceneType = _currentSceneType;
                _currentSceneType = sceneType;
                break;
            }

            yield return null;
        }

        yield return _asyncOperation;
        _isSceneLoading = false;
    }

    public void UseScene(SceneType sceneType)
    {
        int key = (int)sceneType;
        if (_sceneUseCount.ContainsKey(key))
            _sceneUseCount[key]++;
        else
            _sceneUseCount.Add(key, 1);
    }

    public int GetSceneUseCount(SceneType sceneType)
    {
        int key = (int)sceneType;

        if (_sceneUseCount.ContainsKey(key))
            return _sceneUseCount[key];
        return 0;
    }
}