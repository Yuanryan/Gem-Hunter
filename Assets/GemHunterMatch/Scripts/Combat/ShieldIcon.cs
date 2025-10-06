using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    /// <summary>
    /// 盾牌圖標組件 - 用於顯示敵人血量鎖定狀態
    /// </summary>
    public class ShieldIcon : MonoBehaviour
    {
        [Header("盾牌設定")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private Color shieldColor = Color.red;
        
        private Image shieldImage;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private float time;
        
        private void Awake()
        {
            Debug.Log("ShieldIcon: Awake() 被調用");
            
            // 創建盾牌圖像
            CreateShieldImage();
            
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            
            // 設置初始位置和大小
            rectTransform.anchoredPosition = new Vector2(0, 70);
            rectTransform.sizeDelta = new Vector2(300, 300); // 60x60 像素的圓形
            
            Debug.Log($"ShieldIcon: 盾牌創建完成，位置: {rectTransform.anchoredPosition}, 大小: {rectTransform.sizeDelta}");
        }
        
        private void CreateShieldImage()
        {
            Debug.Log("ShieldIcon: 開始創建盾牌圖像");
            
            // 添加 Image 組件
            shieldImage = gameObject.AddComponent<Image>();
            
            // 創建圓形精靈
            Texture2D circleTexture = CreateCircleTexture(64, shieldColor);
            Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            
            shieldImage.sprite = circleSprite;
            shieldImage.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, 0.8f);
            
            Debug.Log($"ShieldIcon: 盾牌圖像創建完成，顏色: {shieldImage.color}");
        }
        
        private Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.4f; // 圓形半徑
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pixelPos = new Vector2(x, y);
                    float distance = Vector2.Distance(pixelPos, center);
                    
                    if (distance <= radius)
                    {
                        // 圓形內部
                        pixels[y * size + x] = color;
                    }
                    else if (distance <= radius + 2)
                    {
                        // 圓形邊緣（抗鋸齒）
                        float alpha = 1f - (distance - radius) / 2f;
                        pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha);
                    }
                    else
                    {
                        // 透明
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }
        
        private void Update()
        {
            // 脈衝動畫
            time += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(time) * (pulseScale - 1f) * 0.5f;
            rectTransform.localScale = originalScale * pulse;
            
            // 旋轉動畫
            rectTransform.rotation = Quaternion.Euler(0, 0, time * 30f);
        }
        
        private void OnDestroy()
        {
            // 清理紋理
            if (shieldImage != null && shieldImage.sprite != null)
            {
                DestroyImmediate(shieldImage.sprite.texture);
                DestroyImmediate(shieldImage.sprite);
            }
        }
    }
}
