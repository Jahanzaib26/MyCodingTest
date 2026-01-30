using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateBar();
        if(currentHealth == 0)
        {
            Die();
        }
        Debug.Log("🔥 Player took damage! Health: " + currentHealth);
    }

    void UpdateBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;
    }

    public void Die()
    {
        gameObject.transform.position = new Vector3(149f, 87f, -9f);
    }
    

}
