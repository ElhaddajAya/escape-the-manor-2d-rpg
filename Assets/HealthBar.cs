using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    // Initialise la barre de vie avec les bonnes valeurs
    public void SetMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    // Met à jour la valeur de la barre
    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
