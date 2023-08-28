using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using W02;

public class AimLineRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxLineLength;
    public Chain rope;
    Vector2 aimPosition;
    LayerMask mask;
    InputUser user;
    Vector2 aimVec;
    bool isGamepad = false;
    public GameObject aimCircle;


    private void Awake()
    {
        PlayerInput input = GameObject.Find("GameManagers").GetComponent<PlayerInput>();
        mask = LayerMask.GetMask("Ground", "NotPass");
        aimCircle.SetActive(false);
    }

    
    private void Update()
    {
        
        if (!isGamepad)
        {
            aimPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            aimVec = (aimPosition - (Vector2)transform.position).normalized;
            // ���콺 ��ġ ����� �α�
            //Debug.Log("Mouse Position: " + aimPosition);
        }
        else
        {
            // �����е� ���� ��ġ ���            
            aimVec = (new Vector2(InputManager.Instance.AimHorizontal, InputManager.Instance.AimVertical)).normalized;
           
        }
        // �浹 �˻�
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, aimVec, Mathf.Infinity, mask);
        if (hit.collider != null)
        {
            float length = hit.distance;
            // ���ؼ� �׸��� �� ������Ʈ
            if (length > maxLineLength)
            {
                length = maxLineLength;
                DrawAimLine(aimVec, 0);
                aimCircle.SetActive(false);
                rope.canCreateChain = false;
            }
            else
            {
                rope.targetPosition = hit.point;
                DrawAimLine(aimVec, length);
                aimCircle.SetActive(true);
                rope.canCreateChain = true;
            }
        }
        else
        {
            DrawAimLine(aimVec, 0);
            aimCircle.SetActive(false);
            rope.canCreateChain = false;
            // ���ؼ� �׸��� �� ������Ʈ
            //    float length = hit.distance;
            //    if (length > maxLineLength)
            //    {
            //        length = maxLineLength;
            //    }
            //    DrawAimLine(aimVec, length);
            //
        }
    }

    private void OnEnable()
    {
        InputUser.onChange += OnInputDeviceChange;
    }

    private void OnDisable()
    {
        InputUser.onChange -= OnInputDeviceChange;

    }

    void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged)
        {
           UpdateDeviceType(user);
        }
    }

    void UpdateDeviceType(InputUser user)
    {
        string device = user.controlScheme.Value.name;
        if(device == "Gamepad")
        {
            isGamepad = true;
        }
        else
        {
            isGamepad = false;
        }
    }

    private void DrawAimLine(Vector2 _aimPos, float _length)
    {
        // ���ؼ� �� ��ġ ���
        Vector3 endPos = this.transform.position + (Vector3)aimVec * _length;
        aimCircle.transform.position = endPos;

        // ���ؼ� �׸���
        DrawLine(this.transform.position, endPos);
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos)
    {
        startPos.z = -1;
        endPos.z = -1;

        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0.3f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }
}
