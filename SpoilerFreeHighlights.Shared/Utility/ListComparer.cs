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

    public static ComparisonResult<T> CompareWith<T, TKey>(
        this IEnumerable<T> existingList,
        IEnumerable<T> newList,
        Func<T, TKey> keySelector)
    {
        Dictionary<TKey, T> existingDict = existingList.ToDictionary(keySelector);
        Dictionary<TKey, T> newDict = newList.ToDictionary(keySelector);

        HashSet<TKey> existingKeys = existingDict.Keys.ToHashSet();
        HashSet<TKey> newKeys = newDict.Keys.ToHashSet();

        T[] newItems = newKeys.Except(existingKeys)
            .Select(key => newDict[key])
            .ToArray();

        T[] removedItems = existingKeys.Except(newKeys)
            .Select(key => existingDict[key])
            .ToArray();

        (T, T)[] sameItems = existingKeys.Intersect(newKeys)
            .Select(key => (existingDict[key], newDict[key]))
            .ToArray();

        return new ComparisonResult<T>(newItems, removedItems, sameItems);
    }
}

public record ComparisonResult<T>(T[] NewItems, T[] RemovedItems, (T Existing, T New)[] SameItems);
