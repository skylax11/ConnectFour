using TMPro;
using UnityEngine;

public class GameSceneClientUI : MonoBehaviour
{

    public static GameSceneClientUI Instance;

    [SerializeField]
    private TextMeshProUGUI _turnText;

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

    public void UpdateTurnText(string text)
    {
        _turnText.text = text;
    }

}
