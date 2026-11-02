using UnityEngine;
using UnityEngine.InputSystem;

namespace PCGG.ProtocoloAnochecer.Prototype.Player
{
    /// <summary>
    /// EXPERIMENTAL. Lector de entrada del prototipo FPS.
    /// Usa las acciones de proyecto (InputSystem.actions = Assets/InputSystem_Actions.inputactions):
    /// Player/Move, Player/Look y Player/Sprint. Si no hay acciones de proyecto
    /// o faltan esas acciones, usa un respaldo por código (teclado + mouse).
    /// No modifica el asset de acciones ni ajustes globales.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FPSInput : MonoBehaviour
    {
        InputAction _move;
        InputAction _look;
        InputAction _sprint;
        bool _ownsActions; // true solo si se crearon por código (respaldo)

        /// <summary>x = lateral, y = adelante/atrás.</summary>
        public Vector2 Move => _move.ReadValue<Vector2>();

        /// <summary>Delta de mirada de este frame (mouse: píxeles).</summary>
        public Vector2 Look => _look.ReadValue<Vector2>();

        public bool SprintHeld => _sprint.IsPressed();

        void Awake()
        {
            var asset = InputSystem.actions;
            _move = asset != null ? asset.FindAction("Player/Move", false) : null;
            _look = asset != null ? asset.FindAction("Player/Look", false) : null;
            _sprint = asset != null ? asset.FindAction("Player/Sprint", false) : null;

            if (_move != null && _look != null && _sprint != null) return;

            Debug.LogWarning("FPSInput: no se encontraron Player/Move, Look y Sprint en InputSystem.actions. Usando respaldo por código.", this);
            _ownsActions = true;

            _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");
            _sprint = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
        }

        void OnEnable()
        {
            // Las acciones de proyecto se habilitan solas; por si acaso, solo se activan, nunca se desactivan.
            if (!_move.enabled) _move.Enable();
            if (!_look.enabled) _look.Enable();
            if (!_sprint.enabled) _sprint.Enable();
        }

        void OnDisable()
        {
            if (!_ownsActions) return;
            _move.Disable();
            _look.Disable();
            _sprint.Disable();
        }

        void OnDestroy()
        {
            if (!_ownsActions) return;
            _move.Dispose();
            _look.Dispose();
            _sprint.Dispose();
        }
    }
}

