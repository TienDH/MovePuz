using UnityEngine;
using TMPro;

public class TurtorialManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI t1;
    [SerializeField] private TextMeshProUGUI t2;
    [SerializeField] private GameObject hand1;
    [SerializeField] private GameObject hand2;
    [SerializeField] private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<Player>();

        t1.gameObject.SetActive(true);
        t2.gameObject.SetActive(false);
        hand1.SetActive(true);
        hand2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(player.IsSelected() == true)
        {
            t2.gameObject.SetActive (true);
            t1.gameObject.SetActive (false);
            hand1.SetActive (false);
            hand2.SetActive (true);
        }
    }
}
