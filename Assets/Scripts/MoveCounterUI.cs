using TMPro;
using UnityEngine;

public class MoveCounterUI : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_Text counterText;

    private int moveCount;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (counterText == null)
        {
            counterText = GetComponent<TMP_Text>();
        }

        if (gameManager != null)
        {
            moveCount = gameManager.moveCount;
            gameManager.OnMoveCountChanged += HandleMoveCountChanged;
            RefreshText();
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnMoveCountChanged -= HandleMoveCountChanged;
        }
    }

    private void HandleMoveCountChanged(int newMoveCount)
    {
        moveCount = newMoveCount;
        RefreshText();
    }

    private void RefreshText()
    {
        if (counterText != null)
        {
            counterText.text = $"手数: {moveCount}手";
        }
    }
}
