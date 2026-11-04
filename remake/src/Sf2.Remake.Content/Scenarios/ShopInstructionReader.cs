using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class ShopInstructionReader
{
    internal static OpenShop Read(JsonElement row, PrivateBattleDefinitions? source)
    {
        ObjectOptional(row, "shop", ["op", "stock"], "items");
        var stock = Array(row, "stock").Select(value =>
        {
            Require(value.ValueKind == JsonValueKind.Number && value.TryGetByte(out byte id) && id <= 126, "shop-stock", "shop.stock");
            return value.GetByte();
        }).ToArray();
        IEnumerable<ShopItem> items;
        if (source is not null)
        {
            Require(!row.TryGetProperty("items", out _), "private-shop-items", "shop.items");
            items = source.Items.Values.Select(item => new ShopItem(item.Id, item.DisplayName, item.Price,
                item.ItemTypeExpression.Split('|').Contains("UNSELLABLE"),
                item.ItemTypeExpression.Split('|').Contains("RARE")));
            Require(stock.All(id => source.Items.TryGetValue(id, out var item) && item.ItemTypeExpression is "WEAPON" or "CONSUMABLE"),
                "shop-stock-unsupported", "shop.stock", true);
        }
        else
        {
            items = Array(row, "items").Select(item =>
            {
                Object(item, "shop.item", "id", "name", "price", "unsellable", "rare");
                string name = Text(item, "name");
                Require(name.Length is > 0 and <= 64, "shop-item-name", "shop.items.name");
                return new ShopItem((byte)Number(item, "id", 0, 126), name,
                    (ushort)Number(item, "price", 0, 65535), Boolean(item, "unsellable"), Boolean(item, "rare"));
            }).ToArray();
        }
        Require(stock.Length is >= 1 and <= 16 && stock.Distinct().Count() == stock.Length,
            "shop-stock", "shop.stock");
        var catalog = items.ToArray();
        Require(catalog.Length <= 127 && catalog.Select(item => item.ItemId).Distinct().Count() == catalog.Length &&
            stock.All(id => catalog.Any(item => item.ItemId == id && !item.Rare && !item.Unsellable)), "shop-catalog", "shop.items");
        return new(new(stock, catalog));
    }
}
