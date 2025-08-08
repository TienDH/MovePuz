using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector2Int currentPosition;
    private bool isSelected = false, isMovementLocked = false;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color32 selectedColor = new Color32(215, 29, 67, 255);
    [SerializeField] private Color defaultColor = Color.white;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (spriteRenderer != null) spriteRenderer.color = defaultColor;
    }

    public void SetInitialPosition(Vector2Int pos)
    {
        currentPosition = pos;
        MatrixGame mg = FindFirstObjectByType<MatrixGame>();
        if (mg != null) transform.position = mg.GetWorldPosition(pos.x, pos.y);
    }

    public void SetCurrentPosition(Vector2Int pos)
    {
        currentPosition = pos;
        MatrixGame mg = FindFirstObjectByType<MatrixGame>();
        if (mg != null) transform.position = mg.GetWorldPosition(pos.x, pos.y);
    }

    public Vector2Int GetCurrentPosition() => currentPosition;

    public void OnMouseDowns(bool playSound = true)
    {
        if (playSound)
            AudioManager.Instance.PlayVFX(AudioManager.Instance.clip);

        if (isMovementLocked) return;

        isSelected = true;
        if (spriteRenderer != null) spriteRenderer.color = selectedColor;
    }

    public void SetUnselected()
    {
        isSelected = false;
        if (spriteRenderer != null) spriteRenderer.color = defaultColor;
    }

    public bool IsSelected() => isSelected && !isMovementLocked;

    public void MoveTo(Vector2Int pos, System.Action onComplete = null)
    {
        SetCurrentPosition(pos);
        onComplete?.Invoke();
    }

    public void LockMovement(bool lockState)
    {
        isMovementLocked = lockState;
        if (lockState) SetUnselected();
    }
}