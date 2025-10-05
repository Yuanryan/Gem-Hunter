using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Match3
{
    /// <summary>
    /// EventSystem管理器 - 確保場景中只有一個EventSystem
    /// </summary>
    public class EventSystemManager : MonoBehaviour
    {
        private static EventSystemManager s_Instance;
        private static EventSystem s_EventSystem;
        
        public static EventSystemManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    // 嘗試找到現有的EventSystemManager
                    s_Instance = FindObjectOfType<EventSystemManager>();
                    
                    if (s_Instance == null)
                    {
                        // 創建新的EventSystemManager
                        GameObject eventSystemGO = new GameObject("EventSystemManager");
                        s_Instance = eventSystemGO.AddComponent<EventSystemManager>();
                        DontDestroyOnLoad(eventSystemGO);
                    }
                }
                return s_Instance;
            }
        }
        
        private void Awake()
        {
            // 確保只有一個EventSystemManager實例
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeEventSystem();
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 訂閱場景加載事件
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // 取消訂閱場景加載事件
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        /// <summary>
        /// 初始化EventSystem
        /// </summary>
        private void InitializeEventSystem()
        {
            EnsureSingleEventSystem();
        }
        
        /// <summary>
        /// 場景加載時調用
        /// </summary>
        /// <param name="scene">加載的場景</param>
        /// <param name="mode">加載模式</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureSingleEventSystem();
        }
        
        /// <summary>
        /// 確保場景中只有一個EventSystem
        /// </summary>
        public void EnsureSingleEventSystem()
        {
            // 查找所有EventSystem
            EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
            
            if (eventSystems.Length == 0)
            {
                // 沒有EventSystem，創建一個
                CreateEventSystem();
            }
            else if (eventSystems.Length > 1)
            {
                // 有多個EventSystem，保留第一個，刪除其他的
                Debug.LogWarning($"發現 {eventSystems.Length} 個EventSystem，將保留第一個並刪除其他");
                
                s_EventSystem = eventSystems[0];
                
                // 刪除多餘的EventSystem
                for (int i = 1; i < eventSystems.Length; i++)
                {
                    Debug.Log($"刪除多餘的EventSystem: {eventSystems[i].gameObject.name}");
                    DestroyImmediate(eventSystems[i].gameObject);
                }
            }
            else
            {
                // 只有一個EventSystem，使用它
                s_EventSystem = eventSystems[0];
            }
            
            // 確保EventSystem是活躍的
            if (s_EventSystem != null)
            {
                s_EventSystem.gameObject.SetActive(true);
                Debug.Log($"使用EventSystem: {s_EventSystem.gameObject.name}");
            }
        }
        
        /// <summary>
        /// 創建EventSystem
        /// </summary>
        private void CreateEventSystem()
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            s_EventSystem = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            
            Debug.Log("創建新的EventSystem");
        }
        
        /// <summary>
        /// 獲取當前的EventSystem
        /// </summary>
        /// <returns>當前的EventSystem</returns>
        public EventSystem GetEventSystem()
        {
            if (s_EventSystem == null)
            {
                EnsureSingleEventSystem();
            }
            return s_EventSystem;
        }
        
        /// <summary>
        /// 檢查是否有EventSystem
        /// </summary>
        /// <returns>是否有EventSystem</returns>
        public bool HasEventSystem()
        {
            return s_EventSystem != null && s_EventSystem.gameObject.activeInHierarchy;
        }
        
        /// <summary>
        /// 強制重新創建EventSystem
        /// </summary>
        public void RecreateEventSystem()
        {
            // 刪除現有的EventSystem
            if (s_EventSystem != null)
            {
                DestroyImmediate(s_EventSystem.gameObject);
                s_EventSystem = null;
            }
            
            // 創建新的EventSystem
            CreateEventSystem();
        }
    }
}
