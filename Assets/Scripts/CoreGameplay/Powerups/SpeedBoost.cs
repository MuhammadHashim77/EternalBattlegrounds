using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpeedBoost : NetworkBehaviour
{
    private PlayerControls playerControls;

    [SerializeField] private float durationSpeedUp = 5f;

    private NetworkVariable<float> speedIncrease = 
        new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && playerControls != null && IsOwner)
        {
            StartCoroutine(PickUpSpeed(playerControls));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerControls localPlayer = other.GetComponent<PlayerControls>();
            if (localPlayer != null && localPlayer.IsLocalPlayer)
            {
                playerControls = localPlayer;

                if (playerControls != null)
                {
                    RequestSpeedUpOwnershipServerRpc(localPlayer.NetworkObjectId);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpeedUpOwnershipServerRpc(ulong clientNetworkObjectId, ServerRpcParams serverRpcParams = default)
    {
        // Change the ownership of the power-up to the client that requested it
        NetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);

        // Now that the client owns the power-up, call the RPC to apply the power-up effect
        PickUpSpeedUpSequenceClientRpc(clientNetworkObjectId);
    }

    [ClientRpc]
    public void PickUpSpeedUpSequenceClientRpc(ulong playerNetworkObjectId)
    {
        NetworkObject netObj;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out netObj))
        {
            PlayerControls playerControls = netObj.gameObject.GetComponent<PlayerControls>();
            if (playerControls != null && playerControls.IsLocalPlayer)
            {
                

                // Store that the playerControls has a speed boost
                

                // After the effect has been applied, despawn the power-up
                if (NetworkObject.IsOwner)
                {
                    DespawnPowerupServerRpc();
                }
            }
        }
    }

    [ServerRpc]
    public void SpeedBoostServerRpc(ulong networkObjectId)
    {
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.NetworkObjectId == networkObjectId)
            {
                PlayerControls playerControls = netObj.gameObject.GetComponent<PlayerControls>();
                if (playerControls != null)
                {
                    // No need to call ModifySpeedServerRpc here, because it's called in PickUpSpeed coroutine
                }
                break;
            }
        }
    }

    [ClientRpc]
    public void SpeedBoostClientRpc(ulong networkObjectId)
    {
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.NetworkObjectId == networkObjectId)
            {
                PlayerControls playerControls = netObj.gameObject.GetComponent<PlayerControls>();
                if (playerControls != null)
                {
                    StartCoroutine(PickUpSpeed(playerControls));
                }
                break;
            }
        }
    }

    [ServerRpc]
    public void ModifySpeedServerRpc(ulong networkObjectId, float speedChange, ServerRpcParams serverRpcParams = default)
    {
        NetworkObject networkObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out networkObject))
        {
            PlayerControls playerControls = networkObject.gameObject.GetComponent<PlayerControls>();
            if (playerControls != null)
            {
                playerControls.movementSpeed.Value += speedChange;
                StartCoroutine(ResetSpeed(playerControls, speedChange));
            }
        }
    }


    private IEnumerator ResetSpeed(PlayerControls playerControls, float speedChange)
    {
        yield return new WaitForSeconds(durationSpeedUp);
        playerControls.movementSpeed.Value -= speedChange;
    }


    [ClientRpc]
    public void ModifySpeedClientRpc(ulong networkObjectId, float speedChange, ClientRpcParams clientRpcParams = default)
    {
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.NetworkObjectId == networkObjectId)
            {
                PlayerControls playerControls = netObj.gameObject.GetComponent<PlayerControls>();
                if (playerControls != null && playerControls.IsLocalPlayer)
                {
                    // Now that we are on the owning client, we can change the variable.
                    playerControls.movementSpeed.Value += speedChange;
                }
                break;
            }
        }
    }

    IEnumerator PickUpSpeed(PlayerControls playerControls)
    {
        
        ModifySpeedServerRpc(playerControls.NetworkObjectId, speedIncrease.Value);

        yield return new WaitForSeconds(durationSpeedUp);

    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnPowerupServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //Call ClientRPC to disable on the client
        DespawnPowerupClientRpc();
        // Disable the power-up
        GetComponent<Collider>().enabled = false;
        GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    [ClientRpc]
    public void DespawnPowerupClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Disable the power-up visual and collision
        GetComponent<Collider>().enabled = false;
        GetComponentInChildren<MeshRenderer>().enabled = false;
    }
}
