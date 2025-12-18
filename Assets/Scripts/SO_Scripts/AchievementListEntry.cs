using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementListEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI iconText; // emoji fallback

    // Volá Manager po vytvoøení instance
    public void Setup(AchievementService.Achievement achievement, AchievementIconDatabase iconDb)
    {
        if (nameText != null) nameText.text = achievement.name;
        if (descriptionText != null) descriptionText.text = achievement.description;

        Sprite sprite = iconDb != null ? iconDb.GetSprite(achievement.id) : null;
        if (sprite == null)
            sprite = Resources.Load<Sprite>($"Icons/{achievement.id}");

        if (sprite != null && iconImage != null)
        {
            iconImage.sprite = sprite;
            iconImage.preserveAspect = true;
            iconImage.gameObject.SetActive(true);
            if (iconText != null) iconText.gameObject.SetActive(false);
        }
        else
        {
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            if (iconText != null)
            {
                iconText.text = achievement.icon ?? "";
                iconText.gameObject.SetActive(true);
            }
        }
    }
}