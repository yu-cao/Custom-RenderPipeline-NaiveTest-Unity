using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCamera : MonoBehaviour
{
#if UNITY_EDITOR
    private static Texture2D ms_invisibleCursor = null;
#endif

    public bool enableInputCapture = true;//是否对鼠标事件进行捕获
    public bool holdRightMouseCapture = false;//是否使用右键进行控制

    public float lookSpeed = 5f;//旋转Camera的灵敏度
    public float moveSpeed = 5f;//移动Camera的灵敏度
    public float sprintSpeed = 50f;//按住左Shift情况下的Camera快速移动速度

    private bool m_inputCapture;
    private float m_yaw;
    private float m_pitch;

    void Awake()
    {
        enabled = enableInputCapture;
    }

    void OnValidate()
    {
        if (Application.isPlaying)
            enabled = enableInputCapture;
    }

    void CaptureInput()
    {
        Cursor.lockState = CursorLockMode.Locked;

#if UNITY_EDITOR
        Cursor.SetCursor(ms_invisibleCursor, Vector2.zero, CursorMode.ForceSoftware);
#else
        Cursor.visible = false;
#endif
        m_inputCapture = true;

        m_yaw = transform.eulerAngles.y;
        m_pitch = transform.eulerAngles.x;
    }

    void ReleaseInput()
    {
        Cursor.lockState = CursorLockMode.None;
#if UNITY_EDITOR
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
#else
		Cursor.visible = true;
#endif
        m_inputCapture = false;
    }

    void OnApplicationFocus(bool focus)
    {
        if (m_inputCapture && !focus)
            ReleaseInput();
    }

    void Update()
    {
        //没有输入
        if (!m_inputCapture)
        {
            //允许左键控制且按下了左键
            if (!holdRightMouseCapture && Input.GetMouseButtonDown(0))
                CaptureInput();
            //允许右键控制且按下了右键
            else if (holdRightMouseCapture && Input.GetMouseButtonDown(1))
                CaptureInput();
        }
        if (!m_inputCapture)
            return;

        //读入到了鼠标进入了game中
        if (m_inputCapture)
        {
            if (!holdRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
                ReleaseInput();
            else if (holdRightMouseCapture && Input.GetMouseButtonUp(1))
                ReleaseInput();
        }

        var rotStrafe = Input.GetAxis("Mouse X");
        var rotFwd = Input.GetAxis("Mouse Y");

        m_yaw = (m_yaw + lookSpeed * rotStrafe) % 360f;
        m_pitch = (m_pitch - lookSpeed * rotFwd) % 360f;
        transform.rotation = Quaternion.AngleAxis(m_yaw, Vector3.up) * Quaternion.AngleAxis(m_pitch, Vector3.right);

        var speed = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
        var forward = speed * Input.GetAxis("Vertical");
        var right = speed * Input.GetAxis("Horizontal");
        var up = speed * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));
        transform.position += transform.forward * forward + transform.right * right + Vector3.up * up;
    }
}
