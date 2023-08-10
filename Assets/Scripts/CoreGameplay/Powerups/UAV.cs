using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UAV : NetworkBehaviour
{
    private Player player;

    [SerializeField] private float durationUav = 4f;
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Vector3 followOffset;
    [SerializeField] private float heightIncrease = 7f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && player != null && IsOwner)
        {
            UAVServerRpc(player.NetworkObjectId);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Player localPlayer = other.GetComponent<Player>();
            if (localPlayer != null && localPlayer.IsLocalPlayer)
            {
                player = localPlayer;
                cinemachineVirtualCamera = player.GetComponentInChildren<CinemachineVirtualCamera>();

                if (cinemachineVirtualCamera != null)
                {
                    followOffset = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
                }
                else
                {
                    Debug.Log("Cinemachine Virtual Camera not found on player");
                }

                if (player != null)
                {
                    RequestUAVOwnershipServerRpc(localPlayer.NetworkObjectId);
                }
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestUAVOwnershipServerRpc(ulong clientNetworkObjectId, ServerRpcParams serverRpcParams = default)
    {
        // Change the ownership of the power-up to the client that requested it
        NetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);

        // Now that the client owns the power-up, call the RPC to apply the power-up effect
        PickUpUAVSequenceClientRpc(clientNetworkObjectId);
    }

    [ClientRpc]
    public void PickUpUAVSequenceClientRpc(ulong playerNetworkObjectId)
    {
        NetworkObject netObj;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out netObj))
        {
            Player player = netObj.gameObject.GetComponent<Player>();
            if (player != null && player.IsLocalPlayer)
            {
                GetComponent<Collider>().enabled = false;
                GetComponentInChildren<MeshRenderer>().enabled = false;
                

                // Store that the player has a UAV boost
                

                // After the effect has been applied, despawn the power-up
                if (NetworkObject.IsOwner)
                {
                    DespawnPowerupServerRpc();
                }
            }
        }
    }

    [ServerRpc]
    public void UAVServerRpc(ulong networkObjectId)
    {
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.NetworkObjectId == networkObjectId)
            {
                Player player = netObj.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    UAVClientRpc(netObj.NetworkObjectId);
                }
                break;
            }
        }
    }

    [ClientRpc]
    public void UAVClientRpc(ulong networkObjectId)
    {
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.NetworkObjectId == networkObjectId)
            {
                Player player = netObj.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    StartCoroutine(PickUpUAV(player));
                }
                break;
            }
        }
    }

    IEnumerator PickUpUAV(Player player)
    {
        

        // Ensure that the following logic runs only on the owner
        if (player.IsLocalPlayer)
        {
            followOffset.y += heightIncrease;
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = followOffset;
        }

        yield return new WaitForSeconds(durationUav);

        // Ensure that the following logic runs only on the owner
        if (player.IsLocalPlayer)
        {
            followOffset.y -= heightIncrease;
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = followOffset;
        }
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
