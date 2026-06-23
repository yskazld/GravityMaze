using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAction : MonoBehaviour
{
    public Button targetButton;
    public MonoBehaviour targetObject;
    public string methodName;

    private void Awake()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        if (targetButton == null || targetObject == null || string.IsNullOrEmpty(methodName))
        {
            return;
        }

        var method = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
        if (method == null)
        {
            Debug.LogWarning($"ButtonAction: method '{methodName}' not found on {targetObject.GetType().Name}", targetObject);
            return;
        }

        targetButton.onClick.AddListener(() => method.Invoke(targetObject, null));
    }
}
