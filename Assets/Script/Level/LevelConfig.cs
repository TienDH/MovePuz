using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [System.Serializable]
    public class BlockData
    {
        public string addressableKey;
        public Vector2Int position;
        public float rotation = 0f;
    }

    public Vector2Int matrixSize = new Vector2Int(3, 3);
    public BlockData[] Blocks;
    public Vector2Int starPosition; // Vị trí ngôi sao, sẽ được đặt tại block cuối
}