using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static MonoBehaviour Instance { get; private set; }
    public static AudioSource audioSource;
    private static float stepVol = 1f;
    private static float atkVol = 1f;
    private static float hitVol = 1f;
    private static float UIVol = 1f;
    //load each sfx in once
    private static AudioClip grassWalk;
    private static List<AudioClip> stoneWalks = new List<AudioClip>();
    private static AudioClip strikeSFX;
    private static AudioClip slashSFX;
    private static AudioClip pierceSFX;
    private static AudioClip playerHit;
    private static AudioClip enemyHit;
    private static AudioClip partBreak;
    private static AudioClip UISelect;



    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        //load in sfx assets
        grassWalk = Resources.Load<AudioClip>("Audio/grassFootsteps");
        stoneWalks.Add(Resources.Load<AudioClip>("Audio/stoneFootsteps1"));
        stoneWalks.Add(Resources.Load<AudioClip>("Audio/stoneFootsteps2"));
        strikeSFX = Resources.Load<AudioClip>("Audio/strikeSfx");
        slashSFX = Resources.Load<AudioClip>("Audio/slashSfx");
        pierceSFX = Resources.Load<AudioClip>("Audio/pierceSfx");
        playerHit = Resources.Load<AudioClip>("Audio/playerHit");
        enemyHit = Resources.Load<AudioClip>("Audio/enemyHit");
        partBreak = Resources.Load<AudioClip>("Audio/partBreak");
        UISelect = Resources.Load<AudioClip>("Audio/UISelect");
    }

    public static void playFootsteps(bool grass)
    {
        //randomize pitch within range
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        if (grass)
        {
            audioSource.PlayOneShot(grassWalk, stepVol);
        }
        else
        {
            //pick a sound randomly
            int i = Random.Range(0, stoneWalks.Count);
            audioSource.PlayOneShot(stoneWalks[i], stepVol);
        }
        //audioSource.pitch = 1f;
    }

    public static void playWeaponAttack(string type)
    {
        //randomize pitch within range
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        switch (type.ToLower())
        {
            case "strike":
                audioSource.PlayOneShot(strikeSFX, atkVol);
                break;
            case "pierce":
                audioSource.PlayOneShot(pierceSFX, atkVol);
                break;
            case "slash":
                audioSource.PlayOneShot(slashSFX, atkVol);
                break;
        }
    }

    public static void playPlayerHit()
    {
        //increase volume for more damaging hits?
        audioSource.PlayOneShot(playerHit, hitVol);
    }

    public static void playEnemyHit(float effectiveness)
    {
        //increase volume and pitch for effective hits
        audioSource.pitch = Random.Range(0.9f + effectiveness/10, 1.1f + effectiveness/10);
        audioSource.PlayOneShot(enemyHit, hitVol);
    }

    public static void playPartBreak()
    {
        audioSource.PlayOneShot(partBreak, hitVol);
    }

    public static void playUISelect()
    {
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(UISelect, UIVol);
    }
}
