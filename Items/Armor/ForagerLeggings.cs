using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using System.ComponentModel;

namespace DemoMod.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class ForagerLeggings : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Forager's Legs");
			Tooltip.SetDefault(
				"4% increased minion damage\n"
				+ "5% increased movement speed");
		}

		public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.value = Item.sellPrice(silver: 1);
			item.rare = ItemRarityID.White;
			item.defense = 3;
		}

		public override void UpdateEquip(Player player) {
			player.minionDamageMult += 0.04f;
			player.moveSpeed += 0.05f;
		}

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Acorn, 2);
            recipe.AddIngredient(ItemID.Wood, 12);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
	}
}