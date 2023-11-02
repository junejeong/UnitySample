using System;
using Common;
using JuneSample;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SceneInfoAttribute : Attribute
{
    public SceneType _sceneType { get; private set; }
    public string _sceneName { get; private set; }

    public SceneInfoAttribute(SceneType scenetype, string sceneName)
    {
        _sceneType = scenetype;
        _sceneName = sceneName;
    }
}
