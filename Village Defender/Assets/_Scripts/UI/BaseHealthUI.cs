using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;
    public TextMeshProUGUI healthText;

    [Header("Health Settings")]
    public int maxHealth = 10;
    public float smoothSpeed = 5f;

    private float targetHealth;

    void Start()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        targetHealth = maxHealth;

        UpdateColor();
        UpdateText();
    }

    void Update()
    {
        healthSlider.value = Mathf.Lerp(
            healthSlider.value,
            targetHealth,
            Time.deltaTime * smoothSpeed
        );

        UpdateColor();
    }

    public void UpdateHealth(int currentHealth)
    {
        targetHealth = currentHealth;
        UpdateText();
    }

    void UpdateColor()
    {
        float percent = healthSlider.value / maxHealth;

        if (percent > 0.6f)
            fillImage.color = Color.green;
        else if (percent > 0.3f)
            fillImage.color = new Color(1f, 0.65f, 0f); // orange
        else
            fillImage.color = Color.red;
    }

    void UpdateText()
    {
        if (healthText != null)
            healthText.text = $"Vie de la base : {Mathf.CeilToInt(targetHealth)} / {maxHealth}";
    }
}