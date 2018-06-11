using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public char openGateId;

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        Player player = other.GetComponentInChildren<Player>();

        if(player)
        {
            //player.interfaceController.setWarningText("Has  cogido la llave de la puerta " + openGateId);
            player.interfaceController.setWarningText("You've picked up the key " + openGateId);
            Debug.Log("Has  cogido la llave de la puerta " + openGateId + " !!");
            player.addKey(openGateId);
            gameObject.SetActive(false);
        }
    }
}
