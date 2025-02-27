﻿using AmuletOfManyMinions.Core.BackportUtils;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories.CombatPetAccessories
{
	class CombatPetStylishTeamworkBow : BackportModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Stylish Bow of Teamwork");
			Tooltip.SetDefault("Increases max number of combat pets by 1");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += 1;
		}
	}

	class CombatPetMightyTeamworkBow : BackportModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mighty Bow of Teamwork");
			Tooltip.SetDefault(
				"Increases max number of combat pets by 2,\n" +
				"but decreases max number of non-combat pet minions by 1\n" +
				"Increases combat pet movement speed\n" +
				"Increases minion damage\n");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamage += 0.2f;
			player.maxMinions = Math.Max(0, player.maxMinions - 1);
			// Reducing max minions by one also decreases max combat pets by one,
			// So increase max combat pets by 2 for a total increase of 1 combat pet (a bit confusing)
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += 2;
			player.GetModPlayer<LeveledCombatPetModPlayer>().PetSpeedBonus += 2;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<CharmOfMightyMinions>(), 1)
			.AddRecipeGroup("AmuletOfManyMinions:CombatPetChewToys")
			.AddIngredient(ModContent.ItemType<CombatPetStylishTeamworkBow>(), 1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
	class CombatPetSpookyTeamworkBow : BackportModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spooky Bow of Teamwork");
			Tooltip.SetDefault(
				"Increases max number of combat pets by 2\n" +
				"Increases combat pet movement speed\n" +
				"Increases minion damage\n");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Yellow;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamage += 0.2f;
			player.GetModPlayer<LeveledCombatPetModPlayer>().PetSpeedBonus += 2;
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += 2;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.NecromanticScroll, 1)
			.AddIngredient(ModContent.ItemType<CombatPetMightyTeamworkBow>(), 1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}

	abstract class CombatPetChewToy: BackportModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Chaotic Chew Toy");
			Tooltip.SetDefault("Increases combat pet movement speed");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<LeveledCombatPetModPlayer>().PetSpeedBonus += 2;
		}
	}
	class CombatPetChaoticChewToy : CombatPetChewToy
	{
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.DemoniteBar, 12)
			.AddIngredient(ItemID.ShadowScale, 6)
			.AddTile(TileID.Anvils)
			.Register();
	}
	class CombatPetCrimsonChewToy : CombatPetChewToy
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Chew Toy");
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.CrimtaneBar, 12)
			.AddIngredient(ItemID.TissueSample, 6)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
