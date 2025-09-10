using Mirror;
using SteamLobbyTutorial;
using System.Collections;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [Header("Stats")]
    [SyncVar] public int health = 100;
    [SyncVar] public float fatigue = 0f;

    [Header("Config")]
    public int maxHealth = 100;
    public float maxFatigue = 100f;

    public float baseFatigueRate = 1f;     // pasivní únava za sekundu
    public float runFatigueRate = 5f;      // extra únava za sekundu při běhu
    public float fatigueRecoveryRate = 2f; // regenerace za sekundu (když třeba sedí)
    bool isSafe = false;
    bool ragdoll = false;
    public bool lockinIn = false;

    void Start()
    {
        if (isLocalPlayer)
        {
            // aktivujeme hlavní kameru při spawn
            Camera mainCam = GetComponentInChildren<Camera>();
            if (mainCam != null) mainCam.enabled = true;
        }
    }


    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleFatigue();

        // test: ubrání HP klávesou K
        if (Input.GetKeyDown(KeyCode.K))
        {
            CmdTakeDamage(10);
        }
    }

    void HandleFatigue()
    {
        if (lockinIn) return;

        float delta = baseFatigueRate * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift)) // běží → víc únavy
            delta += runFatigueRate * Time.deltaTime;

        CmdChangeFatigue(fatigue + delta);

        if (fatigue >= maxFatigue && !isSafe)
        {
            ragdoll = true;
            RpcOnBurnout();
        }
        if (fatigue < maxFatigue)
            ragdoll = false;

    }

    [Command]
    public void CmdTakeDamage(int amount)
    {
        health = Mathf.Max(health - amount, 0);
        if (health <= 0)
            RpcOnDeath();
    }

    [Command]
    public void CmdChangeFatigue(float value)
    {
        fatigue = Mathf.Clamp(value, 0, maxFatigue);
    }

    [ClientRpc]
    void RpcOnDeath()
    {
        Debug.Log($"{netIdentity.netId} zemřel!");

        if (isLocalPlayer)
        {
            // deaktivujeme player input
            var move = GetComponent<PlayerMovementHandler>();
            if (move != null) move.enabled = false;

            var camMove = GetComponent<PlayerCameraLook>();
            if(camMove != null) camMove.enabled = false;

            // deaktivujeme hlavní kameru
            Camera mainCam = GetComponentInChildren<Camera>();
            if (mainCam != null)
            {
                mainCam.enabled = false;
                mainCam.GetComponent<AudioListener>().enabled = false;
            }

            // aktivujeme spectator kameru
            var specCam = GetComponentInChildren<SpectatorFollowCam>(true);
            if (specCam != null)
                specCam.ActivateSpectator();
        }
    }


    [ClientRpc]
    void RpcOnBurnout()
    {
        Debug.Log($"{netIdentity.netId} omdlel!");
        //Ragdoll
    }

    public IEnumerator LockIn()
    {
        lockinIn = true;
        while (fatigue > 0)
        {
            yield return new WaitForSeconds(0.01f);
            CmdChangeFatigue(fatigue - 1);
        }
    }

}
