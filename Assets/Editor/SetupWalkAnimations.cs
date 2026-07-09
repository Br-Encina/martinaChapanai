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

        GameObject animTarget = FindVisualRoot(root);

        // Limpia un Animator que haya quedado mal puesto en el padre en una corrida anterior.
        if (animTarget != root)
        {
            Animator stray = root.GetComponent<Animator>();
            if (stray != null) Object.DestroyImmediate(stray);
        }

        Animator animator = animTarget.GetComponent<Animator>();
        if (animator == null) animator = animTarget.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        if (avatar != null) animator.avatar = avatar;
        animator.applyRootMotion = false;

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(animTarget);
    }

    // El Animator tiene que vivir en el mismo GameObject que la raiz del esqueleto animado
    // (el modelo instanciado como hijo), no en el objeto padre que trae el CharacterController/scripts.
    static GameObject FindVisualRoot(GameObject root)
    {
        var skinned = root.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (skinned == null) return root;

        Transform t = skinned.transform;
        while (t.parent != null && t.parent != root.transform)
            t = t.parent;

        return t.gameObject;
    }
}
