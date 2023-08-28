using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float smoothing = 0.2f;
    [SerializeField] Vector2 minCameraBoundary;
    [SerializeField] Vector2 maxCameraBoundary;
    public Vector3 offset; // ���� ī�޶� ������ �Ÿ� ����

    Vector3 velocity;

    private void LateUpdate()
    {
        //Vector3 targetPos = new Vector3(player.position.x, player.position.y, this.transform.position.z);

        /*targetPos.x = Mathf.Clamp(targetPos.x, minCameraBoundary.x, maxCameraBoundary.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minCameraBoundary.y, maxCameraBoundary.y);*//*

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothing);*/

        Vector3 desiredPosition = player.position + offset; // ����� ��ġ�� �Ÿ� ���͸� ���Ͽ� ī�޶��� ���ϴ� ��ġ ���
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothing); // ���� ��ġ���� ���ϴ� ��ġ���� �ε巯�� ����

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z); // ī�޶� ��ġ ������Ʈ
    }
}
