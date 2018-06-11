using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour {

    // Enemy UI elements
    public Text nameText;
    public Text enemyLevel;
    public Image healtBarBorder;
    public GameObject healtBar;

    public float maxHealthPoints;

    public void updateHealthPoints(float currentHealthPoints)
    {
        float width = currentHealthPoints / maxHealthPoints;

        Vector3 scale = new Vector3(width, 1, 1);
        healtBar.transform.localScale = scale;
    }

    public void setName(string name)
    {
        nameText.text = name;
    }

    public void setLevel(int level)
    {
        enemyLevel.text = level.ToString();
    }
}
