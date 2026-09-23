using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject _bluePreviewChipPrefab;

    [SerializeField]
    private GameObject _redPreviewChipPrefab;

    private GameObject _previewChip;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        GameplayServerInputHandler handler = GameplayServerInputHandler.Instance;

        if (handler == null || !handler.IsSpawned)
        {
            HidePreview();
            return;
        }

        Vector2 clickPosition = Input.mousePosition;
        Vector2 resolution = new Vector2(Screen.width, Screen.height);

        if (handler.TryGetPreviewPosition(clickPosition, resolution, out Vector3 previewPosition))
        {
            ShowPreview(previewPosition, handler.GetReleaseRotation());
        }
        else
        {
            HidePreview();
        }

        if (Input.GetMouseButtonDown(0))
        {
            handler.SubmitInputServerRpc(clickPosition, resolution);
        }
    }

    private void ShowPreview(Vector3 position, Quaternion rotation)
    {
        if (_previewChip == null)
        {
            GameObject prefab = GetPreviewPrefab();

            if (prefab == null)
            {
                return;
            }

            _previewChip = Instantiate(prefab);
        }

        if (!_previewChip.activeSelf)
        {
            _previewChip.SetActive(true);
        }

        _previewChip.transform.SetPositionAndRotation(position, rotation);
    }

    private GameObject GetPreviewPrefab()
    {
        PlayerObject playerObject = GetComponent<PlayerObject>();
        int team = playerObject != null ? playerObject.Team.Value : 0;

        return team == 0 ? _bluePreviewChipPrefab : _redPreviewChipPrefab;
    }

    private void HidePreview()
    {
        if (_previewChip != null && _previewChip.activeSelf)
        {
            _previewChip.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_previewChip != null)
        {
            Destroy(_previewChip);
        }
    }
}
