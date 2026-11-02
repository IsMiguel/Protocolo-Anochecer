using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Prototipo experimental de rifle y pistola en primera persona.</summary>
public sealed class FirstPersonWeaponController : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private GameObject rifleView;
    [SerializeField] private GameObject pistolView;
    [SerializeField] private Light muzzleFlash;
    [SerializeField] private Text weaponLabel;
    [SerializeField] private Text ammunitionLabel;
    [SerializeField] private Text statusLabel;
    [SerializeField, Min(1f)] private float range = 150f;

    private const int RifleCapacity = 5;
    private const int PistolCapacity = 7;
    private int rifleRounds = RifleCapacity;
    private int rifleReserve = 35;
    private int pistolRounds = PistolCapacity;
    private int pistolReserve = 28;
    private int activeWeapon;
    private float nextShotTime;
    private float reloadTime;
    private float muzzleTime;
    private float recoil;
    private Vector3 weaponRestPosition;

    private void Awake()
    {
        if (weaponRoot != null)
            weaponRestPosition = weaponRoot.localPosition;
    }

    private void Start()
    {
        SetWeapon(0);
        RefreshHud();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (muzzleTime > 0f)
        {
            muzzleTime -= Time.deltaTime;
            if (muzzleTime <= 0f && muzzleFlash != null)
                muzzleFlash.enabled = false;
        }

        if (reloadTime > 0f)
        {
            reloadTime -= Time.deltaTime;
            if (reloadTime <= 0f)
                FinishReload();
        }

        if (keyboard != null && reloadTime <= 0f)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) SetWeapon(0);
            if (keyboard.digit2Key.wasPressedThisFrame) SetWeapon(1);
            if (keyboard.rKey.wasPressedThisFrame) BeginReload();
        }

        if (Cursor.lockState == CursorLockMode.Locked && reloadTime <= 0f &&
            mouse != null && mouse.leftButton.wasPressedThisFrame)
            Fire();
    }

    private void LateUpdate()
    {
        if (weaponRoot == null)
            return;

        recoil = Mathf.Lerp(recoil, 0f, 14f * Time.deltaTime);
        Vector3 target = weaponRestPosition + Vector3.back * recoil;
        weaponRoot.localPosition = Vector3.Lerp(weaponRoot.localPosition, target, 16f * Time.deltaTime);
    }

    private void Fire()
    {
        if (Time.time < nextShotTime)
            return;

        if (CurrentRounds() <= 0)
        {
            SetStatus("Sin munición · R para recargar");
            return;
        }

        if (activeWeapon == 0)
        {
            rifleRounds--;
            nextShotTime = Time.time + 0.72f;
        }
        else
        {
            pistolRounds--;
            nextShotTime = Time.time + 0.28f;
        }

        recoil = Mathf.Min(recoil + (activeWeapon == 0 ? 0.065f : 0.035f), 0.16f);
        muzzleTime = 0.055f;
        if (muzzleFlash != null)
            muzzleFlash.enabled = true;

        if (aimCamera != null)
        {
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                PrototypeTarget target = hit.collider.GetComponentInParent<PrototypeTarget>();
                if (target != null)
                    target.ApplyDamage(activeWeapon == 0 ? 60f : 35f);
            }
        }

        SetStatus(string.Empty);
        RefreshHud();
    }

    private void BeginReload()
    {
        if (CurrentRounds() >= CurrentCapacity() || CurrentReserve() <= 0)
            return;

        reloadTime = activeWeapon == 0 ? 1.6f : 1.15f;
        SetStatus("RECARGANDO");
    }

    private void FinishReload()
    {
        if (activeWeapon == 0)
        {
            int loaded = Mathf.Min(RifleCapacity - rifleRounds, rifleReserve);
            rifleRounds += loaded;
            rifleReserve -= loaded;
        }
        else
        {
            int loaded = Mathf.Min(PistolCapacity - pistolRounds, pistolReserve);
            pistolRounds += loaded;
            pistolReserve -= loaded;
        }

        SetStatus(string.Empty);
        RefreshHud();
    }

    private void SetWeapon(int index)
    {
        activeWeapon = Mathf.Clamp(index, 0, 1);
        if (rifleView != null) rifleView.SetActive(activeWeapon == 0);
        if (pistolView != null) pistolView.SetActive(activeWeapon == 1);
        if (muzzleFlash != null)
            muzzleFlash.transform.localPosition = activeWeapon == 0
                ? new Vector3(0.18f, -0.21f, 0.55f)
                : new Vector3(0.15f, -0.20f, 0.24f);
        SetStatus(string.Empty);
        RefreshHud();
    }

    private int CurrentRounds() => activeWeapon == 0 ? rifleRounds : pistolRounds;
    private int CurrentReserve() => activeWeapon == 0 ? rifleReserve : pistolReserve;
    private int CurrentCapacity() => activeWeapon == 0 ? RifleCapacity : PistolCapacity;

    private void RefreshHud()
    {
        if (weaponLabel != null)
            weaponLabel.text = activeWeapon == 0 ? "MAUSER 1909 · PROVISIONAL" : "M1911A1 · PROVISIONAL";
        if (ammunitionLabel != null)
            ammunitionLabel.text = $"{CurrentRounds():00} / {CurrentReserve():00}";
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }

    private void OnDisable()
    {
        if (muzzleFlash != null)
            muzzleFlash.enabled = false;
    }
}
