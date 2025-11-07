namespace SpoilerFreeHighlights.Shared.Utility;

public static class ListComparer
{
    /// <summary>
    /// Returns items in <paramref name="newList"/> that are not in <paramref name="existingList"/>.
    /// </summary>
    public static T[] GetNewItems<T, TKey>(
        this IEnumerable<T> existingList,
        IEnumerable<T> newList,
        Func<T, TKey> keySelector)
    {
        HashSet<TKey> existingKeys = [.. existingList.Select(keySelector)];
        return newList.Where(item => !existingKeys.Contains(keySelector(item))).ToArray();
    }
}
