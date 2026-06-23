using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
#endif

public class UIController : MonoBehaviour
{
    public GameManager gameManager;

    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        EnsureInputSystemUiModule();
#endif

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

#if ENABLE_INPUT_SYSTEM
    private static void EnsureInputSystemUiModule()
    {
        var eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            return;
        }

        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneModule != null)
        {
            Destroy(standaloneModule);
            Debug.Log("UIController: Replaced StandaloneInputModule with InputSystemUIInputModule");
        }
    }
#endif

    public void OnUp()
    {
        Debug.Log("UIController: OnUp");
        if (gameManager == null) Debug.LogWarning("UIController: GameManager is null");
        gameManager?.MoveUp();
    }

    public void OnDown()
    {
        Debug.Log("UIController: OnDown");
        if (gameManager == null) Debug.LogWarning("UIController: GameManager is null");
        gameManager?.MoveDown();
    }

    public void OnLeft()
    {
        Debug.Log("UIController: OnLeft");
        if (gameManager == null) Debug.LogWarning("UIController: GameManager is null");
        gameManager?.MoveLeft();
    }

    public void OnRight()
    {
        Debug.Log("UIController: OnRight");
        if (gameManager == null) Debug.LogWarning("UIController: GameManager is null");
        gameManager?.MoveRight();
    }
}
