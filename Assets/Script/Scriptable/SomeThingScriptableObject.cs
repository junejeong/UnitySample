
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjectCreate/Create SomeThingScriptableObject", fileName = "Something")]
public class SomeThingScriptableObject : ScriptableObject
{
    [Serializable]
    public class SomethingData
    {
        public string name;
        public Sprite sprite;
    }

    [Header("[Something]")]
    [SerializeField] public SomethingData[] somethingData;

}
