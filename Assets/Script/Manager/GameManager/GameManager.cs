using UnityEngine;

namespace JuneSample
{
    [DefaultExecutionOrder(-100)]
    public partial class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                {
                    GameObject go = new GameObject();
                    go.name = "GameManager";
                    _instance = go.AddComponent<GameManager>();
                }
                return _instance;
            }
        }

        public static SceneControllManager SceneCtr;

        bool _isPause;
        public bool IsPause { get { return _isPause; } }

        void InitializeAwake()
        {

            SceneCtr = new SceneControllManager();

            SoundManager.Instance.Initialize();
            SoundManager.Instance.LoadDefaultSound();

            OffLineDataManager.Instance.LoadOffLineData();
        }

        void InitializeStart()
        {

        }


        public void Pause(bool tutorialPause = false)
        {
            if (_isPause)
                return;

            _isPause = true;
            Time.timeScale = 0.0f;
        }

        public void UnPause()
        {
            if (!_isPause)
                return;

            _isPause = false;
            Time.timeScale = 1.0f;
        }
    }
}
