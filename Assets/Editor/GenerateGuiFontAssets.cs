using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

public static class GenerateGuiFontAssets
{
    const string MontenegrinTtf = "Assets/UI/Fonts/MontenegrinGothicOne-Regular.ttf";
    const string MontenegrinSdf = "Assets/UI/Fonts/MontenegrinGothicOne-Regular SDF.asset";
    const string PlaywriteTtf = "Assets/UI/Fonts/PlaywriteAUTAS-VariableFont_wght.ttf";
    const string PlaywriteSdf = "Assets/UI/Fonts/PlaywriteAUTAS-VariableFont_wght SDF.asset";

    [MenuItem("Tools/Generar Font Assets TMP para GUI")]
    public static void Generate()
    {
        CreateFontAsset(MontenegrinTtf, MontenegrinSdf);
        CreateFontAsset(PlaywriteTtf, PlaywriteSdf);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Font Assets TMP generados en Assets/UI/Fonts.");
    }

    [MenuItem("Tools/Aplicar MontenegrinGothicOne a todos los textos")]
    public static void ApplyMontenegrinToAllTexts()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MontenegrinSdf);
        if (font == null)
        {
            Debug.LogError("Primero corre 'Tools > Generar Font Assets TMP para GUI' para crear el Font Asset.");
            return;
        }

        int count = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    Undo.RecordObject(text, "Apply Font");
                    text.font = font;
                    EditorUtility.SetDirty(text);
                    count++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"Fuente MontenegrinGothicOne-Regular aplicada a {count} textos en las escenas abiertas.");
    }

    static void CreateFontAsset(string sourcePath, string destPath)
    {
        if (File.Exists(destPath))
        {
            Debug.Log($"Ya existe {destPath}, se omite.");
            return;
        }

        Font font = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
        if (font == null)
        {
            Debug.LogError($"No se encontro la fuente en {sourcePath}");
            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);

        if (fontAsset == null)
        {
            Debug.LogError($"Fallo la creacion del Font Asset para {sourcePath}");
            return;
        }

        AssetDatabase.CreateAsset(fontAsset, destPath);

        if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);

        if (fontAsset.material != null)
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        EditorUtility.SetDirty(fontAsset);
        Debug.Log($"Creado: {destPath}");
    }
}
