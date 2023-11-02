
using Common;
using System.Collections.Generic;
using UnityEngine;

using JuneSample;


[DefaultExecutionOrder(-1)]
public class SceneBase : CommonBehaviour, ISceneLoaded, ISceneDestroy, ISceneProgress, ISceneDone
{
    string _sceneName;
    SceneType _sceneType;
  
    public class SortingOrderInfo
    {
        public GameObject _obj;
        public int _order;
    }


    public SceneBase(string sceneName, SceneType sceneType)
    {
        _sceneName = sceneName;
        _sceneType = sceneType;
    }

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {

    }

    protected virtual void OnDestroy()
    {

    }

    protected virtual void OnApplicationPause(bool pause)
    {
        if (pause)
        {
         
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CheckEscape();
        }
    }

    protected virtual void EscapeInput()
    {
        Application.Quit();
    }

    public virtual void SceneExit()
    {
        CommonDebug.Log($"<color=Red>[SceneExit] {_sceneName} Scene</color>");
    }

    public void CheckEscape()
    {     
        EscapeInput();
    }




    #region SceneManagerInterface

    public void AddInputInterface()
    {
        GameManager.SceneCtr.AddController(_sceneName, this);
        GameManager.SceneCtr.AddScene(this);
    }
    public void RemoveInputInterface()
    {
        GameManager.SceneCtr.RemoveController(_sceneName, this);
        GameManager.SceneCtr.RemoveScene(this);
    }

    public virtual bool LoadedDone
    {
        get
        {
            return true;
        }
    }

    public virtual void SceneLoaded()
    {

    }

    public virtual void SceneDestroy()
    {
    }

    public virtual void NextSceneProgress(float progress)
    {
    }

    bool _sceneDone;
    public virtual void SceneDone()
    {
        _sceneDone = true;
    }
    #endregion
}
