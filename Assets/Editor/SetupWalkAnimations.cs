using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupWalkAnimations
{
    [MenuItem("Tools/Configurar Animacion de Caminata (Player y Enemy)")]
    public static void Setup()
    {
        var player = Object.FindFirstObjectByType<PlayerStateMachine>();
        SetupCharacter(
            player != null ? player.gameObject : null,
            "Assets/Models/Characters/Adventurer women.fbx",
            "Assets/Animation/Player/AdventurerWomenAnimator.controller",
            paramName: "isRunning");

        if (player != null) AlignDetectionPoint(player);

        var enemy = Object.FindFirstObjectByType<EnemyController>();
        SetupCharacter(
            enemy != null ? enemy.gameObject : null,
            "Assets/Models/Characters/Adventurer men.fbx",
            "Assets/Animation/Player/AdventurerMenAnimator.controller",
            paramName: "isWalking");

        AssetDatabase.SaveAssets();
        Debug.Log("Animator Controllers creados y asignados. Revisa en el Editor y guarda la escena con Ctrl+S.");
    }

    static void SetupCharacter(GameObject root, string modelPath, string controllerPath, string paramName)
    {
        if (root == null)
        {
            Debug.LogWarning($"No se encontro el GameObject correspondiente a {modelPath} en la escena abierta.");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        var clips = subAssets.OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToList();
        var avatar = subAssets.OfType<Avatar>().FirstOrDefault();

        AnimationClip idleClip = clips.FirstOrDefault(c => c.name.ToLower().Contains("idle"));
        AnimationClip walkClip = clips.FirstOrDefault(c => c.name.ToLower().Contains("walk"))
                                   ?? clips.FirstOrDefault(c => c != idleClip);

        if (idleClip == null || walkClip == null)
        {
            Debug.LogError($"No se encontraron clips Idle/Walk en {modelPath}. Clips disponibles: {string.Join(", ", clips.Select(c => c.name))}");
            return;
        }

        if (avatar == null)
            Debug.LogWarning($"No se encontro un Avatar Humanoid en {modelPath}; el Animator puede no funcionar correctamente.");

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter(paramName, AnimatorControllerParameterType.Bool);

            var rootSM = controller.layers[0].stateMachine;
            var idleState = rootSM.AddState("Idle");
            idleState.motion = idleClip;
            var walkState = rootSM.AddState("Walk");
            walkState.motion = walkClip;
            rootSM.defaultState = idleState;

            var toWalk = idleState.AddTransition(walkState);
            toWalk.hasExitTime = false;
            toWalk.duration = 0.15f;
            toWalk.AddCondition(AnimatorConditionMode.If, 0, paramName);

            var toIdle = walkState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.15f;
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, paramName);
        }
        else
        {
            Debug.Log($"Ya existia {controllerPath}, se reutiliza tal cual.");
        }

        GameObject animTarget = FindInstantiatedChild(root, prefab) ?? root;

        // Limpia cualquier Animator que haya quedado mal puesto en una corrida anterior
        // (por ejemplo en un hueso de la malla vieja de Ch09, o en el padre).
        foreach (var stray in root.GetComponentsInChildren<Animator>(true))
        {
            if (stray.gameObject != animTarget)
                Object.DestroyImmediate(stray);
        }

        Animator animator = animTarget.GetComponent<Animator>();
        if (animator == null) animator = animTarget.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        if (avatar != null) animator.avatar = avatar;
        animator.applyRootMotion = false;

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(animTarget);
    }

    // El punto rojo (gizmo de deteccion que usa el Enemy para la linea de vision) estaba
    // calibrado para la altura de Ch09_nonPBR. Lo recalculamos segun la altura real del
    // modelo Adventurer women que quedo puesto ahora.
    static void AlignDetectionPoint(PlayerStateMachine player)
    {
        var renderer = player.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (renderer == null)
        {
            Debug.LogWarning("No se encontro un SkinnedMeshRenderer en el Player para recalibrar el punto de deteccion.");
            return;
        }

        float feetY = player.transform.position.y;
        float topY = renderer.bounds.max.y;
        float height = topY - feetY;

        if (height <= 0.01f)
        {
            Debug.LogWarning($"Altura calculada invalida ({height:F3}) para el Player, no se ajusta el punto de deteccion.");
            return;
        }

        float standing = height * 0.85f;
        float crouching = height * 0.35f;

        var so = new SerializedObject(player);
        so.FindProperty("standingDetectionHeight").floatValue = standing;
        so.FindProperty("crouchingDetectionHeight").floatValue = crouching;
        so.ApplyModifiedProperties();

        Debug.Log($"Punto de deteccion recalibrado: standing={standing:F2}, crouching={crouching:F2} (altura del modelo: {height:F2}).");
    }

    // Busca, entre los hijos directos de root, el que fue instanciado especificamente
    // a partir de "prefab" (comparando contra el asset de origen, no por nombre) -
    // asi evitamos agarrar por error una malla vieja que haya quedado de otro modelo.
    static GameObject FindInstantiatedChild(GameObject root, GameObject prefab)
    {
        if (prefab == null) return null;

        foreach (Transform child in root.transform)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (source == prefab)
                return child.gameObject;
        }
        return null;
    }
}
