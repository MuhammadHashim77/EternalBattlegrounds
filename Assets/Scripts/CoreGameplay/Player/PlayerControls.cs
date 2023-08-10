using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerControls : NetworkBehaviour 
{
    Vector3 movement;

    [SerializeField] public NetworkVariable<float> movementSpeed = 
        new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    [SerializeField] private float rotationSpeed = 720f;

    private Animator animator;
    public event Action<bool> PrimaryFireEvent;

    public override void OnNetworkSpawn()
    { 
        if(!IsOwner) { return; }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }

        CharacterMovementRotation();
        Shooting();
    }

    private void CharacterMovementRotation()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movement = new Vector3(horizontal, 0f, vertical);
        movement.Normalize();

        transform.Translate(movement * movementSpeed.Value * Time.deltaTime, Space.World);

        if (movement != Vector3.zero)
        {
            animator.SetBool("Running", true);
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("Running", false);
        }
    }

    public void Shooting()
    {
        if (Input.GetButton("Fire1"))
        {
            PrimaryFireEvent?.Invoke(true);
            animator.SetBool("Shooting", true);
        }
        else
        {
            PrimaryFireEvent?.Invoke(false);
            animator.SetBool("Shooting", false);
        }
    }

}
