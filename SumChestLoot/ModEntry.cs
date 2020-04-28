using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.IO;
using StardewValley.Objects;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SumChestLoot
{
    class ModEntry : Mod
    {
        /// <summary>The item ID for auto-grabbers.</summary>
        private readonly int AutoGrabberID = 165;
        private ModConfig config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            this.config = helper.ReadConfig<ModConfig>();

            helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;

        }
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            IClickableMenu menu = Game1.activeClickableMenu;
            int totalSellPrice = 0;

            var context = GetContextFromMenu(menu);
            if (context is null)
                return;

            var chest = GetInventoryFromContext(context);
            if (chest is null || chest.Count == 0)
                return;

            foreach (var item in chest)
            {
                if (item is null)
                    continue;

                int itemPrice = GetTruePrice(item) / 2;

                totalSellPrice += itemPrice * item.Stack;
            }

            if (totalSellPrice > 0)
            {
                int offset = 0;
                switch (context)
                {
                    case JunimoHut _:
                        offset = 140;
                        break;
                    case Chest c when c.fridge.Value:
                        offset = 140;
                        break;
                    //add logic to determine if color picker is on or off and move height accordingly
                    case Chest c:
                        offset = 215;
                        break;
                    // auto-grabber                    
                    case StardewValley.Object obj when obj.ParentSheetIndex == this.AutoGrabberID:
                        offset = 140;
                        break;
                    case StorageFurniture _:
                        offset = 140;
                        break;
                    //farm?
                    case Farm _:
                    case ShippingBin _:
                        offset = -200;
                        break;
                    default:
                        offset = 140;
                        break;
                }
                Vector2 tooltipPosition = new Vector2(menu.xPositionOnScreen - 0, menu.yPositionOnScreen - offset);
                CommonHelper.DrawHoverBox(e.SpriteBatch, "Total Sell Price: " + totalSellPrice, tooltipPosition, Game1.viewport.Width - tooltipPosition.X - Game1.tileSize / 2f);
            }

        }

        /// <summary>Get the player chest from the given menu, if any.</summary>
        /// <param name="menu">The menu to check.</param>
        private object GetContextFromMenu(IClickableMenu menu)
        {
            switch (menu)
            {
                case ItemGrabMenu itemGrabMenu:
                    return itemGrabMenu.context;
                case ShopMenu shopMenu:
                    return shopMenu.source;
            }

            return null;
        }

        /// <summary>Get the underlying inventory for an <see cref="ItemGrabMenu.context"/> value.</summary>
        /// <param name="context">The menu context.</param>
        private IList<Item> GetInventoryFromContext(object context)
        {
            switch (context)
            {
                // chest
                case Chest chest:
                    return chest.items;

                // auto-grabber
                case StardewValley.Object obj when obj.ParentSheetIndex == this.AutoGrabberID:
                    return (obj.heldObject.Value as Chest)?.items;

                // buildings
                case JunimoHut hut:
                    return hut.output.Value?.items;
                case Mill mill:
                    return mill.output.Value?.items;

                // shipping bin
                case Farm _:
                case ShippingBin _:
                    return Game1.getFarm().getShippingBin(Game1.player);

                // dresser
                case StorageFurniture furniture:
                    return furniture.heldItems;

                // unsupported type
                default:
                    return null;
            }
        }


        public static int GetTruePrice(Item item)
        {
            int truePrice = 0;

            if (item is StardewValley.Object objectItem)
            {
                truePrice = objectItem.sellToStorePrice() * 2;
            }
            else if (item is StardewValley.Item thing)
            {
                truePrice = thing.salePrice();
            }

            return truePrice;
        }




    }
}
