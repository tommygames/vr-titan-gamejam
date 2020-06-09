using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    public void Start()
    {
        Debug.Assert(string.IsNullOrEmpty(PlayFabSettings.TitleId) == false, "PlayFabSettings.TitleId is null or empty. Please finish setting up your GameJam project.");
#if UNITY_IOS && !UNITY_EDITOR
        var request = new LoginWithGameCenterRequest {TitleId = PlayFabSettings.TitleId, CreateAccount = true};
        PlayFabClientAPI.LoginWithGameCenter(request, OnLoginSuccess, OnLoginFailure);
#elif UNITY_ANDROID && !UNITY_EDITOR
        var request = new LoginWithGoogleAccountRequest { TitleId = PlayFabSettings.TitleId, CreateAccount = true};
        PlayFabClientAPI.LoginWithGoogleAccount(request, OnLoginSuccess, OnLoginFailure);
#else
        var request = new LoginWithCustomIDRequest { CustomId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true};
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
#endif
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }
}