using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

    // Arguments
    public float cameraSpeed = 3f;
    public float movSpeed = 12f;
    public GameObject CameraGo;

    // Weapon
    public Weapon currentWeapon;

    // Melee
    public float meleeRange;
    public int meleeDamage;

    // Camera
    Camera mainCamera;
    float horizontal;
    float vertical;

    // Interface
    public InterfaceController interfaceController;

    // State
    public playerState state = playerState.NONE;
        
        // reload
    float reloadAnimationLength;
    float reloadAnimationCount;

        // melee strike
    float strikeAnimationLenght;
    float strikeAnimationCount;

        // Aim
    float aimAnimationLength;
    float aimAnimationCount;

    public enum playerState
    {
        NONE,
        RELOADING,
        AIMING,
        MELEE_STRIKE
    }

    // Stats
    public float hp;
    public float hpCount;

    public List<char> acquiredKeys = new List<char>();

    private void Start()
    {
        // Getting Components
        mainCamera = CameraGo.GetComponentInChildren<Camera>();

        // Stats
        hpCount = hp;

        interfaceController.updateCrossHairSpread(currentWeapon.weaponSpread);
    }

    void Update ()
    {
        updateCameraRotation();
        updateMovement();
        checkDeath();

        switch (state)
        {
            case playerState.NONE:
                checkingAim();
                checkReload();
                checkMeleeStrike();
                checkShooting();
                break;
            case playerState.AIMING:
                checkingAim();
                checkShooting();
                checkReload();
                break;
            case playerState.RELOADING:
                checkReload();
                break;
            case playerState.MELEE_STRIKE:
                checkMeleeStrike();
                break;
        }
       
    }

    void updateCameraRotation()
    {
        horizontal = Input.GetAxis("Mouse X") * cameraSpeed;
        vertical -= Input.GetAxis("Mouse Y") * cameraSpeed;
        vertical = Mathf.Clamp(vertical, -80, 80);

        transform.Rotate(0, horizontal, 0);
        CameraGo.transform.localRotation = Quaternion.Euler(vertical, 0, 0);
    }

    void updateMovement()
    {
        float movementVertical;
        float movementHorizontal;
        movementVertical = Input.GetAxis("Vertical") * Time.deltaTime * (state == playerState.AIMING ? movSpeed/2 : movSpeed);
        movementHorizontal = Input.GetAxis("Horizontal") * Time.deltaTime * (state == playerState.AIMING ? movSpeed / 2 : movSpeed);

        bool moving = !(movementHorizontal == 0 && movementVertical == 0);

        // Updateando animator
        if (moving)
            currentWeapon.animator.SetBool("Walking", true);
        else
            currentWeapon.animator.SetBool("Walking", false);

        transform.Translate(movementHorizontal, 0, movementVertical);
    }

    // Weapon Checks

    void checkingAim()
    {
        if (Input.GetMouseButton(1))
        {
            currentWeapon.animator.SetBool("Aiming", true);
            interfaceController.updateCrossHairSpread(currentWeapon.weaponSpread * 0.25f);
            state = playerState.AIMING;
        }
        else
        {
            currentWeapon.animator.SetBool("Aiming", false);
            interfaceController.updateCrossHairSpread(currentWeapon.weaponSpread);
            state = playerState.NONE;
        }
    }

    void checkShooting()
    {
        // Fire button
        if (Input.GetMouseButton(0))
        {
            // If we are out of ammo we can't shoot
            if (currentWeapon.weaponMagazineCount == 0)
            {
                // Play audio once
                if(Input.GetMouseButtonDown(0))
                    currentWeapon.outOfAmmoAudio.Play();
                return;
            }

            if (currentWeapon.weaponFireRateCount < 0)
            {
                // Play effects
                currentWeapon.muzzleFlashParticles.Play();
                currentWeapon.shootAudio.Play();

                // RayCast
                RaycastHit hit;

                float range = 50f;
                Vector2 randomPoint = Random.insideUnitCircle * (state == playerState.AIMING ? currentWeapon.weaponSpread * 0.25f: currentWeapon.weaponSpread);
                Vector3 offsetVector = new Vector3(randomPoint.x, randomPoint.y, 0);
                Vector3 shootingTarget = CameraGo.transform.forward * range + offsetVector;

                Physics.Raycast(mainCamera.transform.position, shootingTarget, out hit);
                if (hit.collider)
                {
                    GameObject colliderOwner = hit.collider.gameObject;
                    //Debug.Log("Hit normal " + hit.normal);

                    // Check if it's an enemy
                    if(colliderOwner.GetComponentInChildren<Unit>())
                    {
                        Unit hitUnit = colliderOwner.GetComponentInChildren<Unit>();
                        if (hitUnit.state != Unit.UnitState.DEAD && hitUnit.state != Unit.UnitState.RECOVERING)
                        {
                            hitUnit.reduceHPByAmount(currentWeapon.weaponDamage);
                            //hitUnit.reduceMovementSpeed();

                            currentWeapon.hitMarkerSound.Play();
                            interfaceController.putHitMarker();
                        }
                        // Create hit effect
                        //GameObject.Instantiate(shotImpact, hit.point, Quaternion.LookRotation(hit.normal), colliderOwner.transform);
                    }
                    else if(currentWeapon.shotImpact != null)
                        GameObject.Instantiate(currentWeapon.shotImpact, hit.point, Quaternion.LookRotation(hit.normal));
                }

                // Time
                currentWeapon.weaponFireRateCount = currentWeapon.weaponFireRate;
                if (currentWeapon.weaponMagazineCount > 0)
                {
                    currentWeapon.weaponMagazineCount--;

                    // Update UI
                    interfaceController.updateAmmoDisplay(currentWeapon.weaponMagazineCount, currentWeapon.totalAmmo);
                }
            }
            else
                currentWeapon.weaponFireRateCount -= Time.deltaTime;
        }
    }

    void checkReload()
    {
        if (currentWeapon.weaponMagazineCount == currentWeapon.weaponMagazineSize || currentWeapon.totalAmmo == 0)
            return;

            // Reload Button
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentWeapon.animator.SetBool("Reload", true);
                reloadAnimationLength = Utilities.getAnimationLengthByNameInAnimator("Reload", currentWeapon.animator);
                reloadAnimationCount = reloadAnimationLength;
                state = playerState.RELOADING;

                currentWeapon.reloadAudio.Play();
            }

        if (state == playerState.RELOADING)
        {
            if (reloadAnimationCount <= 0)
            {
                int offset = 0;
                currentWeapon.totalAmmo -= (currentWeapon.weaponMagazineSize - currentWeapon.weaponMagazineCount);

                if (currentWeapon.totalAmmo < 0)
                {
                    offset = currentWeapon.totalAmmo;
                    currentWeapon.totalAmmo = 0;
                }

                currentWeapon.weaponMagazineCount = currentWeapon.weaponMagazineSize + offset;
                currentWeapon.animator.SetBool("Reload", false);
                state = playerState.NONE;

                // Update UI
                interfaceController.updateAmmoDisplay(currentWeapon.weaponMagazineCount, currentWeapon.totalAmmo);
            }
            else
            {
                reloadAnimationCount -= Time.deltaTime;
            }
        }
    }

    void checkMeleeStrike()
    {
            if (Input.GetKeyDown(KeyCode.F))
            {
                currentWeapon.animator.SetBool("Melee Strike", true);
                strikeAnimationLenght = Utilities.getAnimationLengthByNameInAnimator("MeleeStrike", currentWeapon.animator)/2;
                strikeAnimationCount = strikeAnimationLenght;
                state = playerState.MELEE_STRIKE;
            }

        if (state == playerState.MELEE_STRIKE)
        {
            if (strikeAnimationCount < 0)
            {
                // Golpeamos al más cercano
                RaycastHit hit;
                Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, meleeRange);

                if (hit.collider)
                {
                    GameObject colliderOwner = hit.collider.gameObject;
                    //Debug.Log("Hit normal " + hit.normal);

                    // Check if it's an enemy
                    if (colliderOwner.GetComponentInChildren<Unit>())
                    {
                        Unit hitUnit = colliderOwner.GetComponentInChildren<Unit>();
                        if (hitUnit.state != Unit.UnitState.DEAD && hitUnit.state != Unit.UnitState.RECOVERING)
                        {
                            hitUnit.reduceHPByAmount(meleeDamage);
                            interfaceController.putHitMarker();
                        }
                    }
                }

                    currentWeapon.animator.SetBool("Melee Strike", false);
                state = playerState.NONE;
            }
            else
            {
                strikeAnimationCount -= Time.deltaTime;
            }
        }
    }

    // State checks

    void checkDeath()
    {
        if (hpCount <= 0)
        {
            transform.SetParent(GarbageController.garbageGo.transform);
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void receiveDamage(float damage)
    {
        hpCount -= damage;
        interfaceController.updateHP(hpCount, hp);
    }

    // Keys

    public void addKey(char keyId)
    {
        acquiredKeys.Add(keyId);
    }

    public bool hasKey(char keyId)
    {
        return acquiredKeys.Contains(keyId);
    }
}
