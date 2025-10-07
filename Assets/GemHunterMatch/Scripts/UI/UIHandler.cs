using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Match3
{
    [DefaultExecutionOrder(-9000)]
    public class UIHandler : MonoBehaviour
    {
        class UIAnimationEntry
        {
            public VisualElement UIElement;
            public Vector3 WorldPosition;
            public Vector3 StartPosition;
            public Vector3 StartToEnd;
            public float Time;

            public AnimationCurve Curve;

            //this is played when the animation reach its end position;
            public AudioClip EndClip;
        }

        class ShopEntry
        {
            public Button BuyButton;
            public ShopSetting.ShopItem LinkedItem;

            public void UpdateButtonState()
            {
                BuyButton.SetEnabled(GameManager.Instance.Coins >= LinkedItem.Price);
            }
        }

        public enum CharacterAnimation
        {
            Match,
            Win,
            LowMove,
            Lose
        }
    
        public static UIHandler Instance { get; protected set; }
    
        public VisualTreeAsset GemGoalTemplate;
        public Camera PortraitCameraPrefab;

        public Sprite CoinSprite;

        public VisualTreeAsset ShopItemEntryTemplate;
        public VisualTreeAsset BonusItemTemplate;
    
        private UIDocument m_Document;

        private VisualElement m_CoverElement;
        private Action m_FadeCallback;

        private VisualElement m_GemGoalContent;
        private Label m_MoveCounter;
        private Label m_LevelName;
        
        // 拖拽時間條相關
        private VisualElement m_DragTimerContainer;
        private VisualElement m_DragTimerBar;
        private Label m_DragTimerLabel;

        private VisualElement m_BottomBarRoot;

        private VisualElement m_SelectedBonusItem;

        private VisualElement m_EndTitleContent;
        private VisualElement m_WinTitle;
        private VisualElement m_LoseTitle;
        
        private Image m_PortraitTarget;
        
        private VisualElement m_EndScreen;

        private VisualElement m_CharacterPortrait;
        private Animator m_CharacterAnimator;
        private int m_WinTriggerID, m_MatchTriggerId, m_LowMoveTriggerId, m_LoseTriggerId;

        private Dictionary<int, Label> m_GoalCountLabelLookup = new();
        private Dictionary<int, VisualElement> m_Checkmarks = new();
        private List<UIAnimationEntry> m_CurrentGemAnimations = new();

        private float m_MatchEffectEndTime = 0.0f;

        private Camera mainCamera;
        
        // Setting Menu
        private VisualElement m_SettingMenuRoot;

        private Slider m_MainVolumeSlider;
        private Slider m_MusicVolumeSlider;
        private Slider m_SFXVolumeSlider;
    
        // End Screen
        private Label m_CoinLabel;
        private Label m_LiveLabel;
        private Label m_StarLabel;
    
        // Shop
        private VisualElement m_ShopRoot;
        private ScrollView m_ShopScrollView;

        private List<ShopEntry> m_ShopEntries = new();
    
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private class DebugGemButton
        {
            public Button Button;
            public Gem Gem;
        }
    
        private VisualElement m_DebugMenuRoot;
        private ScrollView m_DebugGemScrollView;

        private DebugGemButton m_CurrentEnabledDebugButton;

        public bool DebugMenuOpen => m_DebugMenuRoot.style.display == DisplayStyle.Flex;
        public Gem SelectedDebugGem => m_CurrentEnabledDebugButton?.Gem;
    
#endif

        private void Awake()
        {
            Instance = this;
            m_Document = GetComponent<UIDocument>();
        }

        private void Start()
        {
            // 確保只有一個EventSystem
            EventSystemManager.Instance.EnsureSingleEventSystem();
            
            m_WinTriggerID = Animator.StringToHash("Win");
            m_MatchTriggerId = Animator.StringToHash("Match");
            m_LowMoveTriggerId = Animator.StringToHash("LowMove");
            m_LoseTriggerId = Animator.StringToHash("Lose");

            m_Document.panelSettings.match = Screen.orientation == ScreenOrientation.Portrait ? 1.0f : 0.0f;

            m_LevelName = m_Document.rootVisualElement.Q<Label>("LevelName");
            
            m_GemGoalContent = m_Document.rootVisualElement.Q<VisualElement>("GoalContainer");
            m_MoveCounter = m_Document.rootVisualElement.Q<Label>("MoveCounter");
            
            // 初始化拖拽時間條 - 使用Q方法查找
            m_DragTimerContainer = m_Document.rootVisualElement.Q<VisualElement>("DragTimerContainer");
            m_DragTimerBar = m_Document.rootVisualElement.Q<VisualElement>("DragTimerBar");
            m_DragTimerLabel = m_Document.rootVisualElement.Q<Label>("DragTimerLabel");
            
            // 調試：檢查是否找到時間條元素
            if (m_DragTimerContainer == null)
            {
                Debug.LogWarning("找不到 DragTimerContainer，將使用程序化創建");
                CreateDragTimerProgrammatically();
            }
            else
            {
                Debug.Log("成功找到 DragTimerContainer");
                // 確保初始狀態是隱藏的
                m_DragTimerContainer.style.display = DisplayStyle.None;
            }
            
            m_EndTitleContent = m_Document.rootVisualElement.Q<VisualElement>("EndTitleContent");
            m_WinTitle = m_EndTitleContent.Q<VisualElement>("WinTitle");
            m_LoseTitle = m_EndTitleContent.Q<VisualElement>("LoseTitle");
            
            m_EndScreen = m_Document.rootVisualElement.Q<VisualElement>("EndScreen");

            m_CharacterPortrait = m_Document.rootVisualElement.Q<VisualElement>("MiddleTopSection");

            var playAgainButton = m_Document.rootVisualElement.Q<Button>("ReplayButton");
            if (playAgainButton != null)
            {
                Debug.Log("找到 ReplayButton，綁定事件");
                Debug.Log($"ReplayButton 位置: {playAgainButton.layout}");
                Debug.Log($"ReplayButton 是否啟用: {playAgainButton.enabledSelf}");
                Debug.Log($"ReplayButton 顯示狀態: {playAgainButton.style.display.value}");
                
                // 添加多種事件監聽器
                playAgainButton.clicked += () =>
                {
                    Debug.Log("ReplayButton clicked 事件觸發");
                    // 淡出戰鬥UI
                    var combatUIController = FindObjectOfType<CombatUIController>();
                    if (combatUIController != null)
                    {
                        combatUIController.ShowCombatUI(false);
                    }
                    FadeOut(() =>
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
                    });
                };
                
                playAgainButton.RegisterCallback<MouseDownEvent>(evt =>
                {
                    Debug.Log("ReplayButton MouseDown 事件觸發");
                });
                
                playAgainButton.RegisterCallback<MouseUpEvent>(evt =>
                {
                    Debug.Log("ReplayButton MouseUp 事件觸發");
                });
                
                playAgainButton.RegisterCallback<PointerDownEvent>(evt =>
                {
                    Debug.Log("ReplayButton PointerDown 事件觸發");
                });
            }
            else
            {
                Debug.LogError("找不到 ReplayButton");
            }

            var exitButton = m_Document.rootVisualElement.Q<Button>("SelectLevelButton");
            if (exitButton != null)
            {
                Debug.Log("找到 SelectLevelButton，綁定事件");
                Debug.Log($"SelectLevelButton 位置: {exitButton.layout}");
                Debug.Log($"SelectLevelButton 是否啟用: {exitButton.enabledSelf}");
                Debug.Log($"SelectLevelButton 顯示狀態: {exitButton.style.display.value}");
                
                exitButton.clicked += () =>
                {
                    Debug.Log("SelectLevelButton clicked 事件觸發");
                    // 淡出戰鬥UI
                    var combatUIController = FindObjectOfType<CombatUIController>();
                    if (combatUIController != null)
                    {
                        combatUIController.ShowCombatUI(false);
                    }
                    FadeOut(() =>
                    {
                        SceneManager.LoadScene(1, LoadSceneMode.Single); 
                    });
                };
                
                exitButton.RegisterCallback<MouseDownEvent>(evt =>
                {
                    Debug.Log("SelectLevelButton MouseDown 事件觸發");
                });
            }
            else
            {
                Debug.LogError("找不到 SelectLevelButton");
            }
            
            m_PortraitTarget = m_Document.rootVisualElement.Q<Image>("RenderTarget");
            m_PortraitTarget.scaleMode = ScaleMode.ScaleToFit;
            
            m_CoinLabel = m_Document.rootVisualElement.Q<Label>("CoinLabel");
            m_LiveLabel = m_Document.rootVisualElement.Q<Label>("LiveLabel");
            m_StarLabel = m_Document.rootVisualElement.Q<Label>("StarLabel");

            m_BottomBarRoot = m_Document.rootVisualElement.Q<VisualElement>("BoosterZone");
            var openSettingButton = m_BottomBarRoot.parent.Q<Button>("ButtonMenu");
            openSettingButton.clicked += () =>
            {
                ToggleSettingMenu(true);
            };
            
            // Setting Menu

            m_SettingMenuRoot = m_Document.rootVisualElement.Q<VisualElement>("Settings");
            m_SettingMenuRoot.style.display = DisplayStyle.None;

            var returnButton = m_SettingMenuRoot.Q<Button>("ReturnButton");
            returnButton.clicked += () =>
            {
                FadeOut(() =>
                {
                    ToggleSettingMenu(false);
                    SceneManager.LoadScene(1, LoadSceneMode.Single); 
                });
            };

            var closeButton = m_SettingMenuRoot.Q<Button>("CloseButton");
            closeButton.clicked += () =>
            {
                ToggleSettingMenu(false);
            };

            m_MainVolumeSlider = m_SettingMenuRoot.Q<Slider>("MainVolumeSlider");
            m_MusicVolumeSlider = m_SettingMenuRoot.Q<Slider>("MusicVolumeSlider");
            m_SFXVolumeSlider = m_SettingMenuRoot.Q<Slider>("SFXVolumeSlider");

            var soundData = GameManager.Instance.Volumes;
            m_MainVolumeSlider.value = soundData.MainVolume;
            m_MusicVolumeSlider.value = soundData.MusicVolume;
            m_SFXVolumeSlider.value = soundData.SFXVolume;

            m_MainVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                soundData.MainVolume = evt.newValue;
                GameManager.Instance.UpdateVolumes();
            });
            
            m_MusicVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                soundData.MusicVolume = evt.newValue;
                GameManager.Instance.UpdateVolumes();
            });
            
            m_SFXVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                soundData.SFXVolume = evt.newValue;
                GameManager.Instance.UpdateVolumes();
            });

            // Shop
        
            var shopButton = m_Document.rootVisualElement.Q<Button>("ShopButton");
            if (shopButton != null)
            {
                Debug.Log("找到 ShopButton，綁定事件");
                Debug.Log($"ShopButton 位置: {shopButton.layout}");
                Debug.Log($"ShopButton 是否啟用: {shopButton.enabledSelf}");
                Debug.Log($"ShopButton 顯示狀態: {shopButton.style.display.value}");
                
                shopButton.clicked += () =>
                {
                    Debug.Log("ShopButton clicked 事件觸發");
                    ShowShop(true);
                };
                
                shopButton.RegisterCallback<MouseDownEvent>(evt =>
                {
                    Debug.Log("ShopButton MouseDown 事件觸發");
                });
            }
            else
            {
                Debug.LogError("找不到 ShopButton");
            }

            m_ShopRoot = m_Document.rootVisualElement.Q<VisualElement>("Shop");
            m_ShopScrollView = m_Document.rootVisualElement.Q<ScrollView>("ShopContentScroll");

            foreach (var shopItem in GameManager.Instance.Settings.ShopSettings.Items)
            {
                var newElem = ShopItemEntryTemplate.Instantiate();
                var itemIcon = newElem.Q<VisualElement>("ItemIcon");
                var itemName = newElem.Q<Label>("ItemName");
                var itemPrice = newElem.Q<Label>("ItemPrice");

                itemIcon.style.backgroundImage = new StyleBackground(shopItem.ItemSprite);
                itemName.text = shopItem.ItemName;
                itemPrice.text = shopItem.Price.ToString();
         
                var newShopEntry = new ShopEntry();
            
                newShopEntry.BuyButton = newElem.Q<Button>("BuyButton");
                newShopEntry.LinkedItem = shopItem;
                newShopEntry.UpdateButtonState();
                newShopEntry.BuyButton.clicked += () =>
                {
                    newShopEntry.LinkedItem.Buy();
                    GameManager.Instance.ChangeCoins(-newShopEntry.LinkedItem.Price);
                
                    UpdateTopBarData();
                    UpdateShopEntry();
                };
              
                m_ShopEntries.Add(newShopEntry);
                m_ShopScrollView.Add(newElem);
            }

            var exitShop = m_ShopRoot.Q<Button>("ShopExitButton");
            exitShop.clicked += () =>
            {
                ShowShop(false);
            };
            
            var curve = GameManager.Instance.Settings.VisualSettings.MatchFlyCurve;
            m_MatchEffectEndTime = curve.keys[curve.keys.Length-1].time;

            m_CoverElement = m_Document.rootVisualElement.Q<VisualElement>("Cover");
            m_CoverElement.style.opacity = 1.0f;
            m_CoverElement.RegisterCallback<TransitionEndEvent>(evt =>
            {
                m_FadeCallback?.Invoke();
                m_FadeCallback = null;
            });
            
            // 添加全局點擊事件監聽器用於調試
            m_Document.rootVisualElement.RegisterCallback<MouseDownEvent>(evt =>
            {
                Debug.Log($"全局 MouseDown 事件在位置: {evt.mousePosition}");
                Debug.Log($"目標元素: {evt.target}");
            });
            
            m_Document.rootVisualElement.RegisterCallback<PointerDownEvent>(evt =>
            {
                Debug.Log($"全局 PointerDown 事件在位置: {evt.position}");
                Debug.Log($"目標元素: {evt.target}");
            });
            
            CreateBottomBar();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            m_DebugMenuRoot = new VisualElement();
            m_DebugMenuRoot.name = "DebugRoot";
        
            m_Document.rootVisualElement.Add(m_DebugMenuRoot);

            m_DebugMenuRoot.style.position = Position.Absolute;
            m_DebugMenuRoot.style.top = Length.Percent(85);
            m_DebugMenuRoot.style.left = 0;
            m_DebugMenuRoot.style.right = 0;
            m_DebugMenuRoot.style.bottom = 0;
            m_DebugMenuRoot.style.backgroundColor = Color.black;

            m_DebugGemScrollView = new ScrollView();
            m_DebugGemScrollView.mode = ScrollViewMode.Horizontal;
            m_DebugGemScrollView.style.flexDirection = FlexDirection.Row;
        
            m_DebugMenuRoot.Add(m_DebugGemScrollView);
        
            ToggleDebugMenu();
