namespace IDPJobManager.Core
{
    public interface ICommandInvoker<in TIn, out TOut>
    {
        TOut Execute(TIn command);
    }
}
