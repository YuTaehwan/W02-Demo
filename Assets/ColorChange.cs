using UnityEngine;

public class ColorChange : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float colorChangeInterval = 0.05f; // �� ��ȭ ���� (�� ����)
    private float timeSinceLastChange = 0.0f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timeSinceLastChange += Time.deltaTime;

        if (timeSinceLastChange >= colorChangeInterval)
        {
            timeSinceLastChange = 0.0f;

            // ������ �� ����
            Color newColor = new Color(Random.value, Random.value, Random.value);

            // SpriteRenderer �� ����
            spriteRenderer.color = newColor;
        }
    }
}
