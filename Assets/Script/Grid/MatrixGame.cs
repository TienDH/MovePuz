using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets.ResourceLocators;

public class MatrixGame : MonoBehaviour
{
    [SerializeField] public float cellSize = 1.2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Player player;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private FadePanel fadePanel; // Thêm tham chiếu FadePanel

    private Vector3 gridOrigin;
    private GameObject[,] matrix;
    private Vector2Int matrixSize;
    public bool isMoving;
    private GameObject starInstance;
    private GameObject breakEffectInstance;
    private LevelManager levelManager;
    private GameManager gameManager;
    private List<GameObject> activeBlocks = new List<GameObject>();
    private LevelConfig currentLevelConfig;

    [Header("Input Settings")]
    [SerializeField] private float minDragDistance = 2f;
    private Vector2 inputStartPos;
    private bool isDragging;

    [Header("Double Click Settings")]
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;
    private Vector2 lastClickPos;
    [SerializeField] private GameObject grid1;
    [SerializeField] private GameObject grid2;
    public int isLv2 = 0;
    private bool isInitialized = false;
    public bool IsInitialized() => isInitialized;

    #region Initialization
    void Awake()
    {
        Addressables.InitializeAsync().Completed += OnAddressablesInitialized;
        levelManager = FindFirstObjectByType<LevelManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        if (levelManager == null) Debug.LogError("Không tìm thấy LevelManager trong scene!");
        if (gameManager == null) Debug.LogError("Không tìm thấy GameManager trong scene!");
        if (fadePanel == null) Debug.LogError("FadePanel chưa được gán trong MatrixGame!");
    }

    private void OnEnable()
    {
        grid1.SetActive(false);
        grid2.SetActive(false);

        // Đợi LevelManager gọi SetLevelConfig
    }

    private void OnAddressablesInitialized(AsyncOperationHandle<IResourceLocator> op)
    {
        if (op.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning("Khởi tạo Addressables thất bại");
        }
    }

    private void InitializeGameSettings()
    {
        if (currentLevelConfig == null)
        {
            Debug.LogWarning("currentLevelConfig là null. Sử dụng cài đặt mặc định.");
            matrixSize = new Vector2Int(3, 3);
        }
        else
        {
            matrixSize = currentLevelConfig.matrixSize; // Sửa lỗi: thay 'current' bằng 'currentLevelConfig'
        }
        matrixSize = ValidateMatrixSize(matrixSize);
        UpdateGridOrigin();
        Debug.Log($"Khởi tạo kích thước ma trận: {matrixSize}, gridOrigin: {gridOrigin}, cellSize: {cellSize}");
    }

    private void UpdateGridOrigin()
    {
        if (matrixSize.x == 3 && matrixSize.y == 3)
        {
            grid1.SetActive(true);
            grid2.SetActive(false);
            gridOrigin = new Vector3(-0.8f, 0.55f, 0f);
        }
        else if (matrixSize.x == 4 && matrixSize.y == 4)
        {
            grid1.SetActive(false);
            grid2.SetActive(true);
            gridOrigin = new Vector3(-1.4f, 1.0f, 0f);
        }
        else
        {
            gridOrigin = new Vector3(-0.8f, 0.55f, 0f);
            Debug.LogWarning($"Kích thước ma trận {matrixSize} không được hỗ trợ, sử dụng gridOrigin mặc định: {gridOrigin}");
        }
        Debug.Log($"Đặt gridOrigin: {gridOrigin} cho kích thước ma trận {matrixSize}");
    }

    private Vector2Int ValidateMatrixSize(Vector2Int size)
    {
        return new Vector2Int(
            Mathf.Max(1, size.x),
            Mathf.Max(1, size.y)
        );
    }
    #endregion

    #region Player and Block Initialization
    IEnumerator InitializeAndMovePlayer()
    {
        Debug.Log("Bắt đầu khởi tạo và di chuyển player...");
        yield return SpawnBlocksAsync();
        InitializePlayerPosition();
        SpawnStarAndEffect();
        if (player != null && starInstance != null)
        {
            Vector2Int playerPos = player.GetCurrentPosition();
            Vector2Int starPos = currentLevelConfig?.starPosition ?? Vector2Int.zero;
            if (playerPos == starPos)
            {
                Debug.LogWarning($"Player và Star ở cùng vị trí {playerPos}. Điều chỉnh vị trí Player.");
                player.SetInitialPosition(new Vector2Int(0, 0));
                player.transform.position = GetWorldPosition(0, 0);
            }
        }
        Debug.Log("Khởi tạo và di chuyển player hoàn tất.");
    }

