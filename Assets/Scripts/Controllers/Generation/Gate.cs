using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Gate : MonoBehaviour
{
    public char GateId;
    public NavMeshObstacle obstacle;
    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        Player player = go.GetComponentInChildren<Player>();

        if(player)
        {
            Debug.Log("El jugador está frente a la puerta");
            if(player.hasKey(GateId))
            {
                Debug.Log("El jugador tiene la llave de la puerta " + GateId);
                //player.interfaceController.setWarningText("Has abierto la puerta " + GateId);
                player.interfaceController.setWarningText("You've opened the gate " + GateId);
                // Abrir
                animator.SetBool("Open", true);
                obstacle.enabled = false;
            }
            else
            {
                //player.interfaceController.setWarningText("No tienes la llave de la puerta " + GateId);
                player.interfaceController.setWarningText("You don't have the key for gate " + GateId);
                Debug.Log("El jugador no tiene la llave de la puerta " + GateId);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject go = other.gameObject;
        Player player = go.GetComponentInChildren<Player>();

        if (player)
        {
            Debug.Log("El jugador está frente a la puerta");
            if (player.hasKey(GateId))
            {
                Debug.Log("El jugador tiene la llave de la puerta " + GateId);
                // Abrir
                animator.SetBool("Open", false);
                obstacle.enabled = true;
            }
            else
            {
                Debug.Log("El jugador no tiene la llave de la puerta " + GateId);
            }
        }
    }
}
