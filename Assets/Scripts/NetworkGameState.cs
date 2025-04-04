using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameState : NetworkBehaviour
{
    [SerializeField] private List<Transform> m_spawnPoints;
    [SerializeField] private Transform       m_deathEffect;

    private Dictionary<int, Transform> m_players = new Dictionary<int, Transform>();
    
    public void AddPlayer(int clientID, Transform player)
    {
        m_players.Add(clientID, player);
    }

    // Annoying bug with Netcode where owner of objects can't call ServerRpc functions...
    [ServerRpc(RequireOwnership = false)]
    public void KillPlayerServerRpc(ServerRpcParams param)
    {
        int clientID = ((int)param.Receive.SenderClientId);

        if(m_players.ContainsKey(clientID))
        {
            Transform player = m_players[clientID];

            PlayDeathEffectClientRpc(player.transform.position, m_deathEffect.rotation);
            m_players[clientID].transform.position = new Vector3(0.0f, -100.0f, 0.0f);
        }
    }

    // Play the death effect on each client instead of the server... 
    [ClientRpc]
    private void PlayDeathEffectClientRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(m_deathEffect, position, rotation);
    }

    // Same bug as KillPlayerServerRpc
    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerAtPointServerRpc(ServerRpcParams param)
    {
        int clientID = ((int)param.Receive.SenderClientId);

        if (m_players.ContainsKey(clientID))
        { 
            int randomValue = Random.Range(0, m_spawnPoints.Count - 1);
            m_players[clientID].position = m_spawnPoints[randomValue].position;
            Vector3 centerOfMap = new Vector3(0.0f, m_players[clientID].position.y, 0.0f);
            m_players[clientID].LookAt(centerOfMap);
        }
    }
}
