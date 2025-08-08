using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PipeBlock : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private bool up;
    [SerializeField] private bool down;
    [SerializeField] private bool left;
    [SerializeField] private bool right;

    [Header("Behavior Settings")]
    [SerializeField] private bool canRotate = true;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canElevate;

    [Header("References")]
    [SerializeField] private MatrixGame matrixGame;

    [Header("Elevator Settings")]
    [SerializeField] private float waitTimeBeforeLowering = 0.5f;

    private static readonly WaitForSeconds waitCache = new WaitForSeconds(0.5f);
    private bool isElevating;
    private Player currentPlayer;
    private Vector2Int originalPosition;
    private Vector3 cachedPosition;
    #region Initialization
    private void Awake()
    {
        CacheOriginalPosition();
    }

    private void CacheOriginalPosition()
    {
        if (matrixGame != null)
        {
            originalPosition = matrixGame.WorldToMatrixPos(transform.position);
            cachedPosition = transform.position;
        }
    }
    #endregion

    #region Debug Visualization
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (matrixGame == null) return;

        Vector3 position = transform.position;
        float halfCellSize = matrixGame.cellSize * 0.5f;

        DrawConnectionGizmos(position, halfCellSize);
        DrawBehaviorIndicatorGizmos(position, halfCellSize);
    }

    private void DrawConnectionGizmos(Vector3 position, float halfCellSize)
    {
        Gizmos.color = Color.green;
        if (down) Gizmos.DrawLine(position, position + new Vector3(0, halfCellSize, 0));
        if (up) Gizmos.DrawLine(position, position + new Vector3(0, -halfCellSize, 0));
        if (left) Gizmos.DrawLine(position, position + new Vector3(-halfCellSize, 0, 0));
        if (right) Gizmos.DrawLine(position, position + new Vector3(halfCellSize, 0, 0));

        Gizmos.color = Color.red;
        if (!down) Gizmos.DrawLine(position, position + new Vector3(0, halfCellSize * 0.5f, 0));
        if (!up) Gizmos.DrawLine(position, position + new Vector3(0, -halfCellSize * 0.5f, 0));
        if (!left) Gizmos.DrawLine(position, position + new Vector3(-halfCellSize * 0.5f, 0));
        if (!right) Gizmos.DrawLine(position, position + new Vector3(halfCellSize * 0.5f, 0));
    }

    private void DrawBehaviorIndicatorGizmos(Vector3 position, float halfCellSize)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(position, new Vector3(matrixGame.cellSize * 0.8f, matrixGame.cellSize * 0.8f, 0));

        Gizmos.color = canRotate ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(position, matrixGame.cellSize * 0.2f);

        Gizmos.color = canMove ? Color.blue : Color.gray;
        Gizmos.DrawWireSphere(position, matrixGame.cellSize * 0.15f);

        Gizmos.color = canElevate ? Color.magenta : Color.gray;
        Gizmos.DrawWireSphere(position, matrixGame.cellSize * 0.1f);
    }
