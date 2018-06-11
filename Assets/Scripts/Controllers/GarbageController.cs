using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageController : MonoBehaviour {

	public static GameObject garbageGo;

    private void Start()
    {
        garbageGo = GameObject.FindGameObjectWithTag("Garbage");
    }
}
