using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBox : MonoBehaviour {

    public int hpRestored;

    private void OnCollisionEnter(Collision other)
    {
        GameObject go = other.gameObject;
        Player player = go.GetComponentInChildren<Player>();

        if (player)
        {
            if (player.hpCount == 100)
            {
                //player.interfaceController.setWarningText("No has podido coger el botiquín porque tu salud ya está al máximo");
                player.interfaceController.setWarningText("You couldn't pick up the first aid kit because your health is already full");
                Debug.Log("El jugador ha tratado de coger un botiquín con la salud al máximo!");
                return;
            }

            //player.interfaceController.setWarningText("Has cogido un botiquín");
            player.interfaceController.setWarningText("You've picked up a first aid kit");
            Debug.Log("El jugador ha cogido un botiquín");
            player.hpCount += hpRestored;
            if (player.hpCount > player.hp)
                player.hpCount = player.hp;
            player.interfaceController.updateHP(player.hpCount, player.hp);
            gameObject.SetActive(false);
        }
    }
}
