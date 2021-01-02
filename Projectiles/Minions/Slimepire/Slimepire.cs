﻿using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Slimepire
{
	public class SlimepireMinionBuff : MinionBuff
	{
		public SlimepireMinionBuff() : base(ProjectileType<SlimepireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Slimepire");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class SlimepireMinionItem : MinionItem<SlimepireMinionBuff, SlimepireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimepire Staff");
			Tooltip.SetDefault("Summons a winged slime to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 10;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.White;
		}
	}

	public class SlimepireMinion : SimpleGroundBasedMinion<SlimepireMinionBuff>, IGroundAwareMinion
	{
		private float intendedX = 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimepire");
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 20;
			drawOffsetX = (projectile.width - 44) / 2;
			drawOriginOffsetY = (projectile.height - 32) / 2;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}

		protected override bool DoPreStuckCheckGroundedMovement()
		{
			if(!gHelper.didJustLand)
			{
				projectile.velocity.X = intendedX;
				// only path after landing
				return false;
			}
			return true;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{
			gHelper.DoJump(vector);
			int maxHorizontalSpeed = vector.Y < -64 ? 3 : 6;
			projectile.velocity.X = Math.Max(1, Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) /16)) * Math.Sign(vector.X);
			intendedX = projectile.velocity.X;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = gHelper.isFlying ? 0 : 4;
			maxFrame = gHelper.isFlying ? 4 : 6;
			base.Animate(minFrame, maxFrame);
		}
	}
}