public interface ISceneBase
{
}

public interface ISceneLoaded : ISceneBase
{
    bool LoadedDone
    {
        get;
    }
    void SceneLoaded();
}

public interface ISceneDestroy : ISceneBase
{
    void SceneDestroy();
}

public interface ISceneProgress : ISceneBase
{
    void NextSceneProgress(float progress);
}

public interface ISceneDone : ISceneBase
{
    void SceneDone();
}
