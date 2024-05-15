using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using Photon.Realtime;

public class Weapon : MonoBehaviour
{
    public Image ammoCircle;

    public int damage;
    public float fireRate;

    public Camera camera;

    private float nextFire;

    [Header("VFX")]
    public GameObject hitVFX;
    public AudioSource gunfireSound;


    [Header("Ammo")]
    public int mags = 5;
    public int ammoInOneMag = 30;
    public int currentCountOfAmmo = 30;

    [Header("UI")]
    public TextMeshProUGUI magsText;
    public TextMeshProUGUI currentCountOfAmmoText;

    [Header("Animation")]
    public Animation animation;
    public AnimationClip reload;

    [Header("Recoil Settings")]
    // [Range(0, 1)]
    // public float recoilPersent = 0.3f;

    [Range(0, 2)]
    public float recoverPersent = 0.7f;
    [Space]
    public float recoilUp = 1f;

    public float recoilBack = 0f;



    private Vector3 originalPosition;
    private Vector3 recoilVelocity = Vector3.zero;

    private float recoilLength;
    private float recoverLenth;



    private bool recoiling;
    public bool recovering;


    void SetAmmo()
    {
        ammoCircle.fillAmount = (float)ammoInOneMag / currentCountOfAmmo;
    }


    void Start()
    {
        magsText.text = mags.ToString();
        currentCountOfAmmoText.text = ammoInOneMag + "/" + currentCountOfAmmo;

        SetAmmoUIText();

        originalPosition = transform.localPosition;

        recoilLength = 0;
        recoverLenth = 1 / fireRate * recoverPersent;

        gunfireSound = GetComponentInChildren<AudioSource>();
        if (gunfireSound == null)
        {
            Debug.LogError("AudioSource not found! Make sure it's attached to the Weapon object or its children.");
        }
    }

    void Update()
    {

        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }


        if (Input.GetMouseButton(0) && nextFire <= 0 && currentCountOfAmmo  > 0 && animation.isPlaying == false)
        {
            nextFire = 1 / fireRate;

            currentCountOfAmmo--;

            magsText.text = mags.ToString();
            currentCountOfAmmoText.text = ammoInOneMag + "/" + currentCountOfAmmo;

            SetAmmoUIText();

            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R) && mags > 0)
        {
            Reload();
        }
        

        if (recoiling)
        {
            Recoil();
        }


        if (recovering)
        {
            Recovering();
        }
    }

    void Fire()
    {
        recoiling = true;
        recovering = false;

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        RaycastHit hit;

        PhotonNetwork.LocalPlayer.AddScore(1);

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
        {
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

            if (hit.transform.gameObject.GetComponent<Health>())
            {
                PhotonNetwork.LocalPlayer.AddScore(damage);

                if (damage >= hit.transform.gameObject.GetComponent<Health>().health)
                {
                    RoomManager.instance.kills++;
                    RoomManager.instance.SetHashes();
                    PhotonNetwork.LocalPlayer.AddScore(100);
                }

                hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, damage);
            }
        }

        gunfireSound.Play();
    }

    public void Reload() {
        if (mags > 0)
        {
            animation.Play(reload.name);
            mags--;
            currentCountOfAmmo = ammoInOneMag;
        }

        magsText.text = mags.ToString();
        currentCountOfAmmoText.text = ammoInOneMag + "/" + currentCountOfAmmo;

        SetAmmoUIText();
    }

    public void SetAmmoUIText()
    {
        magsText.text = mags.ToString();

        if (currentCountOfAmmo < 10) 
            currentCountOfAmmoText.text = "0" + currentCountOfAmmo + "/" + ammoInOneMag;
        else
            currentCountOfAmmoText.text = currentCountOfAmmo + "/" + ammoInOneMag;
    }
    
    
    void Recoil()
    {
        Vector3 finalPosition = new Vector3(originalPosition.x, originalPosition.y + recoilUp, originalPosition.z - recoilBack);

        transform.localPosition = 
            Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLength);

        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = true;
        } 
    }

    void Recovering()
    {
        Vector3 finalPosition = originalPosition;

        transform.localPosition =
            Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLenth);

        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = false;
        }
    }

}