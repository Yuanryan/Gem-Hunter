using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    /// <summary>
    /// 角色動畫控制器 - 使用Unity Animation系統控制精靈圖表動畫
    /// </summary>
    public class CharacterAnimationController : MonoBehaviour
    {
        [Header("角色精靈圖")]
        [SerializeField] private Image playerSprite;
        [SerializeField] private Image enemySprite;
        
        [Header("動畫組件")]
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Animator enemyAnimator;
        
        [Header("動畫狀態")]
        private bool isPlayerAttacking = false;
        private bool isEnemyAttacking = false;
        
        private void Start()
        {
            // 確保動畫器存在
            if (playerAnimator == null && playerSprite != null)
            {
                playerAnimator = playerSprite.GetComponent<Animator>();
            }
            
            if (enemyAnimator == null && enemySprite != null)
            {
                enemyAnimator = enemySprite.GetComponent<Animator>();
            }
            
            // 開始閒置動畫
            PlayIdleAnimations();
        }
        
        /// <summary>
        /// 播放閒置動畫
        /// </summary>
        public void PlayIdleAnimations()
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Idle");
            }
            
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Idle");
            }
        }
        
        /// <summary>
        /// 播放玩家攻擊動畫
        /// </summary>
        public void PlayPlayerAttackAnimation()
        {
            Debug.Log("CharacterAnimationController: 嘗試播放玩家攻擊動畫");
            if (playerAnimator != null && !isPlayerAttacking)
            {
                Debug.Log("CharacterAnimationController: 觸發玩家攻擊動畫");
                isPlayerAttacking = true;
                playerAnimator.SetTrigger("Attack");
                
                // 播放玩家攻擊音效
                if (GameManager.Instance.Settings.SoundSettings.PlayerAttackSound != null)
                {
                    GameManager.Instance.PlaySFX(GameManager.Instance.Settings.SoundSettings.PlayerAttackSound);
                }
                
                // 監聽動畫完成事件
                StartCoroutine(WaitForAnimationComplete("PlayerAttack", () => {
                    isPlayerAttacking = false;
                    PlayIdleAnimations();
                }));
            }
            else if (playerAnimator == null)
            {
                Debug.LogWarning("CharacterAnimationController: PlayerAnimator 未設置！");
            }
            else if (isPlayerAttacking)
            {
                Debug.Log("CharacterAnimationController: 玩家正在攻擊中，跳過");
            }
        }
        
        /// <summary>
        /// 播放敵人攻擊動畫
        /// </summary>
        public void PlayEnemyAttackAnimation()
        {
            Debug.Log("CharacterAnimationController: 嘗試播放敵人攻擊動畫");
            if (enemyAnimator != null && !isEnemyAttacking)
            {
                Debug.Log("CharacterAnimationController: 觸發敵人攻擊動畫");
                isEnemyAttacking = true;
                enemyAnimator.SetTrigger("Attack");
                
                // 播放敵人攻擊音效
                if (GameManager.Instance.Settings.SoundSettings.EnemyAttackSound != null)
                {
                    GameManager.Instance.PlaySFX(GameManager.Instance.Settings.SoundSettings.EnemyAttackSound);
                }
                
                // 監聽動畫完成事件
                StartCoroutine(WaitForAnimationComplete("EnemyAttack", () => {
                    isEnemyAttacking = false;
                    PlayIdleAnimations();
                }));
            }
            else if (enemyAnimator == null)
            {
                Debug.LogWarning("CharacterAnimationController: EnemyAnimator 未設置！");
            }
            else if (isEnemyAttacking)
            {
                Debug.Log("CharacterAnimationController: 敵人正在攻擊中，跳過");
            }
        }
        
        /// <summary>
        /// 播放玩家防禦動畫
        /// </summary>
        public void PlayPlayerDefendAnimation()
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Defend");
                
                // 播放玩家格擋音效
                if (GameManager.Instance.Settings.SoundSettings.PlayerBlockSound != null)
                {
                    GameManager.Instance.PlaySFX(GameManager.Instance.Settings.SoundSettings.PlayerBlockSound);
                }
                
                // 防禦動畫完成後回到閒置
                StartCoroutine(WaitForAnimationComplete("PlayerDefend", () => {
                    PlayIdleAnimations();
                }));
            }
        }
        
        /// <summary>
        /// 播放玩家治療動畫
        /// </summary>
        public void PlayPlayerHealAnimation()
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Heal");
                
                // 治療動畫完成後回到閒置
                StartCoroutine(WaitForAnimationComplete("PlayerHeal", () => {
                    PlayIdleAnimations();
                }));
            }
        }
        
        /// <summary>
        /// 播放敵人防禦動畫
        /// </summary>
        public void PlayEnemyDefendAnimation()
        {
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Defend");
                
                // 防禦動畫完成後回到閒置
                StartCoroutine(WaitForAnimationComplete("EnemyDefend", () => {
                    PlayIdleAnimations();
                }));
            }
        }
        
        /// <summary>
        /// 播放玩家受傷動畫
        /// </summary>
        public void PlayPlayerHurtAnimation()
        {
            Debug.Log("CharacterAnimationController: 嘗試播放玩家受傷動畫");
            if (playerAnimator != null)
            {
                Debug.Log("CharacterAnimationController: 觸發玩家受傷動畫");
                playerAnimator.SetTrigger("Hurt");
                
                // 受傷後回到閒置
                StartCoroutine(WaitForAnimationComplete("PlayerHurt", () => {
                    PlayIdleAnimations();
                }));
            }
            else
            {
                Debug.LogWarning("CharacterAnimationController: PlayerAnimator 未設置！");
            }
        }
        
        /// <summary>
        /// 播放玩家受傷動畫（檢查死亡）
        /// </summary>
        /// <param name="currentHealth">當前血量</param>
        public void PlayPlayerHurtAnimation(int currentHealth)
        {
            Debug.Log($"CharacterAnimationController: 嘗試播放玩家受傷動畫，當前血量: {currentHealth}");
            if (playerAnimator != null)
            {
                Debug.Log("CharacterAnimationController: 觸發玩家受傷動畫");
                playerAnimator.SetTrigger("Hurt");
                
                // 播放玩家受傷音效
                if (GameManager.Instance.Settings.SoundSettings.PlayerHurtSound != null)
                {
                    GameManager.Instance.PlaySFX(GameManager.Instance.Settings.SoundSettings.PlayerHurtSound);
                }
                
                // 檢查是否死亡
                if (currentHealth <= 0)
                {
                    Debug.Log("CharacterAnimationController: 玩家血量歸零，受傷後播放死亡動畫");
                    StartCoroutine(WaitForAnimationComplete("PlayerHurt", () => {
                        PlayPlayerDeathAnimation();
                    }));
                }
                else
                {
                    Debug.Log("CharacterAnimationController: 玩家受傷後回到閒置");
                    StartCoroutine(WaitForAnimationComplete("PlayerHurt", () => {
                        PlayIdleAnimations();
                    }));
                }
            }
            else
            {
                Debug.LogWarning("CharacterAnimationController: PlayerAnimator 未設置！");
            }
        }
        
        /// <summary>
        /// 播放敵人受傷動畫
        /// </summary>
        /// <summary>
        /// 播放敵人受傷動畫（檢查死亡）
        /// </summary>
        /// <param name="currentHealth">當前血量</param>
        public void PlayEnemyHurtAnimation(int currentHealth)
        {
            Debug.Log($"CharacterAnimationController: 嘗試播放敵人受傷動畫，當前血量: {currentHealth}");
            if (enemyAnimator != null)
            {
                Debug.Log("CharacterAnimationController: 觸發敵人受傷動畫");
                enemyAnimator.SetTrigger("Hurt");
                
                // 播放敵人受傷音效
                if (GameManager.Instance.Settings.SoundSettings.EnemyHurtSound != null)
                {
                    GameManager.Instance.PlaySFX(GameManager.Instance.Settings.SoundSettings.EnemyHurtSound);
                }
                
                // 檢查是否死亡
                if (currentHealth <= 0)
                {
                    Debug.Log("CharacterAnimationController: 敵人血量歸零，受傷後播放死亡動畫");
                    StartCoroutine(WaitForAnimationComplete("EnemyHurt", () => {
                        PlayEnemyDeathAnimation();
                    }));
                }
                else
                {
                    Debug.Log("CharacterAnimationController: 敵人受傷後回到閒置");
                    StartCoroutine(WaitForAnimationComplete("EnemyHurt", () => {
                        PlayIdleAnimations();
                    }));
                }
            }
            else
            {
                Debug.LogWarning("CharacterAnimationController: EnemyAnimator 未設置！");
            }
        }
        
        /// <summary>
        /// 播放玩家死亡動畫
        /// </summary>
        public void PlayPlayerDeathAnimation()
        {
            Debug.Log("CharacterAnimationController: 嘗試播放玩家死亡動畫");
            if (playerAnimator != null)
            {
                Debug.Log("CharacterAnimationController: 觸發玩家死亡動畫");
                playerAnimator.SetTrigger("Death");
                // 死亡動畫通常不回到閒置狀態
            }
            else
            {
                Debug.LogWarning("CharacterAnimationController: PlayerAnimator 未設置！");
            }
        }
        
        /// <summary>
        /// 播放敵人死亡動畫
        /// </summary>
        public void PlayEnemyDeathAnimation()
        {
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Death");
                // 死亡動畫通常不回到閒置狀態
            }
        }
        
        /// <summary>
        /// 等待動畫完成
        /// </summary>
        private System.Collections.IEnumerator WaitForAnimationComplete(string animationName, System.Action onComplete)
        {
            // 等待一幀確保動畫開始
            yield return null;
            
            // 等待動畫完成
            AnimatorStateInfo stateInfo;
            if (animationName.Contains("Player"))
            {
                stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
            }
            else
            {
                stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);
            }
            
            // 等待動畫播放完成
            yield return new WaitForSeconds(stateInfo.length);
            
            // 執行完成回調
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 停止所有動畫
        /// </summary>
        public void StopAllAnimations()
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Idle");
            }
            
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Idle");
            }
            
            isPlayerAttacking = false;
            isEnemyAttacking = false;
        }
        
        /// <summary>
        /// 手動觸發防禦動畫（未來擴展用）
        /// </summary>
        public void TriggerDefendAnimation(bool isPlayer)
        {
            if (isPlayer)
            {
                PlayPlayerDefendAnimation();
            }
            else
            {
                PlayEnemyDefendAnimation();
            }
        }
        
        /// <summary>
        /// 檢查角色是否死亡
        /// </summary>
        public bool IsPlayerDead()
        {
            if (playerAnimator != null)
            {
                AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.IsName("PlayerDeath");
            }
            return false;
        }
        
        /// <summary>
        /// 檢查敵人是否死亡
        /// </summary>
        public bool IsEnemyDead()
        {
            if (enemyAnimator != null)
            {
                AnimatorStateInfo stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.IsName("EnemyDeath");
            }
            return false;
        }
        
        /// <summary>
        /// 設置動畫速度
        /// </summary>
        /// <param name="speed">動畫速度倍數</param>
        public void SetAnimationSpeed(float speed)
        {
            if (playerAnimator != null)
            {
                playerAnimator.speed = speed;
            }
            
            if (enemyAnimator != null)
            {
                enemyAnimator.speed = speed;
            }
        }
        
        // AnimationEvent 處理函數
        /// <summary>
        /// 動畫事件處理 - 玩家攻擊
        /// </summary>
        public void OnPlayerAttackEvent()
        {
            Debug.Log("玩家攻擊動畫事件觸發");
            // 可以在這裡添加攻擊音效、特效等
        }
        
        /// <summary>
        /// 動畫事件處理 - 敵人攻擊
        /// </summary>
        public void OnEnemyAttackEvent()
        {
            Debug.Log("敵人攻擊動畫事件觸發");
            // 可以在這裡添加攻擊音效、特效等
        }
        
        /// <summary>
        /// 動畫事件處理 - 玩家受傷
        /// </summary>
        public void OnPlayerHurtEvent()
        {
            Debug.Log("玩家受傷動畫事件觸發");
            // 可以在這裡添加受傷音效、特效等
        }
        
        /// <summary>
        /// 動畫事件處理 - 敵人受傷
        /// </summary>
        public void OnEnemyHurtEvent()
        {
            Debug.Log("敵人受傷動畫事件觸發");
            // 可以在這裡添加受傷音效、特效等
        }
        
        /// <summary>
        /// 動畫事件處理 - 玩家死亡
        /// </summary>
        public void OnPlayerDeathEvent()
        {
            Debug.Log("玩家死亡動畫事件觸發");
            // 可以在這裡添加死亡音效、特效等
        }
        
        /// <summary>
        /// 動畫事件處理 - 敵人死亡
        /// </summary>
        public void OnEnemyDeathEvent()
        {
            Debug.Log("敵人死亡動畫事件觸發");
            // 可以在這裡添加死亡音效、特效等
        }
        
        /// <summary>
        /// 動畫事件處理 - 玩家防禦
        /// </summary>
        public void OnPlayerDefendEvent()
        {
            Debug.Log("玩家防禦動畫事件觸發");
            // 可以在這裡添加防禦音效、特效等
        }
        
        /// <summary>
        /// 動畫事件處理 - 敵人防禦
        /// </summary>
        public void OnEnemyDefendEvent()
        {
            Debug.Log("敵人防禦動畫事件觸發");
            // 可以在這裡添加防禦音效、特效等
        }
        
        /// <summary>
        /// 測試動畫 - 用於調試
        /// </summary>
        [ContextMenu("測試玩家攻擊動畫")]
        public void TestPlayerAttack()
        {
            Debug.Log("手動測試玩家攻擊動畫");
            PlayPlayerAttackAnimation();
        }
        
        /// <summary>
        /// 測試動畫 - 用於調試
        /// </summary>
        [ContextMenu("測試敵人攻擊動畫")]
        public void TestEnemyAttack()
        {
            Debug.Log("手動測試敵人攻擊動畫");
            PlayEnemyAttackAnimation();
        }
        
        /// <summary>
        /// 測試動畫 - 用於調試
        /// </summary>
        [ContextMenu("測試玩家受傷動畫")]
        public void TestPlayerHurt()
        {
            Debug.Log("手動測試玩家受傷動畫");
            PlayPlayerHurtAnimation(50); // 模擬受傷但未死亡
        }
        
        /// <summary>
        /// 測試動畫 - 用於調試
        /// </summary>
        [ContextMenu("測試玩家受傷死亡動畫")]
        public void TestPlayerHurtDeath()
        {
            Debug.Log("手動測試玩家受傷死亡動畫");
            PlayPlayerHurtAnimation(0); // 模擬受傷後死亡
        }
        
        /// <summary>
        /// 測試動畫 - 用於調試
        /// </summary>
        [ContextMenu("測試敵人受傷動畫")]
        public void TestEnemyHurt()
        {
            Debug.Log("手動測試敵人受傷動畫");
            PlayEnemyHurtAnimation(30); // 模擬受傷但未死亡
        }
        
        /// <summary>
        /// 測試動畫 - 用於調試
        /// </summary>
        [ContextMenu("測試敵人受傷死亡動畫")]
        public void TestEnemyHurtDeath()
        {
            Debug.Log("手動測試敵人受傷死亡動畫");
            PlayEnemyHurtAnimation(0); // 模擬受傷後死亡
        }
        
        /// <summary>
        /// 檢查動畫器狀態 - 用於調試
        /// </summary>
        [ContextMenu("檢查動畫器狀態")]
        public void CheckAnimatorStatus()
        {
            Debug.Log($"PlayerAnimator: {(playerAnimator != null ? "已設置" : "未設置")}");
            Debug.Log($"EnemyAnimator: {(enemyAnimator != null ? "已設置" : "未設置")}");
            Debug.Log($"isPlayerAttacking: {isPlayerAttacking}");
            Debug.Log($"isEnemyAttacking: {isEnemyAttacking}");
            
            if (playerAnimator != null)
            {
                Debug.Log($"PlayerAnimator 狀態: {playerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash}");
            }
            if (enemyAnimator != null)
            {
                Debug.Log($"EnemyAnimator 狀態: {enemyAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash}");
            }
        }
    }
}
