namespace ManagedCode.Repository.Core
{
    public interface IItem<TId>
    {
        TId Id { get; set; }
    }
}