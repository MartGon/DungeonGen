using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootCrate : MonoBehaviour {

    public Animator animator;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        Player player = other.GetComponentInChildren<Player>();

        if(player)
        {
            animator.SetBool("Open", true);
        }
    }
}
