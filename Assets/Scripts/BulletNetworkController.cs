using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletNetworkController : NetworkBehaviour
{
    [SerializeField] private float m_bulletSpeed = 20.0f;
    [SerializeField] private float m_lifeTime    = 5.0f;

    public NetworkVariable<bool> m_targetHit = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // If i want to make a pooled spawner instead...
    private float m_currentLifeTime = 0.0f;

    public override void OnNetworkSpawn()
    {
        m_currentLifeTime = m_lifeTime;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            float ms = NetworkManager.Singleton.ServerTime.FixedDeltaTime;

            if (m_currentLifeTime > 0.0f)
            {
                Vector3 newPosition = transform.position;
                newPosition += (transform.forward * m_bulletSpeed) * ms;

                transform.position = newPosition;

                m_currentLifeTime -= ms;
            } 
        }
    }

    private void Update()
    {
        if (m_currentLifeTime <= 0.0f || m_targetHit.Value == true)
        {
            GetComponent<NetworkObject>().Despawn(true);
            Destroy(this);
        }

        m_currentLifeTime -= Time.deltaTime;
    }
}
