using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : Unit
{
    public ParticleSystem muzzleEffect;
    public AudioSource shootAudioEffect;

    // Shooting params
    int attackBurst = 0;
    public float shootingRate = 0.075f;
    float shootingRateCount = 0;

    override
    public void startAttack()
    {
        base.startAttack();
        attackCastLenghCount = 0.25f;
        attackBurst = Random.Range(2, 6);
    }

    override
    public void performAttack()
    {
        if (canShoot())
            shoot();

        if (attackBurst == 0)
            state = UnitState.IN_COMBAT;
        else
            state = UnitState.ATTACKING;
    }

    void shoot()
    {
        // Update rotation
        transform.rotation = Quaternion.LookRotation(Player.transform.position - transform.position);

        // Shooting effects
        muzzleEffect.Play();
        shootAudioEffect.Play();

        // Cast Ray
        RaycastHit info = castRayTowards(attackDirection);
        bool result = info.transform.gameObject == Player;

        // Check hit
        if (result)
        {
            Player playerObject = Player.GetComponent<Player>();
            if (playerObject)
                playerObject.receiveDamage(damage);
        }

        // Update next values
        shootingRateCount = shootingRate;
        attackBurst--;
        attackDirection = Player.transform.position - transform.position;
    }

    bool canShoot()
    {
        shootingRateCount -= Time.deltaTime;
        if (shootingRateCount < 0)
            return true;
        else
            return false;
    }

    override
    public void generateStatsByLevel()
    {
        healthPoints = baseHealthPoints + 5 * level;
        damage = baseDamage + 0.25f * level;
        attackRate = baseAtttackRate - 0.25f * level;
        combatDistance = baseCombatDistance + 0.5f * level;
        recoverTimer = baseRecoverTimer - 2.5f * level;
    }
}
