using UnityEngine;
using UnityEngine.InputSystem;

namespace PCGG.ProtocoloAnochecer.Prototype.Player
{
    /// <summary>
    /// EXPERIMENTAL. Cámara con mouse: el yaw rota el cuerpo (este objeto) y el
    /// pitch rota la cámara hija. Esc libera el cursor; clic izquierdo lo vuelve a bloquear.
    /// </summary>
    [RequireComponent(typeof(FPSInput))]
    public sealed class FPSCameraLook : MonoBehaviour
    {
        [Tooltip("Si queda vacío se usa la primera Camera hija.")]
        [SerializeField] Transform cameraPivot;

        [Header("Mouse - provisional")]
        [SerializeField, Min(0.001f)] float sensitivity = 0.1f; // grados por píxel
        [SerializeField, Range(10f, 89f)] float maxPitch = 85f;

        FPSInput _input;
        float _pitch;

        void Awake()
        {
            _input = GetComponent<FPSInput>();
            if (cameraPivot == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraPivot = cam.transform;
            }
            if (cameraPivot == null)
                Debug.LogError("FPSCameraLook: no hay cámara hija ni cameraPivot asignado.", this);
        }

        void OnEnable() => SetCursorLocked(true);
        void OnDisable() => SetCursorLocked(false);

        void Update()
        {
            HandleCursorState();
            if (Cursor.lockState != CursorLockMode.Locked || cameraPivot == null) return;

            Vector2 look = _input.Look * sensitivity; // delta de mouse: no se multiplica por deltaTime

            transform.Rotate(0f, look.x, 0f, Space.Self);

            _pitch = Mathf.Clamp(_pitch - look.y, -maxPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        void HandleCursorState()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                SetCursorLocked(false);
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                SetCursorLocked(true);
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}

