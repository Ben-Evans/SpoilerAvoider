using Blazored.LocalStorage;

namespace SpoilerFreeHighlights.Client.Services;

public class LocalCacheService(ILocalStorageService _localStorage)
{
    private const string UserPreferenceId = "User-Preferences";

    public async Task<UserPreference> GetUserPreferences()
    {
        return await _localStorage.GetItemAsync<UserPreference>(UserPreferenceId) ?? new();
    }

    public async Task SetUserPreferences(UserPreference userPreferences)
    {
        await _localStorage.SetItemAsync(UserPreferenceId, userPreferences);
    }

    public async Task ClearUserPreferences(UserPreference userPreferences)
    {
        await _localStorage.RemoveItemAsync(UserPreferenceId);

        foreach(var leaguePreference in userPreferences.LeaguePreferences)
            leaguePreference.Value.Clear();
    }
}
