using UnityEngine;

public class BounceByDirection : MonoBehaviour
{
    public float moveDistance = 0.5f; // Khoảng cách nhún nhảy
    public float speed = 2f;          // Tốc độ nhún nhảy

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Tạo dao động sin
        float offset = Mathf.Sin(Time.time * speed) * moveDistance;

        // Hướng theo chiều quay hiện tại
        Vector3 direction = transform.up; // Nếu muốn theo hướng forward thì dùng transform.forward

        // Di chuyển theo hướng đó
        transform.position = startPos + direction * offset;
    }
}
