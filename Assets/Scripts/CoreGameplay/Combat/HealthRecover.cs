using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HealthRecover : NetworkBehaviour
{
    [SerializeField] private float recoveryInterval = 2f;
    [SerializeField] private int healthRecoverAmount = 5;

    private Health health;
    private float timeSinceLastDamage;

    private void Awake()
    {
        health = GetComponent<Health>(); // Assuming the Health component is on the same GameObject
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        if (health.CurrHealth.Value < health.MaxHealth)
        {
            timeSinceLastDamage += Time.deltaTime;
            if (timeSinceLastDamage >= recoveryInterval)
            {
                RequestHealthRecoveryServerRpc();
                timeSinceLastDamage = 0f; // reset the timer
            }
        }
    }

    [ServerRpc]
    private void RequestHealthRecoveryServerRpc()
    {
        if (health.CurrHealth.Value < health.MaxHealth)
        {
            int newHealthValue = health.CurrHealth.Value + healthRecoverAmount;
            if (newHealthValue > health.MaxHealth)
            {
                newHealthValue = health.MaxHealth;
            }
            UpdateHealthClientRpc(newHealthValue); // Notify all clients
        }
    }

    [ClientRpc]
    private void UpdateHealthClientRpc(int newHealthValue)
    {
        health.CurrHealth.Value = newHealthValue;
    }

    public void TookDamage()
    {
        timeSinceLastDamage = 0f; // Reset the timer when taking damage
    }
}
