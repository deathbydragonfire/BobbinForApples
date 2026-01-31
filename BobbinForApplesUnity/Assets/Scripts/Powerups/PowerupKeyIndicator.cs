using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Powerups
{
    public class PowerupKeyIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Transform iconContainer;
        [SerializeField] private PowerupUI powerupUI;

        [Header("Text Settings")]
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float fontSize = 36f;
        [SerializeField] private Color textColor = Color.white;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 textOffset = new Vector2(10f, 0f);
        [SerializeField] private Vector2 iconOffset = new Vector2(10f, 0f);
        [SerializeField] private Vector2 iconSize = new Vector2(32f, 32f);
        [SerializeField] private float spacingFromLastIcon = 15f;

        private RectTransform textRectTransform;
        private RectTransform iconRectTransform;
        private RectTransform containerRectTransform;

        private void Awake()
        {
            if (keyText != null)
            {
                textRectTransform = keyText.GetComponent<RectTransform>();
            }
            if (iconImage != null)
            {
                iconRectTransform = iconImage.GetComponent<RectTransform>();
            }
            containerRectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ApplySettings();
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (iconContainer == null || containerRectTransform == null)
            {
                return;
            }

            int iconCount = iconContainer.childCount;
            
            if (iconCount == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            Transform lastIcon = iconContainer.GetChild(iconCount - 1);
            RectTransform lastIconRect = lastIcon.GetComponent<RectTransform>();
            
            if (lastIconRect != null)
            {
                float lastIconRightEdge = lastIconRect.anchoredPosition.x + (lastIconRect.sizeDelta.x * 0.5f);
                float newXPos = lastIconRightEdge + spacingFromLastIcon;
                
                containerRectTransform.anchoredPosition = new Vector2(newXPos, containerRectTransform.anchoredPosition.y);
            }
        }

        public void ApplySettings()
        {
            if (keyText != null)
            {
                if (font != null)
                {
                    keyText.font = font;
                }
                keyText.fontSize = fontSize;
                keyText.color = textColor;
            }

            if (textRectTransform != null)
            {
                textRectTransform.anchoredPosition = textOffset;
            }

            if (iconRectTransform != null)
            {
                iconRectTransform.anchoredPosition = iconOffset;
                iconRectTransform.sizeDelta = iconSize;
            }
        }

        public void SetFont(TMP_FontAsset newFont)
        {
            font = newFont;
            if (keyText != null)
            {
                keyText.font = newFont;
            }
        }

        public void SetFontSize(float newSize)
        {
            fontSize = newSize;
            if (keyText != null)
            {
                keyText.fontSize = newSize;
            }
        }

        public void SetTextColor(Color newColor)
        {
            textColor = newColor;
            if (keyText != null)
            {
                keyText.color = newColor;
            }
        }

        public void SetTextOffset(Vector2 offset)
        {
            textOffset = offset;
            if (textRectTransform != null)
            {
                textRectTransform.anchoredPosition = offset;
            }
        }

        public void SetIconOffset(Vector2 offset)
        {
            iconOffset = offset;
            if (iconRectTransform != null)
            {
                iconRectTransform.anchoredPosition = offset;
            }
        }

        public void SetIconSize(Vector2 size)
        {
            iconSize = size;
            if (iconRectTransform != null)
            {
                iconRectTransform.sizeDelta = size;
            }
        }

        public void SetSpacing(float spacing)
        {
            spacingFromLastIcon = spacing;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplySettings();
            }
        }
    }
}
