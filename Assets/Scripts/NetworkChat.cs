using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkChat : NetworkBehaviour
{
    [SerializeField] private GameObject    m_textPrefab;
    [SerializeField] private RectTransform m_textParent;
    
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private TMP_Text       m_inputText;
    [SerializeField] private float          m_messageOffset = 5.0f;
    [SerializeField] private int            m_maxMessages;

    private List<TMP_Text> m_chatMessages = new List<TMP_Text>();

    private void Awake()
    {
        var se = new TMP_InputField.SubmitEvent();
        se.AddListener(SendChatMessage);
        m_inputField.onEndEdit = se;
    }

    void SendChatMessage(string message)
    {
        if (!IsOwner && message.Length <= 0) return;
        m_inputText.text = "";
        SendMessageToServerRpc(message, new ServerRpcParams { Receive = new ServerRpcReceiveParams {SenderClientId = OwnerClientId} });
    }

    // Send the message to the server so it can then send the messages to all clients. 
    [ServerRpc(RequireOwnership = false)]
    public void SendMessageToServerRpc(string message, ServerRpcParams param)
    {
        DisplayMessageToClientRpc("Player " + param.Receive.SenderClientId.ToString() + ": " + message);
    }
   
    [ClientRpc]
    public void DisplayMessageToClientRpc(string message)
    {
        TMP_Text newText = Instantiate(m_textPrefab, m_textParent).GetComponent<TMP_Text>();
        newText.text = message;
        
        foreach (TMP_Text text in m_chatMessages)
        {
            text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, text.rectTransform.anchoredPosition.y + (text.rectTransform.rect.height / 2.0f) + m_messageOffset);
        }
      
        if (m_chatMessages.Count >= m_maxMessages)
        {
            Destroy(m_chatMessages[0].gameObject);
            m_chatMessages.RemoveAt(0);
        }
        
       
        m_chatMessages.Add(newText);
    }

    //void AddMessage(string message)
    //{
    //    TMP_Text newText = Instantiate(m_textPrefab, m_textParent).GetComponent<TMP_Text>();
    //    newText.text = message;
    //    
    //    foreach (TMP_Text text in m_chatMessages)
    //    {
    //        text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, text.rectTransform.anchoredPosition.y + (text.rectTransform.rect.height / 2.0f) + m_messageOffset);
    //    }
    //
    //    if (m_chatMessages.Count >= m_maxMessages)
    //    {
    //        Destroy(m_chatMessages[0].gameObject);
    //        m_chatMessages.RemoveAt(0);
    //    }
    //
    //    m_chatMessages.Add(newText);
    //}
}
