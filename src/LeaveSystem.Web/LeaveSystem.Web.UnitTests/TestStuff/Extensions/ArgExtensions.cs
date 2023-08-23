namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class ArgExtensions
{
    public static T IsEquivalentTo<T>(object source) where T : class =>
        Arg.Is<T>(x => x.IsEquivalentTo(source));

    private static bool IsEquivalentTo<T>(this T source, object target) where T : class
    {
        var properties = source.GetType().GetProperties();
        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var targetProperty = target.GetType().GetProperty(propertyName);
            if (targetProperty is null)
            {
                return false;
            }
            var valuesAreEquals = targetProperty.GetValue(target)?.Equals(targetProperty.GetValue(source));
            if (valuesAreEquals == false)
            {
                return false;
            }
        }
        return true;
    }
    
    public static IEnumerable<T> IsCollectionEquivalentTo<T>(IEnumerable<T> source) =>
        Arg.Is<IEnumerable<T>>(x => x.SequenceEqual(source)); 
}