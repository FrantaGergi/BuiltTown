using UnityEngine;

public interface IBuildingSite
{
    bool NeedsResourceForCollectors(ItemType type);
    bool NeedsResourceForBuilders(ItemType stone);

    void AddResourceByCollector(ItemType type, int amount);
    void AddResourceByBuilder(ItemType type, int amount);
    Vector3 GetHolderPosition();
}