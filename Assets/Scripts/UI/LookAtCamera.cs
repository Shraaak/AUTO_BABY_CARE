using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [Tooltip("指定要朝向的相机；为空则使用主相机(MainCamera)")]
    public Transform targetCamera;

    [Tooltip("只绕世界Y轴旋转")]
    public bool onlyYaw = true;

    [Tooltip("反向看向相机（有些UI会正反面颠倒时用）")]
    public bool invertForward = false;

    void LateUpdate()
    {
        var cam = targetCamera != null ? targetCamera : UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null;
        if (cam == null) return;

        Vector3 toCam = cam.position - transform.position;
        if (onlyYaw) toCam.y = 0f;
        if (toCam.sqrMagnitude < 0.000001f) return;

        Quaternion rot = Quaternion.LookRotation(invertForward ? -toCam : toCam, Vector3.up);
        transform.rotation = rot;
    }
}
