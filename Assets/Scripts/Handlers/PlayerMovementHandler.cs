using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

namespace SteamLobbyTutorial
{
    public class PlayerMovementHandler : NetworkBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference move;

        [Header("Movement Settings")]
        [SerializeField] private float normalSpeed = 5f;
        [SerializeField] private float sandSpeed = 2.5f;

        [HideInInspector] public float moveSpeed = 0;

        [HideInInspector] public CharacterController controller;
        private bool isInSand = false;

        public override void OnStartLocalPlayer()
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("⚠️ Player prefab needs a CharacterController component!");
                return;
            }

            move.action.Enable();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (!isOwned) return; // nebo if (!hasAuthority) return;


            Vector2 input = move.action.ReadValue<Vector2>();
            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

            moveSpeed = isInSand ? sandSpeed : normalSpeed;

            Vector3 moveVector = transform.TransformDirection(direction) * moveSpeed;
            controller.SimpleMove(moveVector);
        }

        // detekce kolize pro CharacterController
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.CompareTag("Sand"))
                isInSand = true;
            else
                isInSand = false;
        }
    }
}
