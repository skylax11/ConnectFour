using TMPro;
using Unity.Collections;
using UnityEngine;

public class LobbyPlayer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _nameTag;

    [SerializeField]
    private TextMeshProUGUI _isReady;

    public void Setup(FixedString32Bytes name,bool isReady)
    {
        _nameTag.text = name.ToString();
        _isReady.text = isReady ? "Ready" : "Not Ready";
    }
}
