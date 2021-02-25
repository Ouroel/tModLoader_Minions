﻿using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	class MinionWaypoint : ModProjectile
	{
		public const int duration = 180000; // a long time
		private int rotationFrame = 0;
		public const int rotationFrames = 60;

		public static int Type => ProjectileType<MinionWaypoint>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.damage = 0;
			projectile.width = 1;
			projectile.height = 1;
			projectile.tileCollide = false;
			projectile.timeLeft = duration;
			projectile.friendly = false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}

		public override void AI()
		{
			rotationFrame = (rotationFrame + 1) % rotationFrames;
			float startAngle = -2f * (float)Math.PI * rotationFrame / rotationFrames;
			MinionPathfindingPlayer player = Main.player[projectile.owner].GetModPlayer<MinionPathfindingPlayer>();
			if(player.pHelper.searchSucceeded || !player.pHelper.searchFailed)
			{

				int radius = player.pHelper.searchSucceeded ? 12 : 6;
				Color color = player.pHelper.searchSucceeded  && player.InWaypointRange(projectile.Center) ? Color.LimeGreen : Color.Gray;
				float scale = player.pHelper.searchSucceeded ? 1.2f : 0.8f; 
				for (int i = 0; i < 3; i++)
				{
					float angle = startAngle + i * 2 * (float)Math.PI / 3;
					Vector2 pos = projectile.Center + radius * angle.ToRotationVector2();
					Dust.NewDust(pos, 1, 1, DustType<MinionWaypointDust>(), newColor: color, Scale: scale);
				}
			} else if (player.pHelper.searchFailed)
			{
				for(int i = 0; i < 2; i++)
				{
					float offset = 12 * (i == 0 ? (float)Math.Sin(startAngle) : (float)Math.Cos(startAngle));
					Vector2 pos = projectile.Center + new Vector2(i == 1 ? offset : -offset, offset);
					Dust.NewDust(pos, 1, 1, DustType<MinionWaypointDust>(), newColor: Color.Red, Scale: 1.2f);
				}
			}
		}

		// doesn't matter, never drawn
		public override string Texture => "Terraria/NPC_0";
	}
}
