using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class SimpleFpsLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;
    public Transform[] weapons;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 200f;
    public bool useDeltaTime = true;

    [Header("Pitch clamp")]
    public float minPitch = -89f;
    public float maxPitch = 89f;

    [Header("Cursor")]
    public bool lockCursor = true;

    [Header("Sensitivity UI")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider[] extraSensitivitySliders; 

    private bool canLook = true;

    private const string SensitivityKey = "MouseSensitivity";

    float pitch = 0f;
    float yaw = 0f;

    bool[] weaponIsChild;
    Vector3[] weaponLocalPos;
    Quaternion[] weaponLocalRot;

    void Start()
    {
        if (playerBody == null && transform.parent != null)
            playerBody = transform.parent;

        yaw = playerBody != null ? playerBody.eulerAngles.y : transform.eulerAngles.y;
        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        mouseSensitivity = PlayerPrefs.GetFloat(SensitivityKey, mouseSensitivity);

        SetupSensitivitySlider(sensitivitySlider);
        if (extraSensitivitySliders != null)
        {
            foreach (var s in extraSensitivitySliders)
                SetupSensitivitySlider(s);
        }

        if (lockCursor)
            SetCursorLocked(true);

        if (weapons != null && weapons.Length > 0)
        {
            int n = weapons.Length;
            weaponIsChild = new bool[n];
            weaponLocalPos = new Vector3[n];
            weaponLocalRot = new Quaternion[n];

            for (int i = 0; i < n; i++)
            {
                var w = weapons[i];
                if (w == null) continue;

                weaponIsChild[i] = w.IsChildOf(transform);
                if (!weaponIsChild[i])
                {
                    weaponLocalPos[i] = transform.InverseTransformPoint(w.position);
                    weaponLocalRot[i] = Quaternion.Inverse(transform.rotation) * w.rotation;
                }
            }
        }
        //CacheWeapons();
    }

    void Update()
    {
        if (!canLook)
            return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        float dt = useDeltaTime ? Time.deltaTime : 1f;

        yaw += mx * mouseSensitivity * dt;
        pitch -= my * mouseSensitivity * dt;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (playerBody != null)
            playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (weapons != null)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                var w = weapons[i];
                if (w == null) continue;

                if (weaponIsChild != null && i < weaponIsChild.Length && weaponIsChild[i])
                    continue;

                if (weaponLocalPos != null && i < weaponLocalPos.Length)
                {
                    w.position = transform.TransformPoint(weaponLocalPos[i]);
                    w.rotation = transform.rotation * weaponLocalRot[i];
                }
            }
        }
    }
    //private void CacheWeapons()
    //{
    //    if (weapons == null || weapons.Length == 0)
    //        return;

    //    int n = weapons.Length;

    //    weaponIsChild = new bool[n];
    //    weaponLocalPos = new Vector3[n];
    //    weaponLocalRot = new Quaternion[n];

    //    for (int i = 0; i < n; i++)
    //    {
    //        var w = weapons[i];
    //        if (w == null)
    //            continue;

    //        weaponIsChild[i] = w.IsChildOf(transform);

    //        if (!weaponIsChild[i])
    //        {
    //            weaponLocalPos[i] = transform.InverseTransformPoint(w.position);
    //            weaponLocalRot[i] = Quaternion.Inverse(transform.rotation) * w.rotation;
    //        }
    //    }
    //}
    //public void SetWeapon(Transform weapon)
    //{
    //    if (weapon == null)
    //    {
    //        weapons = null;
    //        return;
    //    }

    //    weapons = new Transform[1];
    //    weapons[0] = weapon;


    //    CacheWeapons();
    //}

    private void SetupSensitivitySlider(Slider slider)
    {
        if (slider == null) return;

        slider.minValue = 10f;
        slider.maxValue = 300f;
        slider.value = mouseSensitivity;
        slider.onValueChanged.AddListener(SetSensitivity);
    }

    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
        PlayerPrefs.SetFloat(SensitivityKey, value);
        PlayerPrefs.Save();
    }

    public void SetLookEnabled(bool enabled)
    {
        canLook = enabled;
    }

    public void SetCursorLocked(bool locked)
    {
        lockCursor = locked;
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}