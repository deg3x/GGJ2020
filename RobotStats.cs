using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotStats : MonoBehaviour
{
    public float health;
    public float fuel;
    public float fuelPerSec;
    public Image healthUI;
    public Image fuelUI;
    public GameObject deathUI;

    public AudioClip powerUp;
    public AudioClip fire;

    public float maximumFuel;
    public float maximumHealth;

    private AudioSource audio;

    void Start()
    {
        audio = this.GetComponent<AudioSource>();
        maximumFuel = fuel;
        maximumHealth = health;
    }
    
    void Update()
    {
        UseFuel();
        CheckForDeath();
    }

    public void PlayPowerUp()
    {
        if(audio.isPlaying)
        {
            audio.Stop();
        }
        audio.clip = powerUp;
        audio.Play();
    }

    public void PlayFire()
    {
        if (audio.isPlaying)
        {
            audio.Stop();
        }
        audio.clip = fire;
        audio.Play();
    }

    void CheckForDeath()
    {
        if (health <= 0f || fuel <= 0f)
        {
            this.GetComponent<RobotController>().enabled = false;
            deathUI.SetActive(true);
            Cursor.visible = true;
        }
    }

    void UseFuel()
    {
        AddFuel(-fuelPerSec * Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0f, maximumHealth);
        UpdateHealthUI();
    }

    public void AddFuel(float amount)
    {
        fuel += amount;
        fuel = Mathf.Clamp(fuel, 0f, maximumFuel);
        UpdateFuelUI();
    }

    void UpdateHealthUI()
    {
        healthUI.fillAmount = health / maximumHealth;
    }

    void UpdateFuelUI()
    {
        fuelUI.fillAmount = fuel / maximumFuel;
    }
}
