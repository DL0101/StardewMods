﻿using System;
using System.Linq;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ExpandedStorage.Framework.Extensions
{
    public static class ItemExtensions
    {
        private const string CategoryFurniture = "category_furniture";
        private const string CategoryArtifact = "category_artifact";
        private const string DonateMuseum = "donate_museum";
        private const string DonateBundle = "donate_bundle";

        public static Chest ToChest(this Item item, Storage config = null)
        {
            // Get config for chest
            config ??= ExpandedStorage.GetConfig(item);
            
            // Create Chest from Item
            var chest = new Chest(true, Vector2.Zero, item.ParentSheetIndex)
            {
                name = item.Name,
                Stack = item.Stack,
                SpecialChestType = Enum.TryParse(config.SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                    ? specialChestType
                    : Chest.SpecialChestTypes.None
            };
            chest.fridge.Value = config.IsFridge;
            if (item.ParentSheetIndex == 216)
                chest.lidFrameCount.Value = 2;

            // Copy modData
            foreach (var modData in item.modData)
                chest.modData.CopyFrom(modData);
            
            if (item is not Chest oldChest)
                return chest;

            chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            if (oldChest.items.Any())
                chest.items.CopyFrom(oldChest.items);
            
            return chest;
        }

        public static bool MatchesTagExt(this Item item, string search) => item.MatchesTagExt(search, true);

        public static bool MatchesTagExt(this Item item, string search, bool exactMatch) =>
            item switch
            {
                Furniture when TagEquals(search, CategoryFurniture, exactMatch) => true,
                Object {Type: "Arch"} when TagEquals(search, CategoryArtifact, exactMatch) => true,
                Object {Type: "Arch"} when TagEquals(search, DonateMuseum, exactMatch) => CanDonateToMuseum(item),
                Object {Type: "Minerals"} when TagEquals(search, DonateMuseum, exactMatch) => CanDonateToMuseum(item),
                Object obj when TagEquals(search, DonateBundle, exactMatch) => CanDonateToBundle(obj),
                _ => item.GetContextTags().Any(tag => TagEquals(search, tag, exactMatch))
            };

        private static bool TagEquals(string search, string match, bool exact) =>
            exact && search.Equals(match) || match.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) > -1;

        private static bool CanDonateToMuseum(Item item) =>
            Game1.locations
                .OfType<LibraryMuseum>()
                .FirstOrDefault()?.isItemSuitableForDonation(item)
            ?? false;

        private static bool CanDonateToBundle(Object obj) =>
            Game1.locations
                .OfType<CommunityCenter>()
                .FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
            ?? false;
    }
}