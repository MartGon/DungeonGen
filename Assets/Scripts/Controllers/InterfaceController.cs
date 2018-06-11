using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InterfaceController : MonoBehaviour {

    // Player
    Player player;

    // Player info
    public Text hPDisplay;
    public Text ammoDisplay;
    public Image hitMarker;

    // Level info
    public Text levelInfo;
    public Text seedDisplay;

    // Warning Widgets
    public Text WarningText1;
    public Text WarningText2;
    public Text WarningText3;
    CanvasGroup canvasGroup1;
    CanvasGroup canvasGroup2;
    CanvasGroup canvasGroup3;
    public float reduceStep = 0.0000005f;

    public float hitMarkerDuration;
    float hitMarkerDurationCount;

    // Escape Menu
    public GameObject escapeMenu;

    // Weapon Stats
    public GameObject weaponStatsMenu;

    public Text fireType;
    public Text fireRate;
    public Text damage;
    public Text magazineSize;

    // CrossHair Elements
    public float scaler = 0.8f;
    public Image upperIndicator;
    public Image lowerIndicator;
    public Image rightIndicator;
    public Image leftIndicator;

    Vector3 originalUpperPos;
    Vector3 originalRightPos;

    // Pick Weapon
    public Text pickUpWeaponText;

    // Performance
    public GameObject performanceMenu;

    public Dropdown dungeonDropdown;

    public Text selRoom;
    public Text selCorridor;
    public Text selBlocks;
    public Text selEaT;
    public Text selTotal;

    public Text avgRoom;
    public Text avgCorridor;
    public Text avgBlocks;
    public Text avgEaT;
    public Text avgTotal;

    public Text lastRoom;
    public Text lastCorridor;
    public Text lastBlocks;
    public Text lastEaT;
    public Text lastTotal;

	// Use this for initialization
	void Awake ()
    {
        // Find player
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        player = playerGo.GetComponent<Player>();

        // Warning texts set up
        canvasGroup1 = WarningText1.GetComponent<CanvasGroup>();
        canvasGroup2 = WarningText2.GetComponent<CanvasGroup>();
        canvasGroup3 = WarningText3.GetComponent<CanvasGroup>();
        canvasGroup1.alpha = 0;
        canvasGroup2.alpha = 0;
        canvasGroup3.alpha = 0;

        // Crosshair vars
        originalUpperPos = upperIndicator.transform.localPosition;
        originalRightPos = rightIndicator.transform.localPosition;

        Debug.Log(upperIndicator.transform.localPosition);
    }
	
	// Update is called once per frame
	void Update ()
    {
        checkEscapeMenu();
        checkPerformanceMenu();
        updateHitMarker();
        updateTextWarningAlpha();
	}

    public void updateAmmoDisplay(int bulletsLeft, int maxBullets)
    {
        string ammo = bulletsLeft + " / " + maxBullets;
        ammoDisplay.text = ammo;
    }

    public void updateHP(float hPLeft, float totalHP)
    {
        string hp = Mathf.RoundToInt(hPLeft) + " / " + Mathf.RoundToInt(totalHP);
        hPDisplay.text = hp;
    }
    
    public void checkEscapeMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (weaponStatsMenu.active)
                weaponStatsMenu.SetActive(false);
            else if (escapeMenu.active)
                escapeMenu.SetActive(false);
            else
                escapeMenu.SetActive(true);
        }
        else if(escapeMenu.active || weaponStatsMenu.active || performanceMenu.active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void updateHitMarker()
    {
        if (hitMarkerDurationCount <= 0)
        {
            hitMarker.gameObject.SetActive(false);
        }
        else
            hitMarkerDurationCount -= Time.deltaTime;
    }

    public void putHitMarker()
    {
        hitMarker.gameObject.SetActive(true);
        hitMarkerDurationCount = hitMarkerDuration;
    }

    public void setWarningText(string text)
    {
        if(canvasGroup1.alpha == 0)
        {
            WarningText1.text = text;
            canvasGroup1.alpha = 1;
        }
        else if(canvasGroup2.alpha == 0)
        {
            WarningText2.text = text;
            canvasGroup2.alpha = 1;
        }
        else if (canvasGroup3.alpha == 0)
        {
            WarningText3.text = text;
            canvasGroup3.alpha = 1;
        }
        else
        {
            WarningText1.text = text;
            canvasGroup1.alpha = 1;
        }
    }

    void updateTextWarningAlpha()
    {
        canvasGroup1.alpha -= reduceStep;
        if (canvasGroup1.alpha < 0)
            canvasGroup1.alpha = 0;
        canvasGroup2.alpha -= reduceStep;
        if (canvasGroup2.alpha < 0)
            canvasGroup2.alpha = 0;
        canvasGroup3.alpha -= reduceStep;
        if (canvasGroup3.alpha < 0)
            canvasGroup3.alpha = 0;
    }

    public void updateLevelInfo(int level, int seed)
    {
        levelInfo.text = "Level " + level;
        seedDisplay.text = seed.ToString("X");
    }

    public void exitToMainMenu()
    {
        GameObject.Destroy(player.gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    public void continueButton()
    {
        escapeMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void weaponStatsButton()
    {
        weaponStatsMenu.SetActive(true);

        fireType.text = "Auto";
        fireRate.text = player.currentWeapon.weaponFireRate.ToString();
        damage.text = player.currentWeapon.weaponDamage.ToString();
        magazineSize.text = player.currentWeapon.weaponMagazineSize.ToString();

        escapeMenu.SetActive(false);
    }

    public void weaponStatsBackButoon()
    {
        weaponStatsMenu.SetActive(false);
        escapeMenu.SetActive(true);
    }

    public void updateCrossHairSpread(float spread)
    {
        //spread *= scaler;
        upperIndicator.transform.localPosition = spread * originalUpperPos;
        lowerIndicator.transform.localPosition = spread * (-originalUpperPos);
        rightIndicator.transform.localPosition = spread * originalRightPos;
        leftIndicator.transform.localPosition = spread * (-originalRightPos);
    }
    
    public void setWeaponMsgState(bool active)
    {
        pickUpWeaponText.gameObject.SetActive(active);
    }

    public void updatePerformanceReport()
    {
        dungeonDropdown.ClearOptions();

        int count = PerformanceController.getReportsCount();
        List<string> options = new List<string>();
        for(int i = 0; i < count; i++)
        {
            options.Add(i.ToString());
        }

        dungeonDropdown.AddOptions(options);

        PerformanceController.DungeonPerformanceReport report = PerformanceController.getReportByIndex(0);

        selRoom.text = report.rooms.ToString();
        selCorridor.text = report.corridors.ToString();
        selBlocks.text = report.block.ToString();
        selEaT.text = report.misc.ToString();
        selTotal.text = (report.rooms + report.corridors + report.block + report.misc).ToString();

        report = PerformanceController.getReportByIndex(PerformanceController.getReportsCount() - 1);

        lastRoom.text = report.rooms.ToString();
        lastCorridor.text = report.corridors.ToString();
        lastBlocks.text = report.block.ToString();
        lastEaT.text = report.misc.ToString();
        lastTotal.text = (report.rooms + report.corridors + report.block + report.misc).ToString();

        report = new PerformanceController.DungeonPerformanceReport();

        float avgeTotal = 0;
        for(int i = 0; i < count; i++)
        {
            PerformanceController.DungeonPerformanceReport curReport = PerformanceController.getReportByIndex(i);
            report.rooms += curReport.rooms;
            report.corridors += curReport.corridors;
            report.block += curReport.block;
            report.misc += curReport.misc;
            avgeTotal += curReport.rooms + curReport.corridors + curReport.block + curReport.misc;
        }

        report.rooms /= count;
        report.corridors /= count;
        report.block /= count;
        report.misc /= count;
        avgeTotal /= count;

        avgRoom.text = report.rooms.ToString();
        avgCorridor.text = report.corridors.ToString();
        avgBlocks.text = report.block.ToString();
        avgEaT.text = report.misc.ToString();
        avgTotal.text = avgeTotal.ToString();
    }

    public void checkPerformanceMenu()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            if (!performanceMenu.active)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                updatePerformanceReport();
                performanceMenu.SetActive(true);
            }
            else
                handleOkReportButton();
        }
    }

    public void handleOkReportButton()
    {
        performanceMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void handleReportDropDown(int option)
    {
        option = dungeonDropdown.value;
        PerformanceController.DungeonPerformanceReport report = PerformanceController.getReportByIndex(option);

        selRoom.text = report.rooms.ToString();
        selCorridor.text = report.corridors.ToString();
        selBlocks.text = report.block.ToString();
        selEaT.text = report.misc.ToString();
        selTotal.text = (report.rooms + report.corridors + report.block + report.misc).ToString();
    }
}
