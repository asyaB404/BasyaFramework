using BasyaFramework.Model;
using BasyaFramework.ResourceManager;
using BasyaFramework.Timer;
using BasyaFramework.UI;
using UnityEngine;

namespace BasyaFramework.Core
{
    public class BxGame : MonoBehaviour
    {
        [SerializeField] private BxUIConfig uiConfig;
        private static BxGame _instance;
        public static IResourceManager ResourceManager => Instance._resourceManager;
        private IResourceManager _resourceManager; 

        public static BxModelManager ModelManager => Instance._modelManager;
        private BxModelManager _modelManager;
        public static BxUIManager UIManager => Instance._uiManager;
        private BxUIManager _uiManager;
        public static BxTimerManager TimerManager => Instance._timerManager;
        private BxTimerManager _timerManager;

        internal static BxGame Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BxGame>();
                }

                return _instance;
            }
        }

        public static void Initialize()
        {
            Instance.Initialize(null);
        }

        public void Initialize(IResourceManager resourceManager)
        {
            DontDestroyOnLoad(gameObject);
            _resourceManager = resourceManager ?? new DefaultResourceManager();
            _modelManager = new BxModelManager();
            _uiManager = new BxUIManager(uiConfig);
            _timerManager = new BxTimerManager();
            InitializeModel();
        }

        private void Awake()
        {
            _instance = this;
        }

        private void InitializeModel()
        {
            _modelManager.Init();
            _uiManager.Init();
        }

        private void Update()
        {
            _modelManager.Update();
            _uiManager.Update();
            _timerManager.Update();
        }
    }
}