#endif
            
            ApplySafeArea(m_Document.rootVisualElement.Q<VisualElement>("FullContent"));
            ApplySafeArea(m_EndScreen);
        }
    
        public void Init()
        {
            m_LevelName.text = LevelData.Instance.LevelName;
            
            m_WinTitle.style.scale = Vector2.zero;
            m_LoseTitle.style.scale = Vector2.zero;

            m_EndTitleContent.style.display = DisplayStyle.None;
            m_EndScreen.style.display = DisplayStyle.None;
            
            // 重新初始化 Cover 元素
            if (m_CoverElement != null)
            {
                m_CoverElement.style.opacity = 1.0f;
                m_CoverElement.style.display = DisplayStyle.Flex;
                // 恢復原始的過渡時間
                m_CoverElement.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(2.0f, TimeUnit.Second) });
            }
            
            // 重新初始化 FullContent
            var fullContent = m_Document.rootVisualElement.Q<VisualElement>("FullContent");
            if (fullContent != null)
            {
                fullContent.style.display = DisplayStyle.Flex;
            }
            
            //we clear the goal container as when we reload a level, there 
            m_GemGoalContent.Clear();
            foreach (var goal in LevelData.Instance.Goals)
            {
                var newInstance = GemGoalTemplate.Instantiate();
                m_GemGoalContent.Add(newInstance);

                var label = newInstance.Q<Label>("GemGoalCount");
                label.text = goal.Count.ToString();

                var checkmark = newInstance.Q<VisualElement>("Checkmark");
                checkmark.style.display = DisplayStyle.None;

                var background = newInstance.Q<VisualElement>("GoalGemTemplate");
                background.style.backgroundImage =
                    new StyleBackground(goal.Gem.UISprite);

                m_GoalCountLabelLookup[goal.Gem.GemType] = label;
                m_Checkmarks[goal.Gem.GemType] = checkmark;
            }

            LevelData.Instance.OnGoalChanged += (type, amount) =>
            {
                if (amount == 0)
                {
                    m_GoalCountLabelLookup[type].style.display = DisplayStyle.None;
                    m_Checkmarks[type].style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_GoalCountLabelLookup[type].style.display = DisplayStyle.Flex;
                    m_Checkmarks[type].style.display = DisplayStyle.None;
                    
                    m_GoalCountLabelLookup[type].text = amount.ToString();
                }
            };

            m_MoveCounter.text = LevelData.Instance.RemainingMove.ToString();
            LevelData.Instance.OnMoveHappened += remaining =>
            {
                m_MoveCounter.text = remaining.ToString();
            };

           
            var charInst = Instantiate(PortraitCameraPrefab, new Vector3(-100, -100, 0), Quaternion.identity);

            m_CharacterAnimator = charInst.GetComponentInChildren<Animator>();

            m_PortraitTarget.image = charInst.targetTexture;

            mainCamera = Camera.main;

            m_ShopRoot.style.display = DisplayStyle.None;
        }

        public void Display(bool displayed)
        {
            m_Document.rootVisualElement.style.display = displayed ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public static void ApplySafeArea(VisualElement root)
        {
            Rect safeArea = Screen.safeArea;

            // Calculate borders based on safe area rect
            var left = safeArea.x;
            var right = Screen.width - safeArea.xMax;
            var top = Screen.height - safeArea.yMax;
            var bottom = safeArea.y;
            
            // Set border widths regardless of orientation
            root.style.top = top;
            root.style.bottom = bottom;
            root.style.left = left;
            root.style.right = right;
        }

        public void ShowEnd()
        {
            UpdateTopBarData();
            
            ShowLose();
        }

        public void ShowWin()
        {
            GameManager.Instance.WinTriggered();
            TriggerCharacterAnimation(CharacterAnimation.Win);
            
            // 播放敵人死亡動畫
            var characterAnimations = FindObjectOfType<CharacterAnimationController>();
            if (characterAnimations != null)
            {
                characterAnimations.PlayEnemyDeathAnimation();
            }
            
            // 隱藏主要內容，避免阻擋勝利畫面的按鈕點擊
            var fullContent = m_Document.rootVisualElement.Q<VisualElement>("FullContent");
            if (fullContent != null)
            {
                fullContent.style.display = DisplayStyle.None;
            }
            
            // 確保 Cover 元素不會阻擋按鈕點擊
            if (m_CoverElement != null)
            {
                m_CoverElement.style.opacity = 0.0f;
                m_CoverElement.style.display = DisplayStyle.None;
            }
            
            m_EndTitleContent.style.display = DisplayStyle.Flex;
            m_LoseTitle.style.display = DisplayStyle.None;
            m_WinTitle.style.display = DisplayStyle.Flex;
            m_WinTitle.style.scale = Vector2.one;
            
            // 確保標題元素不會阻擋按鈕點擊
            m_EndTitleContent.pickingMode = PickingMode.Ignore;
            m_WinTitle.pickingMode = PickingMode.Ignore;
            
            // 也為 Title 元素設定 pickingMode
            var titleElement = m_EndTitleContent.Q<VisualElement>("Title");
            if (titleElement != null)
            {
                titleElement.pickingMode = PickingMode.Ignore;
            }

            StartCoroutine(ShowEndControl(GameManager.Instance.Settings.SoundSettings.WinSound));
        }

        public void ShowLose()
        {
            GameManager.Instance.LooseTriggered();
            TriggerCharacterAnimation(CharacterAnimation.Lose);
            
            // 播放玩家死亡動畫
            var characterAnimations = FindObjectOfType<CharacterAnimationController>();
            if (characterAnimations != null)
            {
                Debug.Log("UIHandler: 找到 CharacterAnimationController，播放玩家死亡動畫");
                characterAnimations.PlayPlayerDeathAnimation();
            }
            else
            {
                Debug.LogWarning("UIHandler: 找不到 CharacterAnimationController！");
            }
            
            // 隱藏主要內容，避免阻擋失敗畫面的按鈕點擊
            var fullContent = m_Document.rootVisualElement.Q<VisualElement>("FullContent");
            if (fullContent != null)
            {
                fullContent.style.display = DisplayStyle.None;
            }
            
            // 確保 Cover 元素不會阻擋按鈕點擊
            if (m_CoverElement != null)
            {
                m_CoverElement.style.opacity = 0.0f;
                m_CoverElement.style.display = DisplayStyle.None;
            }
            
            m_EndTitleContent.style.display = DisplayStyle.Flex;
            m_WinTitle.style.display = DisplayStyle.None;
            m_LoseTitle.style.display = DisplayStyle.Flex;
            m_LoseTitle.style.scale = Vector2.one;
            
            // 確保標題元素不會阻擋按鈕點擊
            m_EndTitleContent.pickingMode = PickingMode.Ignore;
            m_LoseTitle.pickingMode = PickingMode.Ignore;
            
            // 也為 Title 元素設定 pickingMode
            var titleElement = m_EndTitleContent.Q<VisualElement>("Title");
            if (titleElement != null)
            {
                titleElement.pickingMode = PickingMode.Ignore;
            }
            
            // 額外確保 LoseTitle 不會阻擋點擊事件
            m_LoseTitle.pickingMode = PickingMode.Ignore;
            // 限制 LoseTitle 的大小，避免覆蓋按鈕區域
            m_LoseTitle.style.flexGrow = 0;
            m_LoseTitle.style.height = Length.Percent(50);
            
            // 調試：檢查失敗畫面顯示時的狀態
            Debug.Log("=== 失敗畫面顯示時的狀態 ===");
            Debug.Log($"EndTitleContent pickingMode: {m_EndTitleContent.pickingMode}");
            Debug.Log($"LoseTitle pickingMode: {m_LoseTitle.pickingMode}");
            Debug.Log($"LoseTitle 顯示狀態: {m_LoseTitle.style.display.value}");
            Debug.Log($"LoseTitle 位置: {m_LoseTitle.layout}");

            StartCoroutine(ShowEndControl(GameManager.Instance.Settings.SoundSettings.LooseSound));
        }

        IEnumerator ShowEndControl(AudioClip clip)
        {
            yield return new WaitForSeconds(3.0f);
            
            GameManager.Instance.PlaySFX(clip);
            
            UpdateTopBarData();
            m_EndScreen.style.display = DisplayStyle.Flex;
            
            // 調試：檢查按鈕在勝利畫面顯示時的狀態
            Debug.Log("=== 勝利畫面顯示時的按鈕狀態 ===");
            var replayButton = m_Document.rootVisualElement.Q<Button>("ReplayButton");
            var selectLevelButton = m_Document.rootVisualElement.Q<Button>("SelectLevelButton");
            var shopButton = m_Document.rootVisualElement.Q<Button>("ShopButton");
            
            if (replayButton != null)
            {
                Debug.Log($"ReplayButton 在勝利畫面顯示時的位置: {replayButton.layout}");
                Debug.Log($"ReplayButton 在勝利畫面顯示時是否啟用: {replayButton.enabledSelf}");
                Debug.Log($"ReplayButton 在勝利畫面顯示時的顯示狀態: {replayButton.style.display.value}");
            }
            
            if (selectLevelButton != null)
            {
                Debug.Log($"SelectLevelButton 在勝利畫面顯示時的位置: {selectLevelButton.layout}");
                Debug.Log($"SelectLevelButton 在勝利畫面顯示時是否啟用: {selectLevelButton.enabledSelf}");
                Debug.Log($"SelectLevelButton 在勝利畫面顯示時的顯示狀態: {selectLevelButton.style.display.value}");
            }
            
            if (shopButton != null)
            {
                Debug.Log($"ShopButton 在勝利畫面顯示時的位置: {shopButton.layout}");
                Debug.Log($"ShopButton 在勝利畫面顯示時是否啟用: {shopButton.enabledSelf}");
                Debug.Log($"ShopButton 在勝利畫面顯示時的顯示狀態: {shopButton.style.display.value}");
            }
            
            // 檢查 EndScreen 的狀態
            Debug.Log($"EndScreen 顯示狀態: {m_EndScreen.style.display.value}");
            Debug.Log($"EndScreen 位置: {m_EndScreen.layout}");
            
            // 檢查 Cover 元素的狀態
            if (m_CoverElement != null)
            {
                Debug.Log($"Cover 元素透明度: {m_CoverElement.style.opacity.value}");
                Debug.Log($"Cover 元素顯示狀態: {m_CoverElement.style.display.value}");
                
                // 確保 Cover 元素不會接收點擊事件
                m_CoverElement.style.display = DisplayStyle.None;
            }
        }

        public void ToggleSettingMenu(bool display)
        {
            m_SettingMenuRoot.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            GameManager.Instance.Board.ToggleInput(!display);

            if (!display)
            {
                GameManager.Instance.SaveSoundData();
            }
        }

        public void FadeIn(Action onFadeFinished)
        {
            // 設定更快的過渡時間用於重新開始和選擇關卡
            m_CoverElement.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.5f, TimeUnit.Second) });
            m_CoverElement.style.opacity = 0.0f;
            m_CoverElement.style.display = DisplayStyle.Flex; // 確保 Cover 元素在淡入時可見
            m_FadeCallback += onFadeFinished;
        }

        public void FadeOut(Action onFadeFinished)
        {
            // 設定更快的過渡時間用於重新開始和選擇關卡
            m_CoverElement.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.5f, TimeUnit.Second) });
            m_CoverElement.style.opacity = 1.0f;
            m_CoverElement.style.display = DisplayStyle.Flex; // 確保 Cover 元素在淡出時可見
            m_FadeCallback += onFadeFinished;
        }

        void SetCoverOpacityNoTransition(float value)
        {
            m_CoverElement.style.opacity = 1.0f;
            m_CoverElement.AddToClassList("no-transition");
        }

        public void AddMatchEffect(Gem gem)
        {
            var elem = new Image();
        
            m_Document.rootVisualElement.Add(elem);
        
            elem.style.position = Position.Absolute;
        
            elem.sprite = gem.UISprite;

            var worldPosition = gem.transform.position;
            var pos = RuntimePanelUtils.CameraTransformWorldToPanel(m_Document.rootVisualElement.panel, 
                worldPosition,
                mainCamera);

            var label = m_GoalCountLabelLookup[gem.GemType];
            var target = (Vector2)label.LocalToWorld(label.transform.position);

            elem.style.left = pos.x;
            elem.style.top = pos.y;
            elem.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
        
            m_CurrentGemAnimations.Add(new UIAnimationEntry()
            {
                Time = 0.0f,
                WorldPosition = worldPosition,
                StartPosition = pos,
                StartToEnd = target - pos,
                UIElement = elem,
                Curve = null
            });
        }

        public void AddCoin(Vector3 startPoint)
        {
            var elem = new Image();
        
            m_Document.rootVisualElement.Add(elem);
        
            elem.style.position = Position.Absolute;
            elem.sprite = CoinSprite;
        
            var pos = RuntimePanelUtils.CameraTransformWorldToPanel(m_Document.rootVisualElement.panel, 
                startPoint,
                mainCamera);

            var target = m_CharacterPortrait.LocalToWorld(
                m_CharacterPortrait.transform.position
                + new Vector3(m_CharacterPortrait.contentRect.width * 0.5f, m_CharacterPortrait.contentRect.height * 0.5f, 0));
        
            elem.style.left = pos.x;
            elem.style.top = pos.y;
            elem.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
        
            m_CurrentGemAnimations.Add(new UIAnimationEntry()
            {
                Time = 0.0f,
                WorldPosition = startPoint,
                StartPosition = pos,
                StartToEnd = target - pos,
                UIElement = elem,
                EndClip = GameManager.Instance.Settings.SoundSettings.CoinSound,
                Curve = GameManager.Instance.Settings.VisualSettings.CoinFlyCurve
            });
        }

        public void ShowShop(bool opened)
        {
            m_ShopRoot.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Update()
        {
            var matchCurve = GameManager.Instance.Settings.VisualSettings.MatchFlyCurve;
        
            for (int i = 0; i < m_CurrentGemAnimations.Count; ++i)
            {
                var anim = m_CurrentGemAnimations[i];

                anim.Time += Time.deltaTime;
                
                Vector3 panelVector = Vector3.zero;
                if (anim.Curve != null)
                {
                    var startToEnd = (Vector3.up * 20) - anim.WorldPosition;
                    Vector3 perpendicular;
                    var angle = Vector3.SignedAngle(Vector3.up, startToEnd, Vector3.forward);
                    if (angle < 0)
                        perpendicular = (Quaternion.AngleAxis(-angle, Vector3.forward) * Vector3.left).normalized;
                    else
                        perpendicular = (Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right).normalized;

                    float angleAmount = Mathf.Clamp01(Mathf.Abs(angle) / 10.0f);

                    float amount = anim.Curve.Evaluate(anim.Time) * angleAmount;
                    perpendicular *= amount;
                    
                    //we need the length of that vector in the panel space, so we add this perpendicular to the world start
                    //point then transform the point into the panel
                    var worldPos = anim.WorldPosition + perpendicular;
                    var panelPos = (Vector3)RuntimePanelUtils.CameraTransformWorldToPanel(m_Document.rootVisualElement.panel, worldPos,
                        mainCamera);

                    panelVector = panelPos - anim.StartPosition;
                }

                //var newPos = Vector2.Lerp(anim.StartPosition, anim.EndPosition, anim.Time);
                var newPos = anim.StartPosition + anim.StartToEnd * matchCurve.Evaluate(anim.Time) + panelVector;

                if (anim.Time >= m_MatchEffectEndTime)
                {
                    anim.UIElement.RemoveFromHierarchy();
                    m_CurrentGemAnimations.RemoveAt(i);
                    i--;

                    if(anim.EndClip != null)
                        GameManager.Instance.PlaySFX(anim.EndClip);
                }
                else
                {
                    anim.UIElement.style.left = newPos.x;
                    anim.UIElement.style.top = newPos.y;
                }
            }
        }

        public void UpdateTopBarData()
        {
            m_CoinLabel.text = GameManager.Instance.Coins.ToString();
            m_LiveLabel.text = GameManager.Instance.Lives.ToString();
            m_StarLabel.text = GameManager.Instance.Stars.ToString();
        }

        public void CreateBottomBar()
        {
            int currentBonus = 0;
            foreach (var child in m_BottomBarRoot.Children())
            {
                var icon = child.Q<VisualElement>("ImageBooster");
                var bonusButton = child.Q<Button>("ButtonBooster");
                
                if (currentBonus < GameManager.Instance.BonusItems.Count)
                {
                    var item = GameManager.Instance.BonusItems[currentBonus];
                    
                    icon.style.display = DisplayStyle.Flex;
                    icon.style.backgroundImage = Background.FromSprite(item.Item.DisplaySprite);
                    
                    bonusButton.clicked += () =>
                    {
                        var currentSelected = m_SelectedBonusItem;
                        DeselectBonusItem();

                        //clicking back on an already selected item just deselect it
                        if (currentSelected == child)
                        {
                            GameManager.Instance.ActivateBonusItem(null);
                            return;
                        }
                        
                        m_SelectedBonusItem = child;
                        m_SelectedBonusItem.AddToClassList("selected");

                        GameManager.Instance.ActivateBonusItem(item.Item);
                    };
                }
                else
                {
                    icon.style.display = DisplayStyle.None;
                }

                currentBonus++;
            }
            
            UpdateBottomBar();
        }

        public void UpdateBottomBar()
        {
            int currentBonus = 0;
            foreach (var child in m_BottomBarRoot.Children())
            {
                var count = child.Q<Label>("LabelBoosterNumber");
                var bonusButton = child.Q<Button>("ButtonBooster");

                if (currentBonus < GameManager.Instance.BonusItems.Count)
                {
                    var item = GameManager.Instance.BonusItems[currentBonus];
                    count.text = item.Amount.ToString();
                    
                    bonusButton.SetEnabled(item.Amount != 0);
                }

                currentBonus += 1;
            }
        }

        public void DeselectBonusItem()
        {
            if (m_SelectedBonusItem == null) return;
        
            GameManager.Instance.ActivateBonusItem(null);
            m_SelectedBonusItem.RemoveFromClassList("selected");
            m_SelectedBonusItem = null;
        }

        public void UpdateShopEntry()
        {
            foreach (var shopEntry in m_ShopEntries)
            {
                shopEntry.UpdateButtonState();
            }
        }

        public void TriggerCharacterAnimation(CharacterAnimation animation)
        {
            if (m_CharacterAnimator != null)
            {
                int trigger;
                switch (animation)
                {
                    case CharacterAnimation.Match:
                        trigger = m_MatchTriggerId;
                        break;
                    case CharacterAnimation.Win:
                        trigger = m_WinTriggerID;
                        break;
                    case CharacterAnimation.LowMove:
                        trigger = m_LowMoveTriggerId;
                        break;
                    case CharacterAnimation.Lose:
                        trigger = m_LoseTriggerId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(animation), animation, null);
                }
            
                m_CharacterAnimator.SetTrigger(trigger);
            }
        }

        // 程序化創建時間條（備用方案）
        void CreateDragTimerProgrammatically()
        {
            Debug.Log("使用程序化創建時間條");
            
             // 創建時間條容器
             m_DragTimerContainer = new VisualElement();
             m_DragTimerContainer.name = "DragTimerContainer";
             m_DragTimerContainer.style.position = Position.Absolute;
             m_DragTimerContainer.style.top = 50;
             m_DragTimerContainer.style.right = 250;
             m_DragTimerContainer.style.width = 1200;
             m_DragTimerContainer.style.height = 100;
             m_DragTimerContainer.style.backgroundColor = new Color(1, 0, 0, 0.8f); // 改為紅色，更容易看到
             m_DragTimerContainer.style.paddingLeft = 10;
             m_DragTimerContainer.style.paddingRight = 10;
             m_DragTimerContainer.style.paddingTop = 5;
             m_DragTimerContainer.style.paddingBottom = 5;
             m_DragTimerContainer.style.flexDirection = FlexDirection.Row;
             m_DragTimerContainer.style.alignItems = Align.Center;
             m_DragTimerContainer.style.display = DisplayStyle.None;
             
             Debug.Log($"時間條容器創建完成，位置: top={m_DragTimerContainer.style.top}, right={m_DragTimerContainer.style.right}");

            // 創建時間條背景
            var timerBackground = new VisualElement();
            timerBackground.style.width = Length.Percent(100);
            timerBackground.style.height = 100;
            timerBackground.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

             // 創建時間條進度
             m_DragTimerBar = new VisualElement();
             m_DragTimerBar.name = "DragTimerBar";
             m_DragTimerBar.style.width = Length.Percent(100);
             m_DragTimerBar.style.height = 100;
             m_DragTimerBar.style.backgroundColor = new Color(0, 1, 0, 1f); // 改為亮綠色，更容易看到


            // 組裝UI元素
            timerBackground.Add(m_DragTimerBar);
            m_DragTimerContainer.Add(timerBackground);

             // 添加到根元素
             m_Document.rootVisualElement.Add(m_DragTimerContainer);
             m_DragTimerContainer.style.display = DisplayStyle.Flex;

        }

        // 拖拽時間條控制方法
        public void ShowDragTimer()
        {
            if (m_DragTimerContainer != null)
            {
                m_DragTimerContainer.style.display = DisplayStyle.Flex;
                Debug.Log("顯示時間條");
            }
            else
            {
                Debug.LogWarning("時間條容器為 null，無法顯示");
            }
        }

        public void HideDragTimer()
        {
            if (m_DragTimerContainer != null)
            {
                m_DragTimerContainer.style.display = DisplayStyle.None;
            }
        }

        public void UpdateDragTimer(float remainingTime, float maxTime)
        {
            if (m_DragTimerBar == null)
                return;

            // 更新進度條
            float progress = remainingTime / maxTime;
            m_DragTimerBar.style.width = Length.Percent(progress * 100);

            // 更新顏色（最後1秒變紅色）
            if (remainingTime <= 1.0f)
            {
                m_DragTimerBar.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            }
            else
            {
                m_DragTimerBar.style.backgroundColor = new Color(0, 1, 0, 1f); // 亮綠色
            }

            
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        public void RegisterGemToDebug(Gem gem)
        {
            var button = new Button();

            button.clicked += () =>
            {
                if (m_CurrentEnabledDebugButton != null)
                {
                    m_CurrentEnabledDebugButton.Button.SetEnabled(true);
                }

                m_CurrentEnabledDebugButton = button.userData as DebugGemButton;
                button.SetEnabled(false);
            };

            button.userData = new DebugGemButton()
            {
                Button = button,
                Gem = gem
            };

            button.style.width = 100;

            var icone = new Image();
            icone.sprite = gem.GetComponentInChildren<SpriteRenderer>().sprite;
            icone.style.width = Length.Percent(100);
            icone.style.height = Length.Percent(100);
        
            button.Add(icone);
        
            m_DebugGemScrollView.Add(button);
        }

        public void ToggleDebugMenu()
        {
            if (m_DebugMenuRoot.style.display == DisplayStyle.None)
                m_DebugMenuRoot.style.display = DisplayStyle.Flex;
            else
                m_DebugMenuRoot.style.display = DisplayStyle.None;
        }
#endif
    }
}