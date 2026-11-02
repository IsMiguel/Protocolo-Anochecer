using UnityEngine;

namespace PCGG.ProtocoloAnochecer.Prototype.Player
{
    /// <summary>
    /// EXPERIMENTAL. Movimiento en primera persona con CharacterController:
    /// caminar, correr (Shift, solo hacia adelante) y gravedad simple.
    /// Los valores numéricos son provisionales y se ajustan en el Inspector.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(FPSInput))]
    public sealed class FPSPlayerController : MonoBehaviour
    {
        [Header("Velocidades (m/s) - provisionales")]
        [SerializeField] float walkSpeed = 3.5f;
        [SerializeField] float sprintSpeed = 6f;

        [Header("Gravedad - provisional")]
        [SerializeField] float gravity = -20f;

        CharacterController _controller;
        FPSInput _input;
        float _verticalVelocity;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<FPSInput>();
        }

        void Update()
        {
            Vector2 move = _input.Move;
            bool sprinting = _input.SprintHeld && move.y > 0.1f;
            float speed = sprinting ? sprintSpeed : walkSpeed;

            Vector3 wish = transform.right * move.x + transform.forward * move.y;
            wish = Vector3.ClampMagnitude(wish, 1f) * speed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f; // mantiene el contacto con el suelo/rampas

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = wish + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}

