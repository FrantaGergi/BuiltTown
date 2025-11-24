public interface IResourceSource
{
    ItemType Type { get; }
    bool CanMine();
    void MineOnce(int quantity);
}