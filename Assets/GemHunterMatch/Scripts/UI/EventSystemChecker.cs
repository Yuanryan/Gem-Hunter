using UnityEngine;
using UnityEngine.EventSystems;

namespace Match3
{
    /// <summary>
    /// EventSystem檢查器 - 在編輯器中檢查EventSystem數量
    /// </summary>
    [System.Serializable]
    public class EventSystemChecker : MonoBehaviour
    {
        [Header("EventSystem檢查")]
        [SerializeField] private bool checkOnStart = true;
        [SerializeField] private bool logEventSystemInfo = true;
        
        private void Start()
        {
            if (checkOnStart)
            {
                CheckEventSystems();
            }
        }
        
        /// <summary>
        /// 檢查場景中的EventSystem
        /// </summary>
        [ContextMenu("檢查EventSystem")]
        public void CheckEventSystems()
        {
            EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
            
            if (logEventSystemInfo)
            {
                Debug.Log($"場景中發現 {eventSystems.Length} 個EventSystem:");
                
                for (int i = 0; i < eventSystems.Length; i++)
                {
                    EventSystem es = eventSystems[i];
                    Debug.Log($"  {i + 1}. {es.gameObject.name} (活躍: {es.gameObject.activeInHierarchy})");
                }
            }
            
            if (eventSystems.Length == 0)
            {
                Debug.LogWarning("場景中沒有EventSystem！");
            }
            else if (eventSystems.Length > 1)
            {
                Debug.LogError($"場景中有 {eventSystems.Length} 個EventSystem！這會導致UI事件衝突。");
                Debug.LogError("建議使用EventSystemManager來管理EventSystem。");
            }
            else
            {
                Debug.Log("EventSystem檢查通過：只有一個EventSystem。");
            }
        }
        
        /// <summary>
        /// 修復EventSystem問題
        /// </summary>
        [ContextMenu("修復EventSystem")]
        public void FixEventSystems()
        {
            EventSystemManager.Instance.EnsureSingleEventSystem();
            Debug.Log("EventSystem問題已修復。");
        }
        
        /// <summary>
        /// 刪除所有EventSystem
        /// </summary>
        [ContextMenu("刪除所有EventSystem")]
        public void RemoveAllEventSystems()
        {
            EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
            
            foreach (EventSystem es in eventSystems)
            {
                Debug.Log($"刪除EventSystem: {es.gameObject.name}");
                DestroyImmediate(es.gameObject);
            }
            
            Debug.Log("所有EventSystem已刪除。");
        }
    }
}
