using UnityEngine;

/// <summary>Objetivo estático experimental para validar impactos y daño.</summary>
public sealed class PrototypeTarget : MonoBehaviour
{
    [SerializeField, Min(1f)] private float hitPoints = 100f;
    [SerializeField] private Renderer hitRenderer;

    private MaterialPropertyBlock properties;
    private Color originalColor;
    private float flashTimer;
    private bool defeated;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (hitRenderer == null)
            hitRenderer = GetComponentInChildren<Renderer>();
        if (hitRenderer != null)
            originalColor = hitRenderer.sharedMaterial != null && hitRenderer.sharedMaterial.HasProperty(BaseColorId)
                ? hitRenderer.sharedMaterial.GetColor(BaseColorId)
                : Color.white;
        properties = new MaterialPropertyBlock();
    }

    public void ApplyDamage(float amount)
    {
        if (defeated || amount <= 0f)
            return;

        hitPoints -= amount;
        flashTimer = 0.12f;
        SetHitColor(Color.white);

        if (hitPoints <= 0f)
        {
            defeated = true;
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (flashTimer <= 0f)
            return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f)
            SetHitColor(originalColor);
    }

    private void SetHitColor(Color color)
    {
        if (hitRenderer == null)
            return;

        hitRenderer.GetPropertyBlock(properties);
        properties.SetColor(BaseColorId, color);
        properties.SetColor(ColorId, color);
        hitRenderer.SetPropertyBlock(properties);
    }
}
