using System;
using UnityEngine;

public class HealthSystem 
{
    [Header("Config")]
    [SerializeField]private int maxHealth = 3;
    [SerializeField]private int currentHealth;

    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnDeath;

    public int CurrentHealth 
    {
        get => currentHealth;
        private set
        {
            currentHealth = Mathf.Clamp(value,0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if(currentHealth <= 0)
                OnDeath?.Invoke();
        }
    }

    public int MaxHealth 
    {
        get => maxHealth;
        private set 
        {
            maxHealth = Mathf.Max(1, value);
            CurrentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }

    public HealthSystem(int initialMaxHealth = 3) 
    {
        maxHealth = initialMaxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount) 
    {
        if (amount <= 0) return;
        CurrentHealth -= amount;
        Debug.Log($"Tomou {amount} de dano!  Vida:{currentHealth}/{maxHealth}");
    }

    public void Heal(int amount) 
    {
        if (amount <= 0) return;
        CurrentHealth += amount;
        Debug.Log($"Curou {amount}! Vida:{currentHealth}/{maxHealth}");
    }

    public void AddMaxHealth(int amount, bool healToo = true) 
    {
        if (amount == 0) return;
        MaxHealth += amount;

        if (healToo) 
        {
            CurrentHealth += amount;
        }
        Debug.Log($"Vida máxima {(amount > 0 ? "aumentou" : "diminuiu")}! Agora {maxHealth}");
    }

    public void ResetToFull()
    {
        CurrentHealth = maxHealth;
    }
    public bool isDead => currentHealth <= 0;
    public float HealthPercentage => (float)CurrentHealth / maxHealth;

}
