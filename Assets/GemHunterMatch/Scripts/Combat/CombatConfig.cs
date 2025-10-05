using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 戰鬥配置 - 用於設定每個關卡的戰鬥參數
    /// </summary>
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "Match3/Combat Config")]
    public class CombatConfig : ScriptableObject
    {
        [Header("血量設定")]
        [Tooltip("玩家初始血量")]
        public int PlayerMaxHealth = 100;
        
        [Tooltip("敵人初始血量")]
        public int EnemyMaxHealth = 100;
        
        [Header("傷害設定")]
        [Tooltip("每個寶石的基礎傷害")]
        public int BaseDamagePerGem = 10;
        
        [Tooltip("連擊加成：消除超過此數量的寶石時開始加成")]
        public int ComboThreshold = 3;
        
        [Tooltip("連擊加成：每多一個寶石增加的傷害")]
        public int ComboBonusPerGem = 5;
        
        [Header("敵人設定")]
        [Tooltip("敵人回合持續時間（秒）")]
        public float EnemyTurnDuration = 2f;
        
        [Tooltip("敵人最小傷害")]
        public int EnemyMinDamage = 5;
        
        [Tooltip("敵人最大傷害")]
        public int EnemyMaxDamage = 15;
        
        [Header("UI設定")]
        [Tooltip("是否顯示戰鬥界面")]
        public bool ShowCombatUI = true;
        
        [Tooltip("戰鬥界面位置")]
        public CombatUIPosition UIPosition = CombatUIPosition.Top;
        
        [Tooltip("戰鬥界面透明度")]
        [Range(0f, 1f)]
        public float UIOpacity = 0.8f;
    }
    
    /// <summary>
    /// 戰鬥界面位置選項
    /// </summary>
    public enum CombatUIPosition
    {
        Top,        // 頂部
        Bottom,     // 底部
        Left,       // 左側
        Right       // 右側
    }
}
