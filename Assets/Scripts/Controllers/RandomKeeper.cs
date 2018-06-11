using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomKeeper : MonoBehaviour
{ 
    public static Random.State randomState;

    private void Awake()
    {
        randomState = Random.state;
    }
}
