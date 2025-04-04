using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    private NetworkGameState m_gameState;

    [SerializeField] private Transform m_bulletPrefab;
    [SerializeField] private Transform m_firePosition;

    [SerializeField] private float m_moveSpeed   = 5.0f;
    [SerializeField] private float m_rotateSpeed = 45.0f;

    [SerializeField] private float m_respawnTime        = 2.0f;
    private float                  m_currentRespawnTime = 0.0f;

    [SerializeField] private int m_maxHealth     = 5;

    [SerializeField] private float m_shootSpeed  = 0.1f;
    private float m_currentShootTimer            = 0.0f;

    private NetworkVariable<int>           m_currentHealth  = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //private NetworkVariable<string>        m_playerName     = new NetworkVariable<string>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool>          m_playerIsAlive  = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int>           m_clientID       = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<MoveRequest>   m_moveVariable   = new NetworkVariable<MoveRequest>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<RotateRequest> m_rotateVariable = new NetworkVariable<RotateRequest>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private bool m_movePlayer   = false;
    private bool m_rotatePlayer = false;

    // Would have been much smarter to separate the player from the 
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Preset client id and player name.
            m_clientID.Value   = (int)OwnerClientId;
        }

        m_gameState = FindObjectOfType<NetworkGameState>(); 
        if(IsServer) // Transform of players only needs to be stored on the server.
            m_gameState.AddPlayer((int)OwnerClientId, transform);

            m_moveVariable.OnValueChanged += (MoveRequest previousValue, MoveRequest newValue) =>
        {
            if (newValue.m_moveX != 0.0f || newValue.m_moveZ != 0.0f)
                m_movePlayer = true;
            else
                m_movePlayer = false;
        };
        m_rotateVariable.OnValueChanged += (RotateRequest previousRequest, RotateRequest newRequest) =>
        {
            if (newRequest.m_rotate != 0.0f)
                m_rotatePlayer = true;
            else
                m_rotatePlayer = false;
        };

    }

    void Update()
    {
        // Player input is client bases that then gets handled by the server.
        if (!IsOwner) return;

        // Kind of a shit way to implement this but clients notify the server when they are ready to spawn.
        if (m_currentRespawnTime <= 0.0f && !m_playerIsAlive.Value)
        {
            SpawnPlayer();
        }

        PlayerMovementInput();

        if (m_playerIsAlive.Value)
        {
            if (Input.GetMouseButton(0) && m_currentShootTimer <= 0.0f)
            {
                m_currentShootTimer = m_shootSpeed;
                ShootBulletServerRpc();
            }
        }

        m_currentShootTimer  -= Time.deltaTime;
        m_currentRespawnTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float ms = NetworkManager.Singleton.ServerTime.FixedDeltaTime;

        // Movement calculations and application is handled by the server, probably not the most efficient way but it works for this...
        if (IsServer)
        {
            HandleMovement();
        }

        if (!IsOwner) return;
    }

    void PlayerMovementInput()
    {
        float z = Input.GetAxisRaw("Vertical")   * m_moveSpeed;
        float x = Input.GetAxisRaw("Horizontal") * m_moveSpeed;

        m_moveVariable.Value = new MoveRequest() { m_moveX = x, m_moveZ = z };          
        
        if (Input.GetKey(KeyCode.Q))
            m_rotateVariable.Value = new RotateRequest() { m_rotate = -1.0f };
        else if (Input.GetKey(KeyCode.E))
            m_rotateVariable.Value = new RotateRequest() { m_rotate =  1.0f };
        else
            m_rotateVariable.Value = new RotateRequest() { m_rotate =  0.0f };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            other.GetComponent<NetworkObject>().Despawn();
            Destroy(other);

            // Also not the greatest way to handle this but it works, clients handle their own health and if the player is alive. 
            TakeDamageClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { (ulong)m_clientID.Value } } }); 
        }
    }

    private void HandleMovement()
    {
        float ms = NetworkManager.Singleton.ServerTime.FixedDeltaTime;

        if (m_playerIsAlive.Value)
        {
            if (m_movePlayer)
            {
                Vector3 newPosition = transform.position;

                newPosition += (transform.forward * m_moveVariable.Value.m_moveZ) * ms;
                newPosition += (transform.right * m_moveVariable.Value.m_moveX) * ms;

                transform.position = newPosition;
            }

            if (m_rotatePlayer)
            {
                Quaternion newRotation = transform.rotation;
                float rot = (m_rotateSpeed * m_rotateVariable.Value.m_rotate) * ms;
                newRotation *= Quaternion.AngleAxis(rot, transform.up);
                transform.rotation = newRotation;
            }
        }
    }

    [ClientRpc]
    public void TakeDamageClientRpc(ClientRpcParams param)
    {
        m_currentHealth.Value--;
        if (m_currentHealth.Value <= 0)
        {
            m_playerIsAlive.Value = false;
            m_currentRespawnTime = m_respawnTime;
            m_gameState.KillPlayerServerRpc(new ServerRpcParams { Receive = new ServerRpcReceiveParams { SenderClientId = OwnerClientId } });
        }
    }

    private void SpawnPlayer()
    {
        m_currentHealth.Value = m_maxHealth;
        m_playerIsAlive.Value = true;

        m_gameState.SpawnPlayerAtPointServerRpc(new ServerRpcParams { Receive = new ServerRpcReceiveParams { SenderClientId = OwnerClientId } });
    }

    [ServerRpc]
    void ShootBulletServerRpc()
    {
        Transform bulletTransform = Instantiate(m_bulletPrefab, m_firePosition.position, Quaternion.LookRotation(m_firePosition.forward, transform.up));
        bulletTransform.GetComponent<NetworkObject>().Spawn(true);
    }
}
