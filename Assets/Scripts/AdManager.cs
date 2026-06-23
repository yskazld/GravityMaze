using UnityEngine;

public class AdManager : MonoBehaviour
{
    // Stub for reward ads. Integrate SDK (Unity Ads / AdMob) in project.

    public void ShowRewardedAd(System.Action onComplete, System.Action onFail = null)
    {
        Debug.Log("AdManager.ShowRewardedAd called - stubbed");
        // Immediately call complete for testing
        onComplete?.Invoke();
    }
}
