using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerControls inputReader;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Collider playerCollider;


    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    //[SerializeField] private float muzzleFlashDuration;

    private bool shouldFire;
    private float previousFireTime;
    //private float muzzleFlashTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }

    private void Update()
    {
        //if(muzzleFlashTimer > 0)
        //{
        //    muzzleFlashTimer -= Time.deltaTime;

        //    if(muzzleFlashTimer <= 0f)
        //    {
        //        muzzleFlash.Play(false);
        //    }
        //}

        //muzzleFlash.Play(false);

        if (!IsOwner) { return; }

        if (!shouldFire) { return; }

        if(Time.time < (1 / fireRate + previousFireTime)) { return; }

        PrimaryFireServerRpc(projectileSpawnPoint.position, transform.forward);

        SpawnDummyProjectile(projectileSpawnPoint.position, transform.forward); 

        previousFireTime = Time.time;
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner)
        {
            if (muzzleFlash.isStopped || muzzleFlash.isPaused)
            {
                muzzleFlash.Play();
            }
        }

        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction));

        Physics.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider>());

        if (projectileInstance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.velocity = direction * projectileSpeed;
        }
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction));

        Physics.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider>());

        if(projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDmg))
        {
            dealDmg.SetOwner(OwnerClientId);
        }

        if (projectileInstance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.velocity = direction * projectileSpeed;
        }

        SpawnDummyPorjectileClientRpc(spawnPos, direction);
    }


    [ClientRpc]
    private void SpawnDummyPorjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner) { return; }

        SpawnDummyProjectile(spawnPos, direction);
    }
}
