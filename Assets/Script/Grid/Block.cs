using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private int blockId;

    public int BlockId => blockId;

    void Start()
    {
        var text = GetComponentInChildren<TextMesh>();
        if (text != null)
        {
            text.text = blockId.ToString();
        }
    }
}