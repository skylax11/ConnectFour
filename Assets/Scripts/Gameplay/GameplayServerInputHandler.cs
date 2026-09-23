using Unity.Netcode;
using UnityEngine;

public class GameplayServerInputHandler : NetworkBehaviour
{
    public static GameplayServerInputHandler Instance { get; private set; }

    [SerializeField]
    private int _columns = 7;

    [SerializeField]
    private Collider _board;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    [Range(0f, 0.45f)]
    private float _edgeInset = 0f;

    [SerializeField]
    private Transform _releasePoint;

    [SerializeField]
    private NetworkObject _blueChipPrefab;

    [SerializeField]
    private NetworkObject _redChipPrefab;

    [SerializeField]
    private float _previewHeight = 0.5f;

    [SerializeField]
    private float _padding;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitInputServerRpc(Vector2 clickPosition, Vector2 resolution, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        int column = GetColumn(clickPosition, resolution);

        if (column < 0)
        {
            return;
        }

        if (GameRuler.Instance == null || !GameRuler.Instance.PutChipOnColumn(clientId, column))
        {
            return;
        }

        GameRuler.Instance.CheckGameStatus();
        SpawnChip(column, GetClientTeam(clientId));
    }

    private void SpawnChip(int column, int team)
    {
        NetworkObject prefab = team == 0 ? _blueChipPrefab : _redChipPrefab;

        if (prefab == null)
        {
            return;
        }

        Vector3 position = GetColumnWorldPosition(column);
        Quaternion rotation = GetReleaseRotation();

        NetworkObject chip = Instantiate(prefab, position, rotation);
        chip.Spawn(true);
    }

    private int GetClientTeam(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            return -1;
        }

        if (client.PlayerObject == null)
        {
            return -1;
        }

        PlayerObject playerObject = client.PlayerObject.GetComponent<PlayerObject>();

        if (playerObject == null)
        {
            return -1;
        }

        return playerObject.Team.Value;
    }

    public int GetColumn(Vector2 clickPosition, Vector2 resolution)
    {
        if (resolution.x <= 0f || _board == null)
        {
            return -1;
        }

        Camera camera = _camera != null ? _camera : Camera.main;

        if (camera == null)
        {
            return -1;
        }

        Bounds bounds = _board.bounds;
        Vector3 center = bounds.center;

        float leftViewportX = camera.WorldToViewportPoint(new Vector3(bounds.min.x, center.y, center.z)).x;
        float rightViewportX = camera.WorldToViewportPoint(new Vector3(bounds.max.x, center.y, center.z)).x;

        if (Mathf.Approximately(rightViewportX, leftViewportX))
        {
            return -1;
        }

        float span = rightViewportX - leftViewportX;
        leftViewportX += span * _edgeInset;
        rightViewportX -= span * _edgeInset;

        float normalizedX = clickPosition.x / resolution.x;
        float t = (normalizedX - leftViewportX) / (rightViewportX - leftViewportX);

        if (t < 0f || t > 1f)
        {
            return -1;
        }

        int column = Mathf.FloorToInt(t * _columns);

        return Mathf.Clamp(column, 0, _columns - 1);
    }

    public bool TryGetPreviewPosition(Vector2 clickPosition, Vector2 resolution, out Vector3 position)
    {
        position = Vector3.zero;

        int column = GetColumn(clickPosition, resolution);

        if (column < 0)
        {
            return false;
        }

        position = GetColumnWorldPosition(column);
        return true;
    }

    public Quaternion GetReleaseRotation()
    {
        return _releasePoint != null ? _releasePoint.rotation : Quaternion.identity;
    }

    public Vector3 GetColumnWorldPosition(int column)
    {
        Bounds bounds = _board.bounds;

        float worldWidth = bounds.size.x;
        float gridLeft = bounds.min.x + worldWidth * _edgeInset;
        float gridRight = bounds.max.x - worldWidth * _edgeInset;
        float columnWidth = (gridRight - gridLeft) / _columns;

        float x = gridLeft + (column + _padding) * columnWidth;
        float y = _releasePoint != null ? _releasePoint.position.y : bounds.max.y + _previewHeight;
        float z = _releasePoint != null ? _releasePoint.position.z : bounds.center.z;

        return new Vector3(x, y, z);
    }
}
