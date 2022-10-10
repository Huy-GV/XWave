using System.Collections.ObjectModel;

namespace XWave.Core.Extension;

public static class CollectionExtension 
{
    public static IReadOnlyCollection<T> AsIReadonlyCollection<T>(this IList<T> list)
    {
        return (IReadOnlyCollection<T>)new ReadOnlyCollection<T>(list);
    }
}