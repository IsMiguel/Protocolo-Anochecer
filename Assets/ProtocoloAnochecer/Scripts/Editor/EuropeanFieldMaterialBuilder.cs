using UnityEditor;
using UnityEngine;

public static class EuropeanFieldMaterialBuilder
{
    private const string AlbedoPath = "Assets/ProtocoloAnochecer/Art/Textures/European_Field_Albedo.png";
    private const string NormalPath = "Assets/ProtocoloAnochecer/Art/Textures/European_Field_Normal.png";
    private const string MaterialPath = "Assets/ProtocoloAnochecer/Art/Materials/European_Field.mat";

    public static Material GetOrCreate(Material fallback)
    {
        Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath);
        if (albedo == null)
            return fallback;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
            return fallback;

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = "European Field" };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", albedo);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", albedo);
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.16f);

        Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath);
        if (normal != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
            if (material.HasProperty("_BumpScale"))
                material.SetFloat("_BumpScale", 0.24f);
        }
        else
        {
            material.DisableKeyword("_NORMALMAP");
        }

        EditorUtility.SetDirty(material);
        return material;
    }
}