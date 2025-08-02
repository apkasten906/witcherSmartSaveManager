namespace WitcherCore.Models
{
    /// <summary>
    /// Represents an inventory item in a Witcher 2 save file
    /// </summary>
    public class InventoryItem
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Weapon", "Armor", "Consumable", etc.
        public int Quantity { get; set; }
        public int Quality { get; set; } // Item quality/rarity level
        public bool IsEquipped { get; set; }
    }
}