#endif
    #endregion

    #region Connection Management
    public bool CanConnect(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return down;
        if (direction == Vector2Int.down) return up;
        if (direction == Vector2Int.left) return left;
        if (direction == Vector2Int.right) return right;
        return false;
    }
    #endregion

    #region Rotation
    public void RotateClockwise()
    {
        if (!canRotate || matrixGame == null || matrixGame.isMoving || IsPlayerOnBlock()) return;
        RotateConnections();
        transform.Rotate(0, 0, -90);
        AudioManager.Instance.PlayVFX(AudioManager.Instance.jump);
    }

    public bool IsPlayerOnBlock()
    {
        if (matrixGame == null || matrixGame.GetPlayerPosition() == null) return false;
        Vector2Int currentBlockPos = matrixGame.WorldToMatrixPos(transform.position);
        Vector2Int playerPos = matrixGame.GetPlayerPosition();
        return currentBlockPos == playerPos;
    }

    private void RotateConnections()
    {
        (up, left, down, right) = (left, down, right, up);
    }
    #endregion

    #region Behavior Properties
    public bool CanRotate() => canRotate;
    public bool CanMove() => canMove;
    public bool CanElevate() => canElevate;
    #endregion

    #region Matrix Reference
    public void SetMatrixGame(MatrixGame mg)
    {
        matrixGame = mg;
        CacheOriginalPosition();
    }
    #endregion

    #region Elevator Behavior
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canElevate || isElevating) return;

        if (other.TryGetComponent<Player>(out var player) && (currentPlayer == null || player == currentPlayer))
        {
            currentPlayer = player;
            StartCoroutine(WaitAndMoveElevatorDown());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!canElevate || isElevating) return;

        if (other.TryGetComponent<Player>(out var player) && player == currentPlayer)
        {
            StartCoroutine(MoveElevatorUp());
            currentPlayer = null;
        }
    }

    private IEnumerator WaitAndMoveElevatorDown()
    {
        yield return waitCache;

        Vector2Int currentPos = matrixGame.WorldToMatrixPos(cachedPosition);
        if (ShouldSkipElevatorMovement(currentPos)) yield break;

        yield return PerformElevatorMovement(currentPos, Vector2Int.up, true);
    }

    private IEnumerator MoveElevatorUp()
    {
        isElevating = true;
        matrixGame.SetMoving(true);

        Vector2Int currentPos = matrixGame.WorldToMatrixPos(cachedPosition);
        yield return PerformElevatorMovement(currentPos, Vector2Int.down, false);

        matrixGame.SetMoving(false);
        isElevating = false;
    }

    private bool ShouldSkipElevatorMovement(Vector2Int currentPos)
    {
        return currentPlayer == null ||
               isElevating ||
               matrixGame.WorldToMatrixPos(currentPlayer.transform.position) != currentPos;
    }

    private IEnumerator PerformElevatorMovement(Vector2Int startPos, Vector2Int direction, bool includePlayer)
    {
        isElevating = true;
        matrixGame.SetMoving(true);
        if (includePlayer) currentPlayer?.LockMovement(true);

        Vector2Int targetPos = CalculateTargetPosition(startPos, direction);
        if (targetPos != startPos)
        {
            yield return ExecuteBlockMovement(startPos, targetPos, includePlayer);
        }

        if (includePlayer)
        {
            currentPlayer?.LockMovement(false);
            currentPlayer?.SetUnselected();
        }
        isElevating = false;
        matrixGame.SetMoving(false);
    }

    private Vector2Int CalculateTargetPosition(Vector2Int startPos, Vector2Int direction)
    {
        Vector2Int targetPos = startPos;
        while (matrixGame.IsValidPosition(targetPos + direction) &&
               matrixGame.GetBlockAt(targetPos + direction) == null)
        {
            targetPos += direction;
        }
        return targetPos;
    }

    private IEnumerator ExecuteBlockMovement(Vector2Int fromPos, Vector2Int toPos, bool movePlayer)
    {
        matrixGame.UpdateMatrix(fromPos, null);
        matrixGame.UpdateMatrix(toPos, gameObject);

        yield return MoveBlockSmoothly(toPos);

        if (movePlayer)
        {
            currentPlayer?.MoveTo(toPos);
        }
    }

    private IEnumerator MoveBlockSmoothly(Vector2Int targetPos)
    {
        Vector3 targetWorldPos = matrixGame.GetWorldPosition(targetPos.x, targetPos.y);
        Vector3 startPos = cachedPosition;
        float distance = Vector3.Distance(startPos, targetWorldPos);
        float duration = distance / matrixGame.GetMoveSpeed();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            cachedPosition = Vector3.Lerp(startPos, targetWorldPos, t);
            transform.position = cachedPosition;

            if (currentPlayer != null)
            {
                currentPlayer.transform.position = cachedPosition;
            }

            yield return null;
        }

        cachedPosition = targetWorldPos;
        transform.position = targetWorldPos;
    }
    #endregion
}