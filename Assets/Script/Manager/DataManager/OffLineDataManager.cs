using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace JuneSample
{
    public class OffLineDataManager
    {
        static OffLineDataManager _instance;
        public static OffLineDataManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OffLineDataManager();
                return _instance;
            }
        }

#if UNITY_EDITOR
        public readonly string OfflineDataFolder = Application.dataPath + "/../OfflineData";
#else
    public readonly string OfflineDataFolder = Application.persistentDataPath + "/Data";
#endif

        static List<BaseLocalData> _baseSaveTestList = new List<BaseLocalData>();

        public OffLineDataManager()
        {
            CreateFolder();
        }

        public void CreateFolder()
        {
            string forder = OfflineDataFolder;
            if (!Directory.Exists(forder))
                Directory.CreateDirectory(forder);
        }

        public void LoadOffLineData()
        {

        }

        public void Clear()
        {
            SaveAll();
        }

        public void SaveAll()
        {
            for (int i = 0; i < (int)DataType.MAX; i++)
            {
                _baseSaveTestList[i].Save();
            }
        }
    }
}
