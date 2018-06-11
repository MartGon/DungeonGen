using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : MonoBehaviour {

    public int ammoRestored;

    private void OnCollisionEnter(Collision other)
    {
        GameObject go = other.gameObject;
        Player player = go.GetComponentInChildren<Player>();

        if (player)
        {
            Debug.Log("El jugador ha cogido un paquete de munición");
            //player.interfaceController.setWarningText("Has cogido un paquete de munición");
            player.interfaceController.setWarningText("You've picked up an ammunition box");
            player.currentWeapon.totalAmmo += ammoRestored;
            player.interfaceController.updateAmmoDisplay(player.currentWeapon.weaponMagazineCount, player.currentWeapon.totalAmmo);
            gameObject.SetActive(false);
        }
    }
}
