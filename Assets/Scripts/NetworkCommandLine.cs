using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkCommandLine : MonoBehaviour
{
    private NetworkManager m_netManager;
    [SerializeField] private Button[] m_buttons;

    void Awake()
    {
        m_netManager = GetComponentInParent<NetworkManager>(); 
    }

    public void StartHost()
    {
        DisableButtons(true);
        m_netManager.StartHost();
    }
    public void StartClient()
    {
        DisableButtons(true);
        m_netManager.StartClient();
    }

    public void StartServer()
    {
        DisableButtons(true);
        m_netManager.StartServer();
    }

    void DisableButtons(bool disable)
    {
        for (uint i = 0; i < m_buttons.Length; i++)
            m_buttons[i].gameObject.SetActive(!disable);
    }
}
