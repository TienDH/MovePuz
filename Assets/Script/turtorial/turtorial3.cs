using TMPro;
using UnityEngine;

public class turtorial3 : MonoBehaviour
{
    [SerializeField] private GameObject hand1;
    [SerializeField] private MatrixGame Game;

    void Update()
    {
        if (Game == null || hand1 == null) return;

        if (Game.isLv2 > 0)
        {
            hand1.SetActive(false);

        }

    }
}

