using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record ShopItem(byte ItemId, string Name, ushort Price, bool Unsellable = false, bool Rare = false);

public sealed class ShopDefinition
{
    internal ShopDefinition(IEnumerable<byte> stock, IEnumerable<ShopItem> items)
    {
        Stock = Array.AsReadOnly(stock.ToArray());
        Items = new ReadOnlyDictionary<byte, ShopItem>(items.ToDictionary(item => item.ItemId));
        if (Stock.Count == 0 || Stock.Distinct().Count() != Stock.Count || Stock.Any(id => !Items.ContainsKey(id)))
            throw new ArgumentException("Shop stock must refer to distinct admitted items.");
    }
    public IReadOnlyList<byte> Stock { get; }
    public IReadOnlyDictionary<byte, ShopItem> Items { get; }
}

public sealed record OpenShop(ShopDefinition Definition) : StoryInstruction;
