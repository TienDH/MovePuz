using TMPro;
using UnityEngine;

public class turtorial2 : MonoBehaviour
{
    [SerializeField] private GameObject hand1;
    [SerializeField] private MatrixGame Game;

    private Vector3 originalPos;
    private Vector3 target1 = new Vector3(-71.72f, -314.09f, 12.32f);
    private Vector3 target2 = new Vector3(-72.78f, -314.09f, 12.32f);

    private float moveSpeed = 1.0f;
    private Vector3 currentTarget;

    void Start()
    {
        Game = FindFirstObjectByType<MatrixGame>();
        originalPos = hand1.transform.localPosition;
        currentTarget = target1;
    }

    void Update()
    {
        if (Game == null || hand1 == null) return;

        if (Game.isLv2 == 2)
        {
            hand1.SetActive(false);
            return;
        }

        hand1.SetActive(true);

        // Di chuyển hand1
        hand1.transform.localPosition = Vector3.MoveTowards(hand1.transform.localPosition, currentTarget, moveSpeed * Time.deltaTime);

        // Kiểm tra nếu đã đến target, thì đổi target mới
        if (Vector3.Distance(hand1.transform.localPosition, currentTarget) < 0.01f)
        {
            if (Game.isLv2 == 1)
            {
                currentTarget = currentTarget == target1 ? target2 : target1;
            }
            else
            {
                currentTarget = currentTarget == target1 ? originalPos : target1;
            }
        }
    }
}

