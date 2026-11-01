using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal")]
    [SerializeField] private Portal other;
    [SerializeField] private Camera portalView;

    [Header("Camera Position")]
    [SerializeField] private float cameraDistance = 5f;

    [SerializeField] private bool followHorizontal = true;
    [SerializeField] private bool followVertical = true;

    [Header("Camera Settings")]
    [SerializeField] private float fieldOfView = 60f;
    [SerializeField] private float nearClipPlane = 0.03f;
    [SerializeField] private float farClipPlane = 500f;

    [Header("Render Texture")]
    [SerializeField] private int depthBuffer = 24;

    private RenderTexture renderTexture;

    private void Start()
    {
        if (other == null || portalView == null)
        {
            Debug.LogError($"Portal '{name}': не назначен Other или PortalView.");
            return;
        }

        CreateRenderTexture();
        SetupCamera();
        SetupVisual();
    }

    private void LateUpdate()
    {
        if (other == null || portalView == null || Camera.main == null)
            return;

        UpdatePortalCamera();
    }

    private void CreateRenderTexture()
    {
        if (renderTexture != null)
            renderTexture.Release();

        renderTexture = new RenderTexture(
            Screen.width,
            Screen.height,
            depthBuffer
        );

        renderTexture.name = $"{name}_PortalTexture";
        renderTexture.Create();

        portalView.targetTexture = renderTexture;
    }

    private void SetupCamera()
    {
        portalView.fieldOfView = fieldOfView;
        portalView.nearClipPlane = nearClipPlane;
        portalView.farClipPlane = farClipPlane;
    }

    private void SetupVisual()
    {
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();

        if (renderer == null)
        {
            Debug.LogWarning($"Portal '{name}': MeshRenderer не найден.");
            return;
        }

        renderer.material.mainTexture = renderTexture;
    }

    private void UpdatePortalCamera()
    {
        Transform mainCamera = Camera.main.transform;

        // Положение основной камеры относительно ВЫХОДНОГО портала
        Vector3 localPosition =
            other.transform.InverseTransformPoint(mainCamera.position);

        // Переворачиваем пространство через портал
        localPosition = new Vector3(
            -localPosition.x,
            localPosition.y,
            -localPosition.z
        );

        // ------------------------------------------------
        // Главное отличие от старого скрипта:
        // Z НЕ зависит от расстояния игрока.
        // ------------------------------------------------

        localPosition.z = cameraDistance;

        if (!followHorizontal)
            localPosition.x = 0f;

        if (!followVertical)
            localPosition.y = 0f;

        // Переводим положение обратно в мировые координаты
        portalView.transform.position =
            other.transform.TransformPoint(localPosition);

        // Поворот камеры через портал
        Quaternion difference =
            transform.rotation *
            Quaternion.Inverse(
                other.transform.rotation *
                Quaternion.Euler(0f, 180f, 0f)
            );

        portalView.transform.rotation =
            difference * mainCamera.rotation;
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            if (portalView != null)
                portalView.targetTexture = null;

            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}






//using UnityEngine;

//public class Portal : MonoBehaviour
//{
//    public Portal Other;
//    public Camera PortalView;

//    private void Start()
//    {
//        Other.PortalView.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
//        GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture = Other.PortalView.targetTexture;
//    }

//    private void Update()
//    {
//        // Position
//        Vector3 lookerPosition = Other.transform.worldToLocalMatrix.MultiplyPoint3x4(Camera.main.transform.position);
//        lookerPosition = new Vector3(-lookerPosition.x, lookerPosition.y, -lookerPosition.z);
//        PortalView.transform.localPosition = lookerPosition;

//        // Rotation
//        Quaternion difference = transform.rotation * Quaternion.Inverse(Other.transform.rotation * Quaternion.Euler(0, 180, 0));
//        PortalView.transform.rotation = difference * Camera.main.transform.rotation;

//        // Clipping
//        PortalView.nearClipPlane = lookerPosition.magnitude;
//    }
//}


