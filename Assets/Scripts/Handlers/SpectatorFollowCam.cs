using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class SpectatorFollowCam : NetworkBehaviour
{
    [Header("Follow Cam Settings")]
    public float distance = 5f;
    public float height = 2f;
    public float rotationSpeed = 5f;

    private List<PlayerStats> livePlayers = new List<PlayerStats>();
    private int currentIndex = 0;

    [SerializeField] private Camera cam;

    private float orbitY = 0f; // horizontální úhel
    private float orbitX = 20f; // vertikální úhel

    public void ActivateSpectator()
    {
        if (!isLocalPlayer) return;

        cam.gameObject.SetActive(true);
        UpdateLivePlayers();
        FollowCurrentPlayer();
    }

    private void Update()
    {
        if (!cam.enabled || livePlayers.Count == 0) return;

        HandleMouseInput();
        FollowCurrentPlayer();

        // pøepínání hráèù
        if (Input.GetKeyDown(KeyCode.Mouse0)) NextPlayer();
        if (Input.GetKeyDown(KeyCode.Mouse1)) PreviousPlayer();
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        orbitY += mouseX;
        orbitX -= mouseY;
        orbitX = Mathf.Clamp(orbitX, 10f, 80f); // omezení vertikálního úhlu
    }

    private void UpdateLivePlayers()
    {
        livePlayers.Clear();
        foreach (var ps in FindObjectsByType<PlayerStats>(FindObjectsSortMode.InstanceID))
            if (ps.health > 0) livePlayers.Add(ps);

        if (currentIndex >= livePlayers.Count) currentIndex = 0;
    }

    private void FollowCurrentPlayer()
    {
        if (livePlayers.Count == 0) return;

        Transform target = livePlayers[currentIndex].transform;

        // vypoèítáme orbitální pozici kamery
        Quaternion rotation = Quaternion.Euler(orbitX, orbitY, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPos = target.position + Vector3.up * height + offset;

        // kolize s terénem / objekty
        RaycastHit hit;
        if (Physics.Linecast(target.position + Vector3.up * 1.5f, targetPos, out hit))
        {
            targetPos = hit.point - (hit.point - (target.position + Vector3.up * 1.5f)).normalized * 0.2f;
        }

        cam.transform.position = targetPos;
        cam.transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private void NextPlayer()
    {
        if (livePlayers.Count == 0) return;
        currentIndex = (currentIndex + 1) % livePlayers.Count;
    }

    private void PreviousPlayer()
    {
        if (livePlayers.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = livePlayers.Count - 1;
    }
}
