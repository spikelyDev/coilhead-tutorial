using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Mouse Settings")]   
    [SerializeField] private float mouseSensitivity = 3.5f;
    [SerializeField][Range(0.0f, 0.5f)] private float mouseSmoothTime = 0.03f;
    public Camera playerCamera;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float gravity = -13.0f;

    private float cameraPitch = 0.0f;
    private float velocityY = 0.0f;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;
    private CharacterController controller;

    private void Start() {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        UpdateMouseLook();
        UpdateMovement();
    }

    private void UpdateMouseLook() {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, mouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(currentMouseDelta.x * mouseSensitivity * Vector3.up);
    }

    private void UpdateMovement() {
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if(controller.isGrounded) {
            velocityY = 0.0f;
        }
        velocityY += gravity * Time.deltaTime;

        Vector3 velocity = (transform.forward * direction.z + transform.right * direction.x) * walkSpeed + Vector3.up * velocityY;
        controller.Move(velocity * Time.deltaTime);
    }
}
