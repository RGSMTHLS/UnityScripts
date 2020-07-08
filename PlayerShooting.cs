using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This script should be placed at the Gun Barrel End
[RequireComponent(typeof(Light))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(ParticleSystem))]
public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 20;
    public float timeBetweenBullets = 0.15f;
    public float range = 100f;
    public bool shotgun = false;
    public Text debug;
    public int maxAmmo = 30;
    public float reloadTime = 2f;
    
    public AudioSource gunAudio;
    public AudioSource reloadAudio;
    WeaponSwitching weaponSwitching;

    public int currentAmmo;
    float timer;
    Ray shootRay;
    RaycastHit shootHit;
    int shootableMask;
    ParticleSystem gunParticles;
    LineRenderer gunLine;
    Light gunLight;
    float effectsDisplayTime = 0.2f;

    void Awake()
    {
        weaponSwitching = GetComponentInParent<WeaponSwitching>();
        currentAmmo = maxAmmo;
        shootableMask = LayerMask.GetMask("Shootable");
        gunParticles = GetComponent<ParticleSystem>();
        gunLine = GetComponent<LineRenderer>();
        gunLight = GetComponent<Light>();
    }

    void Update()
    {
        if(debug != null)
            debug.text = "transform.forward: " + transform.forward;
        timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!weaponSwitching.reloading && currentAmmo != maxAmmo)
            {
                Reload();
            }
        }

        if(Input.GetButton("Fire1") && timer >= timeBetweenBullets)
        {
            Shoot();
        }
        if(timer >= 0.15f * effectsDisplayTime)
        {
            DisableEffects();
        }
    }

    public void DisableEffects() {
        gunLine.enabled = false;
        gunLight.enabled = false;
    }

    public void Shoot() {
        if(currentAmmo > 0 && !weaponSwitching.reloading)
        {
            currentAmmo--;
            timer = 0f;
            gunAudio.Stop();
            gunAudio.Play();
            gunLight.enabled = true;

            gunParticles.Stop();
            gunParticles.Play();

            gunLine.enabled = true;
            gunLine.SetPosition(0, transform.position);

            shootRay.origin = transform.position;
            shootRay.direction = transform.forward;

            if(Physics.Raycast(shootRay, out shootHit, range, shootableMask))
            {
                Debug.Log(shootHit);
                EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();
                if(enemyHealth != null && shootHit.point != null)
                {
                    enemyHealth.TakeDamage(damagePerShot, shootHit.point);
                }
                else
                {
                    Explosive explosive = shootHit.collider.GetComponent<Explosive>();
                    if(explosive != null && shootHit.point != null)
                    {
                        explosive.TakeDamage(damagePerShot);
                    }
                }


                gunLine.SetPosition(1, shootHit.point);
            }
            else
            {
                gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
            }
        }
        else if(!weaponSwitching.reloading)
        {
            Reload();
        }
    }

    void Reload()
    {
        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        Debug.Log("Reloading");
        weaponSwitching.reloading = true;
        reloadAudio.Stop();
        reloadAudio.Play();
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        weaponSwitching.reloading = false;
        reloadAudio.Stop();
        Debug.Log("Finished Reloading");
    }
}
