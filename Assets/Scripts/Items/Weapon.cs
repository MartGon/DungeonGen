using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour {

    // Default position
    public Vector3 weaponPosition;

    // Effect
    public GameObject shotImpact;
    public ParticleSystem muzzleFlashParticles;

    // Audio
    public AudioSource shootAudio;
    public AudioSource outOfAmmoAudio;
    public AudioSource reloadAudio;
    public AudioSource hitMarkerSound;

    // Stats
    public float weaponFireRate;
    public float weaponDamage;
    public int weaponMagazineSize;
    public float weaponSpread;

    public float weaponForce;
    public int totalAmmo;

    public int weaponMagazineCount;
    public float weaponFireRateCount;

    // Base stats
    public float baseWeaponFireRate;
    public float baseWeaponDamage;
    public int baseWeaponMagazineSize;
    public float baseWeaponSpread;

    public int weaponLevel;

    // Components
    public Animator animator;
    public BoxCollider boxCollider;
    public BoxCollider triggerCollider;
    public Rigidbody weaponRigidBody;

    // UI info
    public Canvas weaponCanvas;
    public Text fireRateDisplay;
    public Text damageDisplay;
    public Text magazineSizeDisplay;

    bool canPick = false;

    // Player
    Player player;

    // Use this for initialization
    void Start ()
    {
        // Getting Components
        animator = GetComponentInChildren<Animator>();
        muzzleFlashParticles = GetComponentInChildren<ParticleSystem>();

        // Find player
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        player = playerGo.GetComponent<Player>();

        // Set base values
        weaponFireRate = baseWeaponFireRate;
        weaponMagazineSize = baseWeaponMagazineSize;
        weaponDamage = baseWeaponDamage;

        generateWeaponStatsByLevel();
        updateWeaponInfo();

        // Weapon Stats
        weaponFireRateCount = weaponFireRate;
        weaponMagazineCount = weaponMagazineSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canPick)
        {
            pickWeapon();
        }
    }

    void generateWeaponStatsByLevel()
    {
        for (int i = 0; i < weaponLevel; i++)
        {
            int statToUpgrade = Random.Range(0, 3);

            if (statToUpgrade == 0)
                weaponFireRate -= 0.015f;
            else if (statToUpgrade == 1)
                weaponMagazineSize += 2;
            else
                weaponDamage += 3f;
        }
    }

    void updateWeaponInfo()
    {
        fireRateDisplay.text = weaponFireRate.ToString();
        damageDisplay.text = weaponDamage.ToString();
        magazineSizeDisplay.text = weaponMagazineSize.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        Player player = other.GetComponent<Player>();

        if (player)
        {
            player.interfaceController.setWeaponMsgState(true);
            weaponCanvas.gameObject.SetActive(true);

            canPick = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject go = other.gameObject;
        Player player = other.GetComponent<Player>();

        if (player)
        {
            player.interfaceController.setWeaponMsgState(false);
            weaponCanvas.gameObject.SetActive(false);
            canPick = false;
        }
    }

    void pickWeapon()
    {
        Debug.Log("Cogiendo el arma;");

        Vector3 oldPosition = transform.position;

        // La dropeamos
        player.currentWeapon.weaponRigidBody.isKinematic = true;
        player.currentWeapon.animator.enabled = false;
        player.currentWeapon.boxCollider.enabled = true;
        player.currentWeapon.triggerCollider.enabled = true;

        Transform oldParent = player.currentWeapon.transform.parent;

        player.currentWeapon.transform.SetParent(null);
        player.currentWeapon.transform.position = oldPosition;
        player.currentWeapon.weaponRigidBody.isKinematic = false;

        // Para borrar tras recoger
        player.currentWeapon.transform.SetParent(GarbageController.garbageGo.transform);

        // Desactivamos antes de coger
        boxCollider.enabled = false;
        triggerCollider.enabled = false;
        transform.SetParent(oldParent);
        transform.position = weaponPosition;
        animator.enabled = true;

        // Cogemos el arma
        player.currentWeapon = this;

        // Update UI
        player.interfaceController.updateCrossHairSpread(weaponSpread);
        player.interfaceController.updateAmmoDisplay(weaponMagazineCount, totalAmmo);
        player.interfaceController.setWeaponMsgState(false);
        weaponCanvas.gameObject.SetActive(false);

        canPick = false;
    }
}
