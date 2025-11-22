public interface IBuildingSite
{
    bool NeedsResource(ItemType type);
    void AddResource(ItemType type, int amount);
}