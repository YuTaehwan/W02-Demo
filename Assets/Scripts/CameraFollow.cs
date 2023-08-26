using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // ���� ��� ������Ʈ (Player)�� Transform
    public float smoothSpeed = 0.125f; // ī�޶� �̵� ������ ����
    public Vector3 offset = new Vector3(0, -1, 0); // ���� ī�޶� ������ �Ÿ� ���� (�⺻������ �Ʒ��� 1��ŭ �̵�)

    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset; // ����� ��ġ�� �Ÿ� ���͸� ���Ͽ� ī�޶��� ���ϴ� ��ġ ���
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // ���� ��ġ���� ���ϴ� ��ġ���� �ε巯�� ����

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z); // ī�޶� ��ġ ������Ʈ
    }
}
