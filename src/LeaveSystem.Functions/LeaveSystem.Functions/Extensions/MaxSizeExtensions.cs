namespace LeaveSystem.Functions.Extensions;
internal static class MaxSizeExtensions
{
    private const int MaxPageSize = 25;
    public static int PageSizeOrMax(this int? pageSize) => pageSize < MaxPageSize ? pageSize ?? MaxPageSize : MaxPageSize;
}
