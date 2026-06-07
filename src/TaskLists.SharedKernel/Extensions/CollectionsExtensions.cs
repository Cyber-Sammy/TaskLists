namespace TaskLists.SharedKernel.Extensions;

public static class CollectionsExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
    {
        return collection == null || !collection.Any();
    }

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> collection)
    {
        return collection != null && collection.Any();
    }
}
