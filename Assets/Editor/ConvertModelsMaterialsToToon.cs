using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ConvertModelsMaterialsToToon
{
    const string RootFolder = "Assets/Models";
    static readonly string[] ModelExtensions = { ".obj", ".fbx", ".dae", ".gltf", ".glb", ".stl" };

    [MenuItem("Tools/Convertir Materiales de Assets Models a Toon")]
    public static void Convert()
    {
        Shader toonShader = Shader.Find("Toon/Toon");
        if (toonShader == null)
        {
            Debug.LogError("No se encontro el shader Toon/Toon. Verifica que el paquete com.unity.toonshader este instalado.");
            return;
        }

        var processed = new HashSet<Material>();
        int count = 0;

        // Pass 1: materiales embebidos dentro de los archivos de modelo (obj/fbx/etc).
        // Si estan embebidos, primero se extraen a un .mat externo para que la conversion
        // no se pierda en el proximo reimport del modelo.
        foreach (var file in Directory.GetFiles(RootFolder, "*.*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (System.Array.IndexOf(ModelExtensions, ext) < 0) continue;

            string modelPath = file.Replace('\\', '/');
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            {
                if (obj is not Material mat || !processed.Add(mat)) continue;
                if (!AssetDatabase.IsSubAsset(mat)) continue; // ya es un .mat externo, lo agarra el Pass 2

                string folder = Path.GetDirectoryName(modelPath).Replace('\\', '/') + "/Materials";
                if (!AssetDatabase.IsValidFolder(folder))
                    AssetDatabase.CreateFolder(Path.GetDirectoryName(modelPath).Replace('\\', '/'), "Materials");

                string extractPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{mat.name}.mat");
                string error = AssetDatabase.ExtractAsset(mat, extractPath);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"No se pudo extraer '{mat.name}' de {modelPath}: {error}");
                    continue;
                }

                Material extracted = AssetDatabase.LoadAssetAtPath<Material>(extractPath);
                if (extracted != null && ConvertMaterial(extracted, toonShader)) count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Pass 2: materiales .mat externos (los ya existentes + los recien extraidos arriba).
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { RootFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".mat")) continue;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && processed.Add(mat) && ConvertMaterial(mat, toonShader))
                count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Convertidos {count} materiales a Toon/Toon dentro de {RootFolder}.");
    }

    static bool ConvertMaterial(Material mat, Shader toonShader)
    {
        if (mat.shader != null && mat.shader.name == "Toon/Toon")
            return false; // ya convertido

        Color baseColor = Color.white;
        if (mat.HasProperty("_Color"))
            baseColor = mat.GetColor("_Color");
        else if (mat.HasProperty("_BaseColor"))
            baseColor = mat.GetColor("_BaseColor");

        mat.shader = toonShader;

        Color shade = baseColor * 0.55f;
        shade.a = 1f;

        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_1st_ShadeColor", shade);
        mat.SetColor("_2nd_ShadeColor", shade);
        mat.SetColor("_HighColor", Color.black);
        mat.SetColor("_Outline_Color", new Color(0.05f, 0.05f, 0.05f, 1f));

        mat.SetFloat("_BaseColor_Step", 0.5f);
        mat.SetFloat("_BaseShade_Feather", 0.0001f);
        mat.SetFloat("_ShadeColor_Step", 0f);
        mat.SetFloat("_1st2nd_Shades_Feather", 0.0001f);
        mat.SetFloat("_Use_BaseAs1st", 0f);
        mat.SetFloat("_Use_1stAs2nd", 1f);
        mat.SetFloat("_Outline_Width", 1.5f);
        mat.SetFloat("_Is_LightColor_Outline", 0f);
        mat.SetFloat("_RimLight", 0f);
        mat.SetFloat("_MatCap", 0f);
        mat.SetFloat("_AngelRing", 0f);
        mat.SetFloat("_CullMode", 2f);
        mat.SetFloat("_ZWriteMode", 1f);

        EditorUtility.SetDirty(mat);
        return true;
    }
}
