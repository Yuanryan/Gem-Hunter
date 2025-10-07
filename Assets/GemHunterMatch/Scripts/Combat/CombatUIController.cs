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
        [SerializeField] private SlicedFilledImage playerHealthBar;
        [SerializeField] private SlicedFilledImage playerHealthBarBackground;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        
        [Header("敵人UI")]
        [SerializeField] private SlicedFilledImage enemyHealthBar;
        [SerializeField] private SlicedFilledImage enemyHealthBarBackground;
        [SerializeField] private TextMeshProUGUI enemyHealthText;
        [Header("戰鬥配置")]
        [SerializeField] private CombatConfig combatConfig;
        
        [Header("角色動畫")]
        [SerializeField] private CharacterAnimationController characterAnimations;
        
        [Header("Board引用")]
        [SerializeField] private Board board;
        
        [Header("實時數值顯示")]
        [SerializeField] private TextMeshProUGUI playerDamageText; // 玩家傷害數值
        [SerializeField] private TextMeshProUGUI playerHealText; // 玩家治療數值
        [SerializeField] private TextMeshProUGUI playerShieldText; // 玩家護盾數值
        [SerializeField] private TextMeshProUGUI enemyDamageText; // 敵人傷害數值
        
        [Header("動畫設定")]
        [SerializeField] private float healthBarAnimationSpeed = 2f;
        [SerializeField] private float healthBarShrinkDelay = 0.5f; // 血量條縮短延遲時間
        [SerializeField] private float healthBarShrinkSpeed = 1f; // 血量條縮短速度
        
        [Header("血量鎖定視覺效果")]
        [SerializeField] private GameObject shieldIconPrefab;
        [SerializeField] private Transform enemyHealthBarParent;
        
        private int maxPlayerHealth;
        private int maxEnemyHealth;
        private int currentPlayerHealth;
        private int currentEnemyHealth;
        
        private GameObject currentShieldIcon;
        private bool isPlayerTurn = true;
        
        // 實時數值追蹤
        private int currentTurnDamage = 0;
        private int currentTurnHeal = 0;
        private int currentTurnShield = 0;
        private int currentTurnEnemyDamage = 0; // 敵人下回合傷害（在玩家回合時顯示）
        
        // 動畫同步狀態
        private bool isAnimating = false;
        
        // 血量條動畫狀態
        private float previousPlayerHealthPercent = 1f;
        private float previousEnemyHealthPercent = 1f;
        private bool isPlayerHealthBarShrinking = false;
        private bool isEnemyHealthBarShrinking = false;
        
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
            
            // 初始化血量條背景
            InitializeHealthBarBackgrounds();
            
            // 初始更新UI
            UpdateHealthBars();
            UpdateHealthTexts();
        }
        
        /// <summary>
        /// 初始化血量條背景
        /// </summary>
        private void InitializeHealthBarBackgrounds()
        {
            // 設定玩家血量條背景為灰色
            if (playerHealthBarBackground != null)
            {
                playerHealthBarBackground.color = Color.gray;
                playerHealthBarBackground.fillAmount = 1f;
                Debug.Log("玩家血量條背景已初始化");
            }
            else
            {
                Debug.LogWarning("玩家血量條背景未設置！");
            }
            
            // 設定敵人血量條背景為灰色
            if (enemyHealthBarBackground != null)
            {
                enemyHealthBarBackground.color = Color.gray;
                enemyHealthBarBackground.fillAmount = 1f;
                Debug.Log("敵人血量條背景已初始化");
            }
            else
            {
                Debug.LogWarning("敵人血量條背景未設置！");
            }
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
            
            // 檢查是否血量被鎖定（目標未完成且血量為1）
            bool goalsCompleted = LevelData.Instance.GoalLeft == 0;
            bool healthLocked = !goalsCompleted && enemyHealth == 1;
            
            if (healthLocked)
            {
                Debug.Log($"敵人血量被鎖定！需要完成目標才能擊敗敵人 (血量: {enemyHealth}, 目標剩餘: {LevelData.Instance.GoalLeft})");
                ShowShieldIcon();
            }
            else
            {
                Debug.Log($"敵人血量未鎖定 (血量: {enemyHealth}, 目標剩餘: {LevelData.Instance.GoalLeft})");
                HideShieldIcon();
            }
            
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
            
            // 使用預先計算的敵人傷害
            int damage = currentTurnEnemyDamage;
            Debug.Log($"CombatUIController: 使用預先計算的敵人傷害: {damage}");
            
            // 檢查是否有待處理的護盾量（來自綠色寶石）
            int shieldAmount = 0;
            if (board != null)
            {
                shieldAmount = board.GetAndClearPendingShieldAmount();
                if (shieldAmount > 0)
                {
                    Debug.Log($"CombatUIController: 獲得護盾！護盾量: {shieldAmount}");
                    // 計算實際護盾量（不超過最大護盾）
                    int maxShieldAmount = combatConfig.MaxShieldAmount;
                    int actualShieldAmount = Mathf.Min(shieldAmount, maxShieldAmount - board.GetCurrentShieldAmount());
                    
                    if (actualShieldAmount > 0)
                    {
                        int newShieldAmount = board.GetCurrentShieldAmount() + actualShieldAmount;
                        board.SetCurrentShieldAmount(newShieldAmount);
                        Debug.Log($"CombatUIController: 護盾已激活！當前護盾值: {newShieldAmount}");
                    }
                }
            }
            
            // 計算實際傷害（護盾阻擋）
            int currentShield = board != null ? board.GetCurrentShieldAmount() : 0;
            int actualDamage = Mathf.Max(0, damage - currentShield);
            int shieldUsed = Mathf.Min(damage, currentShield);
            
            if (currentShield > 0)
            {
                Debug.Log($"CombatUIController: 護盾阻擋了 {shieldUsed} 點傷害！剩餘護盾: {currentShield - shieldUsed}");
                // 更新護盾值
                int newShieldAmount = Mathf.Max(0, currentShield - shieldUsed);
                if (board != null)
                {
                    board.SetCurrentShieldAmount(newShieldAmount);
                }
            }
            
            currentPlayerHealth -= actualDamage;
            if (currentPlayerHealth < 0) currentPlayerHealth = 0;
            
            Debug.Log($"CombatUIController: 敵人對玩家造成 {damage} 點傷害，護盾阻擋 {shieldUsed} 點，實際傷害 {actualDamage}，玩家剩餘血量: {currentPlayerHealth}");
            
            // 更新Board中的玩家血量
            UpdateBoardPlayerHealth(currentPlayerHealth);
            
            // 根據是否有護盾來決定播放哪種動畫
            if (currentShield > 0 && shieldUsed > 0)
            {
                // 有護盾阻擋傷害，播放防禦動畫
                Debug.Log("CombatUIController: 播放防禦動畫（護盾阻擋）");
                characterAnimations.PlayPlayerDefendAnimation();
            }
            else
            {
                // 沒有護盾或護盾被完全擊破，播放受傷動畫
            TriggerPlayerHurtAnimation(currentPlayerHealth);
            }
            
            // 等待0.3秒讓動畫播放
            yield return new WaitForSeconds(0.3f);
            
            // 檢查是否有待處理的治療量（來自白色寶石）
            if (board != null)
            {
                int healAmount = board.GetAndClearPendingHealAmount();
                if (healAmount > 0)
                {
                    Debug.Log($"CombatUIController: 開始治療玩家！治療量: {healAmount}");
                    yield return StartCoroutine(HealPlayerSequence(healAmount));
                }
            }
            
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
        /// 治療玩家序列
        /// </summary>
        /// <param name="healAmount">治療量</param>
        private System.Collections.IEnumerator HealPlayerSequence(int healAmount)
        {
            Debug.Log($"CombatUIController: 開始治療序列，治療量: {healAmount}");
            
            // 播放治療動畫
            if (characterAnimations != null)
            {
                Debug.Log("CombatUIController: 播放玩家治療動畫");
                characterAnimations.PlayPlayerHealAnimation();
            }
            
            // 等待治療動畫播放
            yield return new WaitForSeconds(0.5f);
            
            // 計算實際治療量（不超過最大血量）
            int actualHealAmount = Mathf.Min(healAmount, maxPlayerHealth - currentPlayerHealth);
            
            if (actualHealAmount > 0)
            {
                // 記錄治療前的血量百分比
                float previousHealthPercent = (float)currentPlayerHealth / maxPlayerHealth;
                
                currentPlayerHealth += actualHealAmount;
                Debug.Log($"CombatUIController: 玩家被治療！恢復血量: {actualHealAmount}, 當前血量: {currentPlayerHealth}");
                
                // 更新Board中的玩家血量
                UpdateBoardPlayerHealth(currentPlayerHealth);
                
                // 播放治療血量條動畫
                yield return StartCoroutine(AnimateHealthBarIncrease(true, previousHealthPercent, (float)currentPlayerHealth / maxPlayerHealth));
                
                // 更新血量文字
                UpdateHealthTexts();
            }
            else
            {
                Debug.Log($"CombatUIController: 玩家血量已滿，無法治療 (當前血量: {currentPlayerHealth}, 最大血量: {maxPlayerHealth})");
            }
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
            float targetPlayerFill = (float)currentPlayerHealth / maxPlayerHealth;
            float targetEnemyFill = (float)currentEnemyHealth / maxEnemyHealth;
            
            Debug.Log($"AnimateHealthBars: 玩家血量 {currentPlayerHealth}/{maxPlayerHealth} ({targetPlayerFill:F2}), 敵人血量 {currentEnemyHealth}/{maxEnemyHealth} ({targetEnemyFill:F2})");
            Debug.Log($"AnimateHealthBars: 上次玩家血量 {previousPlayerHealthPercent:F2}, 上次敵人血量 {previousEnemyHealthPercent:F2}");
            
            // 檢查是否需要縮短動畫
            bool playerHealthDecreased = targetPlayerFill < previousPlayerHealthPercent;
            bool enemyHealthDecreased = targetEnemyFill < previousEnemyHealthPercent;
            
            Debug.Log($"AnimateHealthBars: 玩家血量減少: {playerHealthDecreased}, 敵人血量減少: {enemyHealthDecreased}");
            
            if (playerHealthDecreased)
            {
                Debug.Log($"玩家血量減少，開始血量條縮短動畫");
                StartCoroutine(AnimateHealthBarDecrease(true, previousPlayerHealthPercent, targetPlayerFill));
            }
            else
            {
                // 血量增加時立即更新
                playerHealthBar.fillAmount = targetPlayerFill;
                if (playerHealthBarBackground != null)
                {
                    playerHealthBarBackground.fillAmount = targetPlayerFill;
                }
                Debug.Log($"玩家血量增加，立即更新到 {targetPlayerFill:F2}");
            }
            
            if (enemyHealthDecreased)
            {
                Debug.Log($"敵人血量減少，開始血量條縮短動畫");
                StartCoroutine(AnimateHealthBarDecrease(false, previousEnemyHealthPercent, targetEnemyFill));
            }
            else
            {
                // 血量增加時立即更新
                enemyHealthBar.fillAmount = targetEnemyFill;
                if (enemyHealthBarBackground != null)
                {
                    enemyHealthBarBackground.fillAmount = targetEnemyFill;
                }
                Debug.Log($"敵人血量增加，立即更新到 {targetEnemyFill:F2}");
            }
            
            // 更新記錄的血量百分比
            previousPlayerHealthPercent = targetPlayerFill;
            previousEnemyHealthPercent = targetEnemyFill;
            
            // 根據血量改變顏色
            UpdateHealthBarColors();
            
            yield return null;
        }
        
        /// <summary>
        /// 血量條減少動畫
        /// </summary>
        /// <param name="isPlayer">是否為玩家血量條</param>
        /// <param name="startFill">開始填充量</param>
        /// <param name="targetFill">目標填充量</param>
        private System.Collections.IEnumerator AnimateHealthBarDecrease(bool isPlayer, float startFill, float targetFill)
        {
            SlicedFilledImage healthBar = isPlayer ? playerHealthBar : enemyHealthBar;
            SlicedFilledImage backgroundBar = isPlayer ? playerHealthBarBackground : enemyHealthBarBackground;
            
            Debug.Log($"AnimateHealthBarDecrease: {(isPlayer ? "玩家" : "敵人")}血量條從 {startFill:F2} 到 {targetFill:F2}");
            
            if (healthBar == null)
            {
                Debug.LogWarning($"{(isPlayer ? "玩家" : "敵人")}血量條未設置！");
                yield break;
            }
            
            // 立即縮短實際血量條
            healthBar.fillAmount = targetFill;
            Debug.Log($"{(isPlayer ? "玩家" : "敵人")}實際血量條立即縮短到 {targetFill:F2}");
            
            // 如果沒有背景血量條，使用實際血量條的動畫
            if (backgroundBar == null)
            {
                Debug.LogWarning($"{(isPlayer ? "玩家" : "敵人")}血量條背景未設置，使用實際血量條動畫");
                
                // 重置實際血量條到開始位置
                healthBar.fillAmount = startFill;
                
                // 等待延遲時間
                yield return new WaitForSeconds(healthBarShrinkDelay);
                
                // 動畫縮短實際血量條
                float animElapsedTime = 0f;
                float animDuration = Mathf.Max(1f, (startFill - targetFill) / healthBarShrinkSpeed);
                
                while (animElapsedTime < animDuration)
                {
                    animElapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(animElapsedTime / animDuration);
                    float easedT = 1f - Mathf.Pow(1f - t, 3f);
                    
                    healthBar.fillAmount = Mathf.Lerp(startFill, targetFill, easedT);
                    yield return null;
                }
                
                healthBar.fillAmount = targetFill;
                Debug.Log($"{(isPlayer ? "玩家" : "敵人")}實際血量條動畫完成");
                yield break;
            }
            
            // 確保背景血量條在正確位置
            backgroundBar.fillAmount = startFill;
            
            // 等待延遲時間後開始縮短背景血量條
            Debug.Log($"等待 {healthBarShrinkDelay} 秒後開始背景血量條動畫");
            yield return new WaitForSeconds(healthBarShrinkDelay);
            
            float elapsedTime = 0f;
            float duration = (startFill - targetFill) / healthBarShrinkSpeed;
            
            // 確保動畫持續時間至少1秒，讓效果更明顯
            duration = Mathf.Max(duration, 1f);
            
            Debug.Log($"{(isPlayer ? "玩家" : "敵人")}背景血量條開始縮短動畫：從 {startFill:F2} 到 {targetFill:F2}，持續時間：{duration:F2}秒");
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                
                // 使用緩動函數讓動畫更平滑
                float easedT = 1f - Mathf.Pow(1f - t, 3f); // 三次方緩動
                
                backgroundBar.fillAmount = Mathf.Lerp(startFill, targetFill, easedT);
                
                yield return null;
            }
            
            // 確保最終值正確
            backgroundBar.fillAmount = targetFill;
            Debug.Log($"{(isPlayer ? "玩家" : "敵人")}血量條縮短動畫完成");
        }
        
        /// <summary>
        /// 血量條增加動畫（用於治療）
        /// </summary>
        /// <param name="isPlayer">是否為玩家血量條</param>
        /// <param name="startFill">開始填充量</param>
        /// <param name="targetFill">目標填充量</param>
        private System.Collections.IEnumerator AnimateHealthBarIncrease(bool isPlayer, float startFill, float targetFill)
        {
            SlicedFilledImage healthBar = isPlayer ? playerHealthBar : enemyHealthBar;
            SlicedFilledImage backgroundBar = isPlayer ? playerHealthBarBackground : enemyHealthBarBackground;
            
            Debug.Log($"AnimateHealthBarIncrease: {(isPlayer ? "玩家" : "敵人")}血量條從 {startFill:F2} 增加到 {targetFill:F2}");
            
            if (healthBar == null)
            {
                Debug.LogWarning($"{(isPlayer ? "玩家" : "敵人")}血量條未設置！");
                yield break;
            }
            
            // 治療動畫：血量條平滑增長
            float duration = Mathf.Max(0.5f, (targetFill - startFill) / healthBarAnimationSpeed);
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // 使用平滑的動畫曲線
                float easedT = Mathf.SmoothStep(0f, 1f, t);
                
                float currentFill = Mathf.Lerp(startFill, targetFill, easedT);
                healthBar.fillAmount = currentFill;
                
                // 同時更新背景血量條
                if (backgroundBar != null)
                {
                    backgroundBar.fillAmount = currentFill;
                }
                
                yield return null;
            }
            
            // 確保最終值正確
            healthBar.fillAmount = targetFill;
            if (backgroundBar != null)
            {
                backgroundBar.fillAmount = targetFill;
            }
            
            Debug.Log($"AnimateHealthBarIncrease: {(isPlayer ? "玩家" : "敵人")}血量條動畫完成，最終填充量: {targetFill:F2}");
        }
        
        /// <summary>
        /// 測試血量條動畫（用於調試）
        /// </summary>
        [ContextMenu("測試血量條動畫")]
        public void TestHealthBarAnimation()
        {
            Debug.Log("=== 開始測試血量條動畫 ===");
            
            // 檢查血量條設置
            Debug.Log($"玩家血量條: {(playerHealthBar != null ? "已設置" : "未設置")}");
            Debug.Log($"敵人血量條: {(enemyHealthBar != null ? "已設置" : "未設置")}");
            
            if (playerHealthBar != null)
            {
                Debug.Log($"玩家血量條 Fill Direction: {playerHealthBar.fillDirection}");
                Debug.Log($"玩家血量條當前填充量: {playerHealthBar.fillAmount}");
                Debug.Log($"玩家血量條顏色: {playerHealthBar.color}");
            }
            
            // 檢查血量條背景是否設置
            Debug.Log($"玩家血量條背景: {(playerHealthBarBackground != null ? "已設置" : "未設置")}");
            Debug.Log($"敵人血量條背景: {(enemyHealthBarBackground != null ? "已設置" : "未設置")}");
            
            if (playerHealthBarBackground != null)
            {
                Debug.Log($"玩家血量條背景顏色: {playerHealthBarBackground.color}");
                Debug.Log($"玩家血量條背景填充量: {playerHealthBarBackground.fillAmount}");
            }
            
            // 強制設置血量條為滿血
            if (playerHealthBar != null)
            {
                playerHealthBar.fillAmount = 1f;
                Debug.Log("強制設置玩家血量條為滿血");
            }
            
            // 模擬玩家血量減少
            int originalHealth = currentPlayerHealth;
            currentPlayerHealth = Mathf.Max(1, currentPlayerHealth - 50);
            
            Debug.Log($"測試：玩家血量從 {originalHealth} 減少到 {currentPlayerHealth}");
            
            // 直接測試血量條變化
            if (playerHealthBar != null)
            {
                float targetFill = (float)currentPlayerHealth / maxPlayerHealth;
                Debug.Log($"目標填充量: {targetFill:F2}");
                
                // 立即改變填充量
                playerHealthBar.fillAmount = targetFill;
                Debug.Log($"立即設置玩家血量條填充量為: {playerHealthBar.fillAmount:F2}");
            }
            
            // 觸發動畫
            StartCoroutine(AnimateHealthBars());
        }
        
        /// <summary>
        /// 簡單測試血量條填充量（用於調試）
        /// </summary>
        [ContextMenu("簡單測試血量條填充量")]
        public void SimpleTestHealthBarFill()
        {
            Debug.Log("=== 簡單測試血量條填充量 ===");
            
            if (playerHealthBar != null)
            {
                Debug.Log($"測試前填充量: {playerHealthBar.fillAmount:F2}");
                
                // 設置為50%
                playerHealthBar.fillAmount = 0.5f;
                Debug.Log($"設置為50%後填充量: {playerHealthBar.fillAmount:F2}");
                
                // 等待1秒
                StartCoroutine(TestFillChange());
            }
            else
            {
                Debug.LogError("玩家血量條未設置！");
            }
        }
        
        private System.Collections.IEnumerator TestFillChange()
        {
            yield return new WaitForSeconds(1f);
            
            if (playerHealthBar != null)
            {
                // 設置為25%
                playerHealthBar.fillAmount = 0.25f;
                Debug.Log($"1秒後設置為25%填充量: {playerHealthBar.fillAmount:F2}");
            }
        }
        
        /// <summary>
        /// 更新實時數值顯示
        /// </summary>
        public void UpdateRealtimeValues()
        {
            if (board == null) return;
            
            // 獲取當前回合的寶石清除數量
            int whiteGems = board.GetCurrentTurnWhiteGemsCleared();
            int greenGems = board.GetCurrentTurnGreenGemsCleared();
            int totalGems = board.GetCurrentTurnGemsCleared();
            
            // 計算玩家數值
            currentTurnDamage = (totalGems - whiteGems - greenGems) * combatConfig.BaseDamagePerGem;
            currentTurnHeal = whiteGems * combatConfig.HealPerWhiteGem;
            currentTurnShield = greenGems * combatConfig.ShieldPerGreenGem;
            
            // 敵人傷害只在回合開始時計算一次，這裡不重新計算
            // currentTurnEnemyDamage 保持不變直到下個回合
            
            // 更新UI顯示
            UpdateRealtimeTexts();
            
            Debug.Log($"實時數值更新 - 玩家傷害: {currentTurnDamage}, 治療: {currentTurnHeal}, 護盾: {currentTurnShield}, 敵人下回合傷害: {currentTurnEnemyDamage}");
        }
        
        /// <summary>
        /// 計算敵人下回合的傷害（在玩家回合開始時調用）
        /// </summary>
        public void CalculateEnemyDamageForTurn()
        {
            currentTurnEnemyDamage = Random.Range(combatConfig.EnemyMinDamage, combatConfig.EnemyMaxDamage + 1);
            Debug.Log($"敵人下回合傷害已計算: {currentTurnEnemyDamage}");
            
            // 更新UI顯示
            UpdateRealtimeTexts();
        }
        
        /// <summary>
        /// 更新實時數值文字
        /// </summary>
        private void UpdateRealtimeTexts()
        {
            // 更新玩家傷害
            if (playerDamageText != null)
            {
                playerDamageText.text = $"Attack: {currentTurnDamage}";
                playerDamageText.color = currentTurnDamage > 0 ? Color.red : Color.gray;
            }
            
            // 更新玩家治療
            if (playerHealText != null)
            {
                playerHealText.text = $"Healing: {currentTurnHeal}";
                playerHealText.color = currentTurnHeal > 0 ? Color.green : Color.gray;
            }
            
            // 更新玩家護盾
            if (playerShieldText != null)
            {
                playerShieldText.text = $"Shield: {currentTurnShield}";
                playerShieldText.color = currentTurnShield > 0 ? Color.cyan : Color.gray;
            }
            
            // 更新敵人傷害（顯示敵人本回合傷害）
            if (enemyDamageText != null)
            {
                enemyDamageText.text = $"Attack: {currentTurnEnemyDamage}";
                enemyDamageText.color = currentTurnEnemyDamage > 0 ? Color.red : Color.gray;
            }
        }
        
        /// <summary>
        /// 重置實時數值
        /// </summary>
        public void ResetRealtimeValues()
        {
            currentTurnDamage = 0;
            currentTurnHeal = 0;
            currentTurnShield = 0;
            currentTurnEnemyDamage = 0;
            UpdateRealtimeTexts();
            Debug.Log("實時數值已重置");
        }
        
        /// <summary>
        /// 測試實時數值更新（用於調試）
        /// </summary>


        /// <summary>
        /// 測試治療動畫（用於調試）
        /// </summary>
        [ContextMenu("測試治療動畫")]
        public void TestHealAnimation()
        {
            Debug.Log("=== 開始測試治療動畫 ===");
            
            if (playerHealthBar == null)
            {
                Debug.LogError("玩家血量條未設置！");
                return;
            }
            
            // 記錄原始血量
            int originalHealth = currentPlayerHealth;
            
            // 減少血量到50%
            currentPlayerHealth = Mathf.Max(1, maxPlayerHealth / 2);
            Debug.Log($"測試：玩家血量從 {originalHealth} 減少到 {currentPlayerHealth}");
            
            // 更新血量條
            float currentFill = (float)currentPlayerHealth / maxPlayerHealth;
            playerHealthBar.fillAmount = currentFill;
            if (playerHealthBarBackground != null)
            {
                playerHealthBarBackground.fillAmount = currentFill;
            }
            
            // 等待1秒後開始治療
            StartCoroutine(TestHealSequence());
        }
        
        /// <summary>
        /// 測試治療序列
        /// </summary>
        private System.Collections.IEnumerator TestHealSequence()
        {
            yield return new WaitForSeconds(1f);
            
            Debug.Log("開始測試治療序列");
            yield return StartCoroutine(HealPlayerSequence(30));
            
            Debug.Log("治療動畫測試完成");
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
        /// 顯示盾牌圖標
        /// </summary>
        private void ShowShieldIcon()
        {
            if (currentShieldIcon != null) return; // 已經顯示了
            
            // 使用敵人血量條的父物件，如果沒有設置則使用敵人血量條本身
            Transform parentTransform = enemyHealthBarParent != null ? enemyHealthBarParent : enemyHealthBar.transform;
            
            if (parentTransform != null)
            {
                // 創建盾牌圖標 GameObject
                currentShieldIcon = new GameObject("ShieldIcon");
                currentShieldIcon.transform.SetParent(parentTransform);
                currentShieldIcon.transform.SetAsLastSibling(); // 確保在最上層
                
                // 添加必要的組件
                currentShieldIcon.AddComponent<RectTransform>();
                currentShieldIcon.AddComponent<ShieldIcon>();
                
                Debug.Log($"顯示盾牌圖標，父物件: {parentTransform.name}");
            }
            else
            {
                Debug.LogWarning("找不到敵人血量條或父物件！");
            }
        }
        
        /// <summary>
        /// 隱藏盾牌圖標
        /// </summary>
        private void HideShieldIcon()
        {
            if (currentShieldIcon != null)
            {
                Destroy(currentShieldIcon);
                currentShieldIcon = null;
                Debug.Log("隱藏盾牌圖標");
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
            
            // 禁用Board輸入
            if (board != null)
            {
                board.ToggleInput(false);
            }
            
            // 觸發UI結束畫面
            if (victory)
            {
                // 勝利
                UIHandler.Instance.ShowWin();
            }
            else
            {
                // 失敗
                UIHandler.Instance.ShowLose();
            }
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
