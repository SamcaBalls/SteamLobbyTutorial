using Mirror;
using Mirror.Examples.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerCameraLook : NetworkBehaviour
{
    [Header("References")]
    public Transform playerBody; // objekt hr��e, kolem kter�ho se kamera ot��� (v�t�inou parent)

    [Header("Settings")]
    public float sensitivity = 100f;
    public float clampAngle = 85f;

    private Vector2 lookInput;
    private float xRotation = 0f;

    [SerializeField] private InputActionReference look;
    [SerializeField] private Camera playerCamera;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.tag = "MainCamera"; // d�le�it�, aby fungovalo Camera.main
            playerCamera.GetComponent<AudioListener>().enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Vypne kamery u jin�ch hr���
        if (!isLocalPlayer && playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
            playerCamera.GetComponent<AudioListener>().enabled = false;
        }
    }

    // Input System callback
    void Update()
    {
        lookInput = look.action.ReadValue<Vector2>();

        // my�/joystick input
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        // vertik�ln� rotace (kamera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // horizont�ln� rotace (t�lo hr��e)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
