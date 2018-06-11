using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public enum DifficultyMode
    {
        NAVIGATION,
        EASY,
        NORMAL,
        HARD
    }

    // Difficulty dictionary
    public Dictionary<DifficultyMode, string> difficultyStringTips;

    // Interface Widgets
    public InputField seedInput;
    public InputField levelInput;
    public Dropdown difficultyDropDown;
    public Text difficultyTip;

    // Interface GOs
    public GameObject mainMenuGameObject;
    public GameObject seedMenuGameObject;

    private void Start()
    {
        initDifficultyStringTips();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void initDifficultyStringTips()
    {
        difficultyStringTips = new Dictionary<DifficultyMode, string>();

        difficultyStringTips.Add(DifficultyMode.NAVIGATION, "There will be no enemies. For debugging or exploration purposes");
        difficultyStringTips.Add(DifficultyMode.EASY, "For those who have not played many shooter games");
        difficultyStringTips.Add(DifficultyMode.NORMAL, "A challenge for most player");
        difficultyStringTips.Add(DifficultyMode.HARD, "Difficulty at its finest");
    }

    public void playButtonHandler()
    {
        seedMenuGameObject.SetActive(true);
        mainMenuGameObject.SetActive(false);
    }
    
    public void continueButtonHandler()
    {
        int inputSeed = 0;
        if (seedInput.text != "")
            inputSeed = int.Parse(seedInput.text);

        int inputLevel = 1;
        if (levelInput.text != "")
            inputLevel = int.Parse(levelInput.text);

        int difficulty = difficultyDropDown.value;

        Debug.Log("El int es " + inputSeed);
        PlayerPrefs.SetInt("seed", inputSeed);
        PlayerPrefs.SetInt("level", inputLevel);
        PlayerPrefs.SetInt("difficulty", difficulty);
        PlayerPrefs.SetInt("load", 1);

        SceneManager.LoadScene("MainGame");
    }

    public void backButtonHandler()
    {
        seedMenuGameObject.SetActive(false);
        mainMenuGameObject.SetActive(true);
    }

    public void exitButtonHandler()
    {
        Application.Quit();
    }

    public void onChangeDifficulty(int option)
    {
        option = difficultyDropDown.value;
        DifficultyMode difficulty = (DifficultyMode)option;

        difficultyTip.text = difficultyStringTips[difficulty];
    }
}
