﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Core.Minions.CombatPetsQuiz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Core.BackportUtils;

namespace AmuletOfManyMinions.Items.Consumables
{
	class CombatPetQuizItems
	{
	}

	public class CombatPetFriendshipBow: BackportModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Grants you a special combat pet based on your personality!\n" +
				"Your environment may influence the result...");
			DisplayName.SetDefault("Bow of Friendship");
		}

		public static void SetFriendshipBowDefaults(Item Item)
		{
			Item.maxStack = 1;
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.HoldingUp;
			Item.UseSound = new LegacySoundStyle(2, 17);
			Item.consumable = true;
		}

		public override void SetDefaults()
		{
			SetFriendshipBowDefaults(Item);
		}

		public override bool CanUseItem(Player player) => !player.GetModPlayer<CombatPetsQuizModPlayer>().IsTakingQuiz;

		public override bool UseItem(Player player)
		{
			if(player.whoAmI == Main.myPlayer)
			{
				player.GetModPlayer<CombatPetsQuizModPlayer>().StartPersonalityQuiz(Type);
			}
			return false;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D texture = Main.itemTexture[Type];
			spriteBatch.Draw(texture, position, frame, Main.DiscoColor, 0, origin, scale, SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D texture = Main.itemTexture[Type];
			spriteBatch.Draw(texture, Item.position - Main.screenPosition, texture.Bounds, Main.DiscoColor, 0, Vector2.Zero, scale, SpriteEffects.FlipHorizontally, 0);
			return false;
		}
	}

	public class CombatPetTeamworkBow: BackportModItem
	{
		internal static string FriendshipBowRequirement =
			"You must use the Bow of Friendship before using this item.\n" +
			"Try searching wooden chests near spawn!";
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			base.ModifyTooltips(tooltips);
			if(Main.player[Main.myPlayer].GetModPlayer<CombatPetsQuizModPlayer>().HasTakenQuiz)
			{
				return;
			}
			tooltips.Add(new TooltipLine(Mod, "FriendshipBowRequirement", FriendshipBowRequirement)
			{
				overrideColor = Color.Gray
			});
		}

		public override void SetDefaults()
		{
			CombatPetFriendshipBow.SetFriendshipBowDefaults(Item);
		}

		public override bool CanUseItem(Player player) {
			var quizPlayer = player.GetModPlayer<CombatPetsQuizModPlayer>();
			if(quizPlayer.IsTakingQuiz)
			{
				return false;
			}
			if(!quizPlayer.HasTakenQuiz)
			{
				Main.NewText(FriendshipBowRequirement);
				return false;
			}
			return true;
		} 
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault(
				"Invites another special combat pet to your team!\n" +
				"It will always be different from the last pet that joined.");
			DisplayName.SetDefault("Bow of Teamwork");
		}

		public override bool UseItem(Player player)
		{
			if(player.whoAmI == Main.myPlayer)
			{
				player.GetModPlayer<CombatPetsQuizModPlayer>().StartPartnerQuiz(Type);
			}
			return false;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<InertCombatPetFriendshipBow>(), 1)
			.AddIngredient(ItemID.Seashell, 3)
			.AddIngredient(ItemID.JungleSpores, 4)
			.AddIngredient(ItemID.Cloud, 12)
			.AddIngredient(ItemID.Sandstone, 12)
			.AddTile(TileID.WorkBenches)
			.Register();


	}



	public class CombatPetAncientFriendshipBow: BackportModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Grants you a special combat pet of your choosing!");
			DisplayName.SetDefault("Ancient Bow of Friendship");
		}

		public override void SetDefaults()
		{
			CombatPetFriendshipBow.SetFriendshipBowDefaults(Item);
			Item.rare = ItemRarityID.Yellow;
		}

		public override bool CanUseItem(Player player) => !player.GetModPlayer<CombatPetsQuizModPlayer>().IsTakingQuiz;

		public override bool UseItem(Player player)
		{
			if(player.whoAmI == Main.myPlayer)
			{
				CombatPetsQuizModPlayer modPlayer = player.GetModPlayer<CombatPetsQuizModPlayer>();
				if(modPlayer.HasTakenQuiz)
				{
					modPlayer.StartAnyPartnerQuiz(Type);
				} else
				{
					modPlayer.StartPersonalityQuiz(Type);
				}
			}
			return false;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.LunarTabletFragment, 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
