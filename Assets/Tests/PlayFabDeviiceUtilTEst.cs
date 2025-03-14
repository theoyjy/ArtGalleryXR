#undef DISABLE_PLAYFABCLIENT_API
using NUnit.Framework;
using PlayFab.Internal;
using PlayFab.SharedModels;
using PlayFab.ClientModels;
using PlayFab;

namespace PlayFab.SharedModels
{
    public class PlayFabResultCommon { }
}

namespace PlayFab.ClientModels
{
    public class LoginResult : PlayFabResultCommon
    {
        public UserSettings SettingsForUser;
        public string PlayFabId;
        public EntityTokenResponse EntityToken;
    }
    public class RegisterPlayFabUserResult : PlayFabResultCommon
    {
        public UserSettings SettingsForUser;
        public string PlayFabId;
        public EntityTokenResponse EntityToken;
    }
    public class UserSettings
    {
        public bool NeedsAttribution;
        public bool GatherDeviceInfo;
        public bool GatherFocusInfo;
    }
    public class EntityTokenResponse
    {
        public EntityToken Entity;
    }
    public class EntityToken
    {
        public string Id;
        public string Type;
    }
}

namespace PlayFab
{
    public class PlayFabApiSettings
    {
        public bool DisableDeviceInfo;
        public bool DisableFocusTimeCollection;
    }
}

namespace PlayFab.Internal
{
    public interface IPlayFabInstanceApi { }
    public static class PlayFabDeviceUtil
    {
        public static void OnPlayFabLogin(PlayFabResultCommon result, PlayFabApiSettings settings, IPlayFabInstanceApi instanceApi)
        {
        }
    }
}

public class DummyPlayFabInstanceApi : IPlayFabInstanceApi { }

public class PlayFabDeviceUtilTests
{
    [Test]
    public void OnPlayFabLogin_DoesNotThrowException_WithNullUserSettings()
    {
        var loginResult = new LoginResult
        {
            SettingsForUser = null,
            PlayFabId = "dummyPlayFabId",
            EntityToken = null
        };
        var settings = new PlayFabApiSettings();
        var dummyApi = new DummyPlayFabInstanceApi();
        Assert.DoesNotThrow(() => PlayFabDeviceUtil.OnPlayFabLogin(loginResult, settings, dummyApi));
    }
}
