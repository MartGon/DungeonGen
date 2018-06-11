using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCanvas : MonoBehaviour {

    public Camera m_Camera;
    Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        m_Camera = playerGo.GetComponentInChildren<Camera>();
        canvas.worldCamera = m_Camera;
    }

    void Update()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);
    }
}
