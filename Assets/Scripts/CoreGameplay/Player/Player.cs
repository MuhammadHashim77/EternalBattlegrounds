using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Collections;

public class Player : NetworkBehaviour
{
    [Header("References")]

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;


    [Header("Settings")]

    [SerializeField] private int ownerPriority = 15;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

            PlayerName.Value = userData.userName;
        }

        if (IsOwner)
        {
            cinemachineVirtualCamera.Priority = ownerPriority; 
        }
    }
}
