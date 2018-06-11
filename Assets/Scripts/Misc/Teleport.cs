using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleport : MonoBehaviour {

    public Vector3 destination = new Vector3();

    private void OnCollisionEnter(Collision collision)
    {
        GameObject agent = collision.gameObject;

        if(agent.GetComponentInChildren<Player>())
        {
            Player player = agent.GetComponentInChildren<Player>();
            int level = PlayerPrefs.GetInt("level");
            level++;
            PlayerPrefs.SetInt("level", level);
            Debug.Log("El nivel es" + level);

            //player.interfaceController.setWarningText("Pasando al nivel " + level);
            player.interfaceController.setWarningText("Going to level " + level);

            SceneManager.LoadScene("MainGame");
        }
    }
}
