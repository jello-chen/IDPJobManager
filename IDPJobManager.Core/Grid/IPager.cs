namespace IDPJobManager.Core.Grid
{
    public interface IPager
    {
        int PageCurrent { get; set; }
        int PageSize { get; set; }
    }
}
