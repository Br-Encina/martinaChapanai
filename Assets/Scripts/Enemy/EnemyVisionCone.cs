using UnityEngine;

// Poné este script en un GameObject vacío HIJO del enemigo.
// Genera un mesh real del cono de visión, visible en el juego final (no solo en el Editor).
// Requiere un Material transparente asignado en el Inspector — ver instrucciones abajo.
//
// CÓMO CREAR EL MATERIAL en Unity (URP):
//   1. Click derecho en Project → Create → Material
//   2. En el Inspector del material, cambiá el Shader a:
//      "Universal Render Pipeline/Unlit"
//   3. En "Surface Type" elegí "Transparent"
//   4. Poné el color que quieras (ej: rojo con Alpha ~100)
//   5. Arrastrá ese material al campo "Cone Material" de este script

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class EnemyVisionCone : MonoBehaviour
{
    [Header("Material")]
    [Tooltip("Material transparente URP/Unlit. Leé las instrucciones en el script para crearlo.")]
    [SerializeField] Material coneMaterial;

    [Header("Colores")]
    [SerializeField] Color normalColor    = new Color(1f, 0f, 0f, 0.35f);
    [SerializeField] Color detectedColor  = new Color(1f, 0.5f, 0f, 0.6f);

    [Header("Forma")]
    [SerializeField] int segments = 32; // Más segmentos = arco más suave
    [Tooltip("Offset vertical para que el cono no se meta bajo el piso.")]
    [SerializeField] float yOffset = 0.05f;

    MeshFilter   meshFilter;
    MeshRenderer meshRenderer;
    EnemyController enemy;

    // Valores previos para detectar cambios en Inspector durante el juego
    float lastRange;
    float lastAngle;

    private void Awake()
    {
        meshFilter   = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Buscar el EnemyController en el padre
        enemy = GetComponentInParent<EnemyController>();
        if (enemy == null)
            Debug.LogWarning("[EnemyVisionCone] No se encontró EnemyController en el padre.");

        // Asignar material
        if (coneMaterial != null)
        {
            // Instanciamos el material para poder cambiar el color sin afectar el asset original
            meshRenderer.material = new Material(coneMaterial);
        }
        else
        {
            Debug.LogWarning("[EnemyVisionCone] No hay material asignado. El cono no se verá.");
        }

        BuildMesh();
    }

    private void Update()
    {
        if (enemy == null) return;

        // Cambiar color según si el enemigo detectó al jugador o no
        if (meshRenderer.material != null)
        {
            meshRenderer.material.color = enemy.IsDetected ? detectedColor : normalColor;
        }

        // Reconstruir el mesh si los valores cambiaron en el Inspector (útil durante desarrollo)
        if (!Mathf.Approximately(enemy.DetectionRange, lastRange) ||
            !Mathf.Approximately(enemy.DetectionAngle, lastAngle))
        {
            BuildMesh();
        }
    }

    void BuildMesh()
    {
        if (enemy == null) return;

        float range    = enemy.DetectionRange;
        float angle    = enemy.DetectionAngle;
        float halfAngle = angle / 2f;

        lastRange = range;
        lastAngle = angle;

        // ── Construir vértices ─────────────────────────────────────────────
        // 1 vértice en el centro + (segments + 1) en el arco
        int vertexCount = segments + 2;
        Vector3[] vertices  = new Vector3[vertexCount];
        Vector2[] uvs       = new Vector2[vertexCount];
        int[]     triangles = new int[segments * 3];

        // Centro del cono (origen local del hijo, que ya está a la altura correcta via yOffset)
        vertices[0] = new Vector3(0f, yOffset, 0f);
        uvs[0]      = new Vector2(0.5f, 0.5f);

        // Vértices del arco
        for (int i = 0; i <= segments; i++)
        {
            float t   = -halfAngle + (angle / segments) * i;
            float rad = t * Mathf.Deg2Rad;

            vertices[i + 1] = new Vector3(
                Mathf.Sin(rad) * range,
                yOffset,
                Mathf.Cos(rad) * range
            );

            // UV simple: mapear el arco en un círculo unitario
            uvs[i + 1] = new Vector2(
                0.5f + Mathf.Sin(rad) * 0.5f,
                0.5f + Mathf.Cos(rad) * 0.5f
            );
        }

        // ── Construir triángulos ───────────────────────────────────────────
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3]     = 0;       // Centro
            triangles[i * 3 + 1] = i + 1;   // Vértice actual del arco
            triangles[i * 3 + 2] = i + 2;   // Siguiente vértice del arco
        }

        // ── Asignar al MeshFilter ──────────────────────────────────────────
        Mesh mesh   = new Mesh();
        mesh.name   = "VisionConeMesh";
        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.uv        = uvs;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