    private void InitializePlayerPosition()
    {
        if (player == null)
        {
            Debug.LogError("Player chưa được gán trong MatrixGame");
            return;
        }
        Vector2Int initialPos = GetInitialPlayerPosition();
        player.SetInitialPosition(initialPos);
        player.transform.position = GetWorldPosition(initialPos.x, initialPos.y);
        player.SetUnselected();
        Debug.Log($"Player được khởi tạo tại {player.transform.position} từ vị trí ma trận {initialPos}");
    }

    private Vector2Int GetInitialPlayerPosition()
    {
        if (currentLevelConfig?.Blocks != null && currentLevelConfig.Blocks.Length > 0 && IsValidPosition(currentLevelConfig.Blocks[0].position))
        {
            Debug.Log($"Vị trí player ban đầu từ LevelConfig: {currentLevelConfig.Blocks[0].position}");
            return currentLevelConfig.Blocks[0].position;
        }
        Debug.LogWarning("Không tìm thấy vị trí player hợp lệ trong LevelConfig, sử dụng (0,0)");
        return Vector2Int.zero;
    }

    IEnumerator SpawnBlocksAsync()
    {
        Debug.Log("Bắt đầu tạo các khối...");
        if (currentLevelConfig == null || currentLevelConfig.Blocks == null)
        {
            Debug.LogWarning("currentLevelConfig hoặc Blocks là null");
            yield break;
        }

        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
        foreach (var blockData in currentLevelConfig.Blocks)
        {
            Vector2Int pos = blockData.position;
            if (!IsValidPosition(pos) || occupiedPositions.Contains(pos) || matrix[pos.x, pos.y] != null)
            {
                Debug.LogWarning($"Bỏ qua khối tại {pos} do vị trí không hợp lệ hoặc trùng");
                continue;
            }

            occupiedPositions.Add(pos);
            yield return TrySpawnBlock(blockData, pos);
        }

        Debug.Log($"Tạo {activeBlocks.Count} khối cho level");
        LogMatrixState();
    }

    private void LogMatrixState()
    {
        for (int x = 0; x < matrixSize.x; x++)
        {
            for (int y = 0; y < matrixSize.y; y++)
            {
                if (matrix[x, y] != null)
                    Debug.Log($"Ma trận[{x},{y}] được chiếm bởi {matrix[x, y].name} tại {GetWorldPosition(x, y)}");
            }
        }
    }

    private IEnumerator TrySpawnBlock(LevelConfig.BlockData blockData, Vector2Int pos)
    {
        Debug.Log($"Đang khởi tạo khối {blockData.addressableKey} tại {pos}");
        Quaternion rotation = Quaternion.Euler(0, 0, blockData.rotation);
        Vector3 worldPos = GetWorldPosition(pos.x, pos.y);

        AsyncOperationHandle<GameObject> instantiateHandle = Addressables.InstantiateAsync(
            blockData.addressableKey,
            worldPos,
            rotation
        );
        yield return instantiateHandle;

        if (instantiateHandle.Status != AsyncOperationStatus.Succeeded || instantiateHandle.Result == null)
        {
            Debug.LogError($"Khởi tạo thất bại: {blockData.addressableKey}, Status: {instantiateHandle.Status}");
            yield break;
        }

        GameObject block = instantiateHandle.Result;
        block.name = $"Block_{blockData.addressableKey}";
        matrix[pos.x, pos.y] = block;
        activeBlocks.Add(block);

        Block blockComponent = block.GetComponent<Block>();
        if (blockComponent == null)
        {
            Debug.LogWarning($"Prefab {block.name} thiếu component Block");
        }
        else
        {
            PipeBlock pipeBlock = block.GetComponent<PipeBlock>();
            if (pipeBlock != null)
            {
                pipeBlock.SetMatrixGame(this);
            }
        }

        // Đảm bảo WebGL có thời gian load xong frame
        yield return null;
    }


