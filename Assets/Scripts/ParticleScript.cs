using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    public float m_destroyTime = 4.0f;

    private void Awake()
    {
        GetComponent<ParticleSystem>().Play();
    }

    private void Update()
    {
        m_destroyTime -= Time.deltaTime;
        if (m_destroyTime <= 0.0f) Destroy(this);
    }
}
