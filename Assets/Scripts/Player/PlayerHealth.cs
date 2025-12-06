using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    public TextMeshProUGUI healthText;

    public int maxHealth = 100;
    public int currentHealth;

    [Header("Damage Screen")]
    public GameObject UIDamageScreen;
    public float damageFadeDuration = 0.5f;

    private Image damageImage;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        if (UIDamageScreen != null)
        {
            UIDamageScreen.SetActive(true);

            damageImage = UIDamageScreen.GetComponentInChildren<Image>(true);

            if (damageImage != null)
            {
                Color c = damageImage.color;
                damageImage.color = new Color(c.r, c.g, c.b, 0f);
            }

            UIDamageScreen.SetActive(false);
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (damageImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator DamageFlash()
    {
        UIDamageScreen.SetActive(true);

        Color c = damageImage.color;
        damageImage.color = new Color(c.r, c.g, c.b, 1f);

        float t = 0f;
        while (t < damageFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / damageFadeDuration);
            damageImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        damageImage.color = new Color(c.r, c.g, c.b, 0f);
        UIDamageScreen.SetActive(false);
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = "Player Health: " + currentHealth;
    }

    private void Die()
    {
        GameManager.TriggerGameOver();
    }
}