    private IEnumerator InstantiateBlock(GameObject prefab, LevelConfig.BlockData data, Vector2Int pos)
    {
        Block blockComponent = prefab.GetComponent<Block>();
        if (blockComponent == null)
        {
            Debug.LogWarning($"Khối prefab {prefab.name} thiếu component Block");
            yield break;
        }

        var rotation = Quaternion.Euler(0, 0, data.rotation);
        var position = GetWorldPosition(pos.x, pos.y);

        AsyncOperationHandle<GameObject> instantiateHandle = Addressables.InstantiateAsync(
            data.addressableKey,
            position,
            rotation
        );
        yield return instantiateHandle;

        if (!instantiateHandle.IsValid() || instantiateHandle.Result == null)
        {
            Debug.LogWarning($"Khởi tạo khối với key {data.addressableKey} thất bại");
            yield break;
        }

        SetupBlock(instantiateHandle.Result, blockComponent, pos);
        activeBlocks.Add(instantiateHandle.Result);
        Debug.Log($"Khởi tạo khối {data.addressableKey} tại {pos} với vị trí thế giới {position}", instantiateHandle.Result);
    }

    private void SetupBlock(GameObject block, Block blockComponent, Vector2Int pos)
    {
        block.name = $"Block_{blockComponent.BlockId}";
        matrix[pos.x, pos.y] = block;

        var pipeBlock = block.GetComponent<PipeBlock>();
        if (pipeBlock != null)
        {
            pipeBlock.SetMatrixGame(this);
        }
    }

    private void SpawnStarAndEffect()
    {
        if (starPrefab == null || currentLevelConfig == null || currentLevelConfig.Blocks == null || currentLevelConfig.Blocks.Length == 0)
        {
            Debug.LogWarning("starPrefab, currentLevelConfig, hoặc Blocks là null hoặc rỗng");
            return;
        }

        Vector2Int starPos = currentLevelConfig.starPosition;
        if (!IsValidPosition(starPos))
        {
            starPos = currentLevelConfig.Blocks[currentLevelConfig.Blocks.Length - 1].position;
            currentLevelConfig.starPosition = starPos;
            Debug.LogWarning($"starPosition không hợp lệ, sử dụng vị trí khối cuối: {starPos}");
        }

        if (IsValidPosition(starPos))
        {
            if (starInstance != null)
            {
                Debug.Log($"Hủy star cũ tại {starInstance.transform.position}");
                Destroy(starInstance);
                starInstance = null;
            }
            starInstance = Instantiate(starPrefab, GetWorldPosition(starPos.x, starPos.y), Quaternion.identity);
            starInstance.name = "Star";
            Star starComponent = starInstance.GetComponent<Star>();
            if (starComponent == null)
            {
                starComponent = starInstance.AddComponent<Star>();
            }
            starComponent.ResetStar();
            starInstance.SetActive(true);

            if (breakEffectInstance != null)
            {
                Debug.Log($"Hủy break effect cũ tại {breakEffectInstance.transform.position}");
                Destroy(breakEffectInstance);
                breakEffectInstance = null;
            }
            if (breakEffectPrefab != null)
            {
                breakEffectInstance = Instantiate(breakEffectPrefab, GetWorldPosition(starPos.x, starPos.y), Quaternion.identity);
                breakEffectInstance.name = "BreakEffect";
                breakEffectInstance.SetActive(false);
                starComponent.SetBreakEffect(breakEffectInstance);
            }
            Debug.Log($"Star được tạo tại {starPos} với vị trí thế giới {GetWorldPosition(starPos.x, starPos.y)}");
        }
    }
    #endregion

    #region Matrix Operations
    public Vector2Int GetMatrixSize() => matrixSize;

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < matrixSize.x &&
               pos.y >= 0 && pos.y < matrixSize.y;
    }

    public GameObject GetBlockAt(Vector2Int pos)
    {
        return IsValidPosition(pos) ? matrix[pos.x, pos.y] : null;
    }

    public PipeBlock GetPipeBlockAt(Vector2Int pos)
    {
        GameObject block = GetBlockAt(pos);
        return block?.GetComponent<PipeBlock>();
    }

    public void UpdateMatrix(Vector2Int pos, GameObject block)
    {
        if (IsValidPosition(pos))
        {
            matrix[pos.x, pos.y] = block;
        }
    }
    #endregion

    #region Position Conversion
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(
            gridOrigin.x + x * cellSize,
            gridOrigin.y - y * cellSize,
            0f
        );
    }

    public Vector2Int WorldToMatrixPos(Vector2 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - gridOrigin.x) / cellSize);
        int y = Mathf.RoundToInt((gridOrigin.y - worldPos.y) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector2Int FindBlockPosition(GameObject block)
    {
        for (int x = 0; x < matrixSize.x; x++)
        {
            for (int y = 0; y < matrixSize.y; y++)
            {
                if (matrix[x, y] == block)
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }
    #endregion

    #region Player Movement
    public Vector2Int GetPlayerPosition()
    {
        return player?.GetCurrentPosition() ?? Vector2Int.zero;
    }

    private bool FindPath(Vector2Int start, Vector2Int end, ref Dictionary<Vector2Int, Vector2Int> parent)
    {
        if (!IsValidPosition(start) || !IsValidPosition(end)) return false;

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        parent.Clear();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = current + dir;
                if (IsValidPosition(next) && matrix[next.x, next.y] != null &&
                    !visited.Contains(next) && CanMove(current, next))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                    parent[next] = current;
                }
            }
        }

        return visited.Contains(end);
    }

    private bool CanMove(Vector2Int from, Vector2Int to)
    {
        var fromBlock = GetPipeBlockAt(from);
        var toBlock = GetPipeBlockAt(to);
        if (fromBlock == null || toBlock == null) return false;

        Vector2Int dir = to - from;
        return fromBlock.CanConnect(dir) && toBlock.CanConnect(-dir);
    }

    private IEnumerator MovePlayer(List<Vector2Int> path)
    {
        Debug.Log($"Bắt đầu di chuyển player qua {path.Count} bước");
        isMoving = true;
        Vector2Int currentPos = player.GetCurrentPosition();
        
        foreach (var pos in path)
        {
            AudioManager.Instance.PlayVFX(AudioManager.Instance.clip);
            yield return MovePlayerToPosition(currentPos, pos);
            currentPos = pos;
        }

        player.SetCurrentPosition(currentPos);
        isMoving = false;
        Debug.Log("Hoàn tất di chuyển player");
    }

    private IEnumerator MovePlayerToPosition(Vector2Int fromPos, Vector2Int toPos)
    {
        Vector3 startPos = GetWorldPosition(fromPos.x, fromPos.y);
        Vector3 endPos = GetWorldPosition(toPos.x, toPos.y);

        float duration = Vector3.Distance(startPos, endPos) / moveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            player.transform.position = Vector3.Lerp(
                startPos,
                endPos,
                Mathf.Clamp01(elapsedTime / duration)
            );
            yield return null;
        }

        player.transform.position = endPos;
    }

    private void OnLevelCompleted()
    {
        isMoving = true;
        gameManager.WinGame();
    }
    #endregion

    #region Block Movement
    public void TryMoveBlock(Vector2Int blockPos, Vector2Int direction)
    {
        GameObject block = GetBlockAt(blockPos);
        if (block == null) return;

        var pipeBlock = block.GetComponent<PipeBlock>();
        if (pipeBlock != null && !pipeBlock.CanMove()) return;
        if (pipeBlock != null && blockPos == GetPlayerPosition()) return;

        Vector2Int newPos = blockPos + direction;
        if (IsValidPosition(newPos) && GetBlockAt(newPos) == null)
        {
            matrix[blockPos.x, blockPos.y] = null;
            matrix[newPos.x, newPos.y] = block;
            StartCoroutine(MoveBlockSmoothly(block, newPos));
            AudioManager.Instance.PlayVFX(AudioManager.Instance.MoveBlock);
            isLv2++;
        }
    }

    private IEnumerator MoveBlockSmoothly(GameObject block, Vector2Int targetPos)
    {
        isMoving = true;
        Vector3 startPos = block.transform.position;
        Vector3 targetWorldPos = GetWorldPosition(targetPos.x, targetPos.y);

        float duration = Vector3.Distance(startPos, targetWorldPos) / moveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            block.transform.position = Vector3.Lerp(
                startPos,
                targetWorldPos,
                Mathf.Clamp01(elapsedTime / duration)
            );
            yield return null;
        }

        block.transform.position = targetWorldPos;
        isMoving = false;
    }
    #endregion

    #region Input Handling
    void Update()
    {
        if (!isMoving)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        Vector2 currentPos = Input.mousePosition;

        if (IsDoubleClick(timeSinceLastClick, currentPos))
        {
            ProcessDoubleClick(Camera.main.ScreenToWorldPoint(currentPos));
            lastClickTime = 0;
        }
        else
        {
            inputStartPos = currentPos;
            isDragging = true;
        }

        UpdateClickInfo(currentPos);
    }

    private bool IsDoubleClick(float timeSinceLastClick, Vector2 currentPos)
    {
        return timeSinceLastClick <= doubleClickThreshold &&
               Vector2.Distance(currentPos, lastClickPos) < 10f;
    }

    private void UpdateClickInfo(Vector2 currentPos)
    {
        lastClickTime = Time.time;
        lastClickPos = currentPos;
    }

    private void HandleMouseUp()
    {
        Vector2 dragDelta = (Vector2)Input.mousePosition - inputStartPos;
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputStartPos);
        Vector2Int targetPos = WorldToMatrixPos(worldPoint);

        if (ShouldCancelPlayerSelection(targetPos))
        {
            player?.SetUnselected();
        }
        else
        {
            ProcessDrag(inputStartPos, dragDelta.magnitude, GetDragDirection(dragDelta), targetPos);
        }
        isDragging = false;
    }

    private bool ShouldCancelPlayerSelection(Vector2Int targetPos)
    {
        return !IsValidPosition(targetPos) && player != null && player.IsSelected();
    }

    private Vector2Int GetDragDirection(Vector2 delta)
    {
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? delta.x > 0 ? Vector2Int.right : Vector2Int.left
            : delta.y > 0 ? Vector2Int.down : Vector2Int.up;
    }

    private void ProcessDoubleClick(Vector2 worldPoint)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

        TryHandlePlayerDoubleClick(hits);
        TryHandlePipeRotation(hits);
    }

    private void TryHandlePlayerDoubleClick(Collider2D[] hits)
    {
        
        foreach (var hit in hits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Player clickedPlayer = hit.GetComponent<Player>();
                clickedPlayer?.OnMouseDowns(false);

                return;
            }
        }
    }

    private void TryHandlePipeRotation(Collider2D[] hits)
    {
        foreach (var hit in hits)
        {
            PipeBlock pipeBlock = hit.GetComponent<PipeBlock>();
            if (pipeBlock != null && pipeBlock.CanRotate())
            {
                pipeBlock.RotateClockwise();
                return;
            }
        }
    }

    private void ProcessDrag(Vector2 startPos, float dragMagnitude, Vector2Int direction, Vector2Int targetPos)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(startPos);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

        if (TryHandlePlayerDrag(hits)) return;

        if (ShouldProcessPlayerMovement(targetPos, hits))
        {
            ProcessPlayerMovement(targetPos);
        }
        else if (ShouldProcessBlockMovement(dragMagnitude, hits))
        {
            ProcessBlockMovement(hits, direction);
        }
    }

    private bool TryHandlePlayerDrag(Collider2D[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Player clickedPlayer = hit.GetComponent<Player>();
                clickedPlayer?.OnMouseDowns();
                return true;
            }
        }
        return false;
    }

    private bool ShouldProcessPlayerMovement(Vector2Int targetPos, Collider2D[] hits)
    {
        return IsValidPosition(targetPos) &&
               player != null &&
               player.IsSelected() &&
               hits.Length > 0;
    }

    private void ProcessPlayerMovement(Vector2Int targetPos)
    {
        Vector2Int currentPlayerPos = player.GetCurrentPosition();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();

        if (FindPath(currentPlayerPos, targetPos, ref parent))
        {
            MovePlayerAlongPath(targetPos, parent);
        }
    }

    private bool ShouldProcessBlockMovement(float dragMagnitude, Collider2D[] hits)
    {
        return dragMagnitude >= minDragDistance && hits.Length > 0;
    }

    private void ProcessBlockMovement(Collider2D[] hits, Vector2Int direction)
    {
        foreach (var hit in hits)
        {
            Vector2Int blockPos = FindBlockPosition(hit.gameObject);
            if (blockPos != new Vector2Int(-1, -1))
            {
                TryMoveBlock(blockPos, direction);
                return;
            }
        }
    }

    private void MovePlayerAlongPath(Vector2Int targetPos, Dictionary<Vector2Int, Vector2Int> parent)
    {
        List<Vector2Int> path = ReconstructPath(targetPos, parent);
        if (path.Count > 0)
        {
            StartCoroutine(MovePlayer(path));
        }
    }

    private List<Vector2Int> ReconstructPath(Vector2Int targetPos, Dictionary<Vector2Int, Vector2Int> parent)
    {
        var path = new List<Vector2Int>();
        Vector2Int current = targetPos;

        while (current != player.GetCurrentPosition())
        {
            path.Add(current);
            if (!parent.ContainsKey(current))
            {
                path.Clear();
                break;
            }
            current = parent[current];
        }

        path.Reverse();
        return path;
    }
    #endregion

    #region Matrix Management
    public void DeactivateEffectAndStar(GameObject effect, GameObject star)
    {
        if (effect == null || star == null) return;
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        float duration = ps != null ? ps.main.duration : 2f;
        StartCoroutine(DeactivateEffectAndStarCoroutine(effect, star, duration));
    }

    private IEnumerator DeactivateEffectAndStarCoroutine(GameObject effect, GameObject star, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (effect != null)
        {
            effect.SetActive(false);
        }
        if (star != null)
        {
            star.SetActive(false);
        }
    }

    public void ResetLevel(LevelConfig newConfig = null)
    {
        Debug.Log("Bắt đầu reset level trong MatrixGame...");
        // Dừng tất cả coroutine để tránh xung đột
        StopAllCoroutines();
        player.SetUnselected();
        isMoving = false; // Đảm bảo trạng thái không bị kẹt

        // Kiểm tra và chạy hiệu ứng FadeIn
        if (fadePanel != null)
        {
            fadePanel.FadeIn(() =>
            {
                // Callback sau khi FadeIn hoàn tất
                PerformReset(newConfig);
                if (fadePanel != null)
                {
                    fadePanel.FadeOut(() => Debug.Log("Hiệu ứng FadeOut hoàn tất sau reset"));
                }
            });
        }
        else
        {
            Debug.LogWarning("FadePanel chưa được gán, thực hiện reset mà không có hiệu ứng mờ dần");
            PerformReset(newConfig);
        }
    }

    private void PerformReset(LevelConfig newConfig)
    {
        // Xóa các khối hiện tại
        foreach (var block in activeBlocks)
        {
            if (block != null)
            {
                Debug.Log($"Hủy khối {block.name} tại {block.transform.position}");
                Addressables.ReleaseInstance(block);
                Destroy(block);
            }
        }
        activeBlocks.Clear();

        // Xóa ma trận
        if (matrix != null)
        {
            for (int x = 0; x < matrixSize.x; x++)
            {
                for (int y = 0; y < matrixSize.y; y++)
                {
                    matrix[x, y] = null;
                }
            }
        }

        // Hủy star và effect
        if (starInstance != null)
        {
            Debug.Log($"Hủy star tại {starInstance.transform.position}");
            Destroy(starInstance);
            starInstance = null;
        }
        if (breakEffectInstance != null)
        {
            Debug.Log($"Hủy break effect tại {breakEffectInstance.transform.position}");
            Destroy(breakEffectInstance);
            breakEffectInstance = null;
        }

        // Đặt lại player
        if (player != null) 
        {
            player.SetUnselected();
            player.SetCurrentPosition(Vector2Int.zero);
            player.transform.position = GetWorldPosition(0, 0);
            Debug.Log($"Đặt lại player về vị trí {player.transform.position}");
        }

        // Cập nhật cấu hình level
        currentLevelConfig = newConfig ?? levelManager?.GetCurrentLevelConfig();
        if (currentLevelConfig == null)
        {
            Debug.LogWarning("Không có cấu hình level được cung cấp để reset, sử dụng cài đặt mặc định");
        }
        else
        {
            Debug.Log($"Sử dụng cấu hình level: {currentLevelConfig.name}");
        }

        // Khởi tạo lại cài đặt game và ma trận
        InitializeGameSettings();
        InitializeMatrix();

        // Đặt lại bộ đếm di chuyển
        isLv2 = 0;

        // Khởi động lại quá trình khởi tạo
        StartCoroutine(InitializeAndMovePlayer());
        Debug.Log("Reset level hoàn tất trong MatrixGame");
    }

    private void InitializeMatrix()
    {
        matrix = new GameObject[matrixSize.x, matrixSize.y];
        for (int x = 0; x < matrixSize.x; x++)
        {
            for (int y = 0; y < matrixSize.y; y++)
            {
                matrix[x, y] = null;
            }
        }
        Debug.Log($"Khởi tạo ma trận với kích thước {matrixSize}");
    }
    #endregion

    #region Public Interface
    public void SetMoving(bool moving) => isMoving = moving;
    public float GetMoveSpeed() => moveSpeed;

    public void SetLevelConfig(LevelConfig config)
    {
        Debug.Log($"Gọi SetLevelConfig trong MatrixGame với config: {(config != null ? config.name : "null")}");
        currentLevelConfig = config;
        ResetLevel(config); // Luôn reset, bỏ qua kiểm tra isInitialized
        isInitialized = true;
    }
    #endregion
}