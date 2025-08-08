using UnityEngine;

public class MoveBackAndForth : MonoBehaviour
{
    public Vector3 pointA; // Điểm bắt đầu
    public Vector3 pointB; // Điểm kết thúc
    public float speed = 2f;

    private Vector3 target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        // Di chuyển tới điểm target
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Khi tới target, đổi hướng
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            if (target == pointB)
                target = pointA;
            else
                target = pointB;
        }
    }
}
