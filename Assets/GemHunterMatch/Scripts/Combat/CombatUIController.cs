using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3
{
    /// <summary>
    /// 戰鬥UI控制器 - 處理戰鬥界面的顯示和更新
    /// </summary>
    public class CombatUIController : MonoBehaviour
    {
        [Header("玩家UI")]
        [SerializeField] private Image playerHealthBar;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        
        [Header("敵人UI")]
        [SerializeField] private Image enemyHealthBar;
        [SerializeField] private TextMeshProUGUI enemyHealthText;
        
        [Header("戰鬥配置")]
        [SerializeField] private CombatConfig combatConfig;
        
        [Header("角色動畫")]
        [SerializeField] private CharacterAnimationController characterAnimations;
        
        [Header("Board引用")]
        [SerializeField] private Board board;
        
        [Header("動畫設定")]
        [SerializeField] private float healthBarAnimationSpeed = 2f;
        
        private int maxPlayerHealth;
        private int maxEnemyHealth;
        private int currentPlayerHealth;
        private int currentEnemyHealth;
        private bool isPlayerTurn = true;
        
        // 動畫同步狀態
        private bool isAnimating = false;
        
        private void Start()
        {
            // 訂閱Board的戰鬥事件
            Board.OnCombatStateChanged += UpdateCombatUI;
            Board.OnCombatEnded += OnCombatEnded;
            
            // 初始化UI
            InitializeUI();
        }
        
        private void OnDestroy()
        {
            // 取消訂閱事件
            Board.OnCombatStateChanged -= UpdateCombatUI;
            Board.OnCombatEnded -= OnCombatEnded;
        }
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            if (combatConfig != null)
            {
                maxPlayerHealth = combatConfig.PlayerMaxHealth;
                maxEnemyHealth = combatConfig.EnemyMaxHealth;
            }
            else
            {
                maxPlayerHealth = 100;
                maxEnemyHealth = 100;
            }
            
            currentPlayerHealth = maxPlayerHealth;
            currentEnemyHealth = maxEnemyHealth;
            
            // 初始更新UI
            UpdateHealthBars();
            UpdateHealthTexts();
        }
        
        /// <summary>
        /// 更新戰鬥UI
        /// </summary>
        /// <param name="playerHealth">玩家血量</param>
        /// <param name="enemyHealth">敵人血量</param>
        /// <param name="isPlayerTurn">是否玩家回合</param>
        private void UpdateCombatUI(int playerHealth, int enemyHealth, bool isPlayerTurn)
        {
            Debug.Log($"CombatUIController.UpdateCombatUI() - 收到更新: Player={playerHealth}, Enemy={enemyHealth}, PlayerTurn={isPlayerTurn}");
            
            // 先更新血量數據
            currentPlayerHealth = playerHealth;
            currentEnemyHealth = enemyHealth;
            this.isPlayerTurn = isPlayerTurn;
            
            // 不再通過CheckHealthChangesAndTriggerAnimations觸發動畫
            // 動畫現在由專門的觸發方法處理（TriggerPlayerAttackAnimation, TriggerEnemyHurtAnimation等）
            
            // 動畫更新血量條
            StartCoroutine(AnimateHealthBars());
            
            // 更新血量文字
            UpdateHealthTexts();
        }
        
        /// <summary>
        /// 觸發玩家攻擊動畫（用於傷害計算時）
        /// </summary>
        public void TriggerPlayerAttackAnimation()
        {
            if (characterAnimations == null) 
            {
                Debug.LogWarning("CharacterAnimationController 未設置！");
                return;
            }
            
            Debug.Log("CombatUIController: 觸發玩家攻擊動畫（傷害計算）");
            characterAnimations.PlayPlayerAttackAnimation();
        }
        
        /// <summary>
        /// 觸發敵人受傷動畫和血量更新
        /// </summary>
        public void TriggerEnemyHurtAnimation(int enemyHealth)
        {
            if (characterAnimations == null) 
            {
                Debug.LogWarning("CharacterAnimationController 未設置！");
                return;
            }
            
            Debug.Log($"CombatUIController: 觸發敵人受傷動畫，血量: {enemyHealth}");
            
            // 更新敵人血量
            currentEnemyHealth = enemyHealth;
            
            // 觸發敵人受傷動畫
            characterAnimations.PlayEnemyHurtAnimation(enemyHealth);
            
            // 更新血量UI
            StartCoroutine(AnimateHealthBars());
            UpdateHealthTexts();
        }
        
        /// <summary>
        /// 觸發敵人攻擊序列
        /// </summary>
        public void TriggerEnemyAttackSequence()
        {
            if (characterAnimations == null) 
            {
                Debug.LogWarning("CharacterAnimationController 未設置！");
                return;
            }
            
            Debug.Log("CombatUIController: 開始敵人攻擊序列");
            StartCoroutine(EnemyAttackSequence());
        }
        
        /// <summary>
        /// 敵人攻擊序列協程
        /// </summary>
        private System.Collections.IEnumerator EnemyAttackSequence()
        {
            // 觸發敵人攻擊動畫
            Debug.Log("CombatUIController: 觸發敵人攻擊動畫");
            characterAnimations.PlayEnemyAttackAnimation();
            
            // 等待0.2秒
            yield return new WaitForSeconds(0.2f);
            
            // 計算敵人傷害
            int damage = Random.Range(combatConfig.EnemyMinDamage, combatConfig.EnemyMaxDamage + 1);
            currentPlayerHealth -= damage;
            if (currentPlayerHealth < 0) currentPlayerHealth = 0;
            
            Debug.Log($"CombatUIController: 敵人對玩家造成 {damage} 點傷害，玩家剩餘血量: {currentPlayerHealth}");
            
            // 更新Board中的玩家血量
            UpdateBoardPlayerHealth(currentPlayerHealth);
            
            // 觸發玩家受傷動畫和血量更新
            TriggerPlayerHurtAnimation(currentPlayerHealth);
            
            // 等待0.5秒後切換回玩家回合
            yield return new WaitForSeconds(0.5f);
            
            // 切換回玩家回合
            isPlayerTurn = true;
            Debug.Log("CombatUIController: 切換回玩家回合");
            
            // 更新Board的回合狀態
            if (board != null)
            {
                board.UpdateTurnState(true);
                board.ResetDamageCalculationFlag(); // 重置傷害計算標誌
            }
            
            // 更新UI
            StartCoroutine(AnimateHealthBars());
            UpdateHealthTexts();
        }
        
        /// <summary>
        /// 觸發玩家受傷動畫和血量更新
        /// </summary>
        private void TriggerPlayerHurtAnimation(int playerHealth)
        {
            Debug.Log($"CombatUIController: 觸發玩家受傷動畫，血量: {playerHealth}");
            
            // 觸發玩家受傷動畫
            characterAnimations.PlayPlayerHurtAnimation(playerHealth);
            
            // 更新血量UI
            StartCoroutine(AnimateHealthBars());
            UpdateHealthTexts();
        }
        
        /// <summary>
        /// 更新Board中的玩家血量
        /// </summary>
        private void UpdateBoardPlayerHealth(int newHealth)
        {
            if (board != null)
            {
                board.UpdatePlayerHealth(newHealth);
            }
            else
            {
                Debug.LogWarning("Board 未設置，無法更新玩家血量");
            }
        }
        
        /// <summary>
        /// 檢查血量變化並觸發動畫
        /// </summary>
          private System.Collections.IEnumerator AnimateHealthBars()
        {
            float startPlayerFill = playerHealthBar.fillAmount;
            float startEnemyFill = enemyHealthBar.fillAmount;
            
            float targetPlayerFill = (float)currentPlayerHealth / maxPlayerHealth;
            float targetEnemyFill = (float)currentEnemyHealth / maxEnemyHealth;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * healthBarAnimationSpeed;
                float t = Mathf.Clamp01(elapsedTime);
                
                playerHealthBar.fillAmount = Mathf.Lerp(startPlayerFill, targetPlayerFill, t);
                enemyHealthBar.fillAmount = Mathf.Lerp(startEnemyFill, targetEnemyFill, t);
                
                // 根據血量改變顏色
                UpdateHealthBarColors();
                
                yield return null;
            }
            
            // 確保最終值正確
            playerHealthBar.fillAmount = targetPlayerFill;
            enemyHealthBar.fillAmount = targetEnemyFill;
        }
        
        /// <summary>
        /// 更新血量條顏色
        /// </summary>
        private void UpdateHealthBarColors()
        {
            // 玩家血量條顏色（綠色到紅色）
            float playerHealthPercent = playerHealthBar.fillAmount;
            if (playerHealthPercent > 0.6f)
            {
                playerHealthBar.color = Color.green;
            }
            else if (playerHealthPercent > 0.3f)
            {
                playerHealthBar.color = Color.yellow;
            }
            else
            {
                playerHealthBar.color = Color.red;
            }
            
            // 敵人血量條顏色（紅色到綠色）
            float enemyHealthPercent = enemyHealthBar.fillAmount;
            if (enemyHealthPercent > 0.6f)
            {
                enemyHealthBar.color = Color.red;
            }
            else if (enemyHealthPercent > 0.3f)
            {
                enemyHealthBar.color = Color.yellow;
            }
            else
            {
                enemyHealthBar.color = Color.green;
            }
        }
        
        /// <summary>
        /// 更新血量文字
        /// </summary>
        private void UpdateHealthTexts()
        {
            if (playerHealthText != null)
            {
                playerHealthText.text = $"{currentPlayerHealth}/{maxPlayerHealth}";
            }
            
            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{currentEnemyHealth}/{maxEnemyHealth}";
            }
        }
        
        /// <summary>
        /// 立即更新血量條（無動畫）
        /// </summary>
        private void UpdateHealthBars()
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.fillAmount = (float)currentPlayerHealth / maxPlayerHealth;
            }
            
            if (enemyHealthBar != null)
            {
                enemyHealthBar.fillAmount = (float)currentEnemyHealth / maxEnemyHealth;
            }
            
            UpdateHealthBarColors();
        }
        
        /// <summary>
        /// 戰鬥結束時調用
        /// </summary>
        /// <param name="victory">是否勝利</param>
        private void OnCombatEnded(bool victory)
        {
            Debug.Log($"戰鬥結束: {(victory ? "勝利" : "失敗")}");
            
            // 可以在這裡添加戰鬥結束的UI效果
            // 例如：顯示勝利/失敗畫面、播放音效等
        }
        
        /// <summary>
        /// 設定戰鬥配置
        /// </summary>
        /// <param name="config">戰鬥配置</param>
        public void SetCombatConfig(CombatConfig config)
        {
            combatConfig = config;
            InitializeUI();
        }
        
        /// <summary>
        /// 顯示/隱藏戰鬥UI
        /// </summary>
        /// <param name="show">是否顯示</param>
        public void ShowCombatUI(bool show)
        {
            gameObject.SetActive(show);
        }
    }
}
