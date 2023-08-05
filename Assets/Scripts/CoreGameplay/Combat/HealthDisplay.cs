using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) { return; }

        health.CurrHealth.OnValueChanged += HandleHealthChange;
        HandleHealthChange(0, health.CurrHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) { return; }

        health.CurrHealth.OnValueChanged -= HandleHealthChange;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void HandleHealthChange(int oldHealth, int newHealth)
    {
        healthBarImage.fillAmount = (float)newHealth / health.MaxHealth;
    }
}
