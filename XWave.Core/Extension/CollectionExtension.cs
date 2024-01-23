using System.Collections.ObjectModel;

namespace XWave.Core.Extension;

public static class CollectionExtension 
{
    public static IReadOnlyCollection<T> AsIReadonlyCollection<T>(this IList<T> list)
    {
        return new ReadOnlyCollection<T>(list);
    }
}