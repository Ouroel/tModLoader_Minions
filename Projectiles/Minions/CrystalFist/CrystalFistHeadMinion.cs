﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CrystalFist
{
	/// <summary>
	/// Uses ai[0] to track animation frames
	/// </summary>
	public class CrystalFistHeadMinion : SimpleMinion
	{
		protected int targetedInertia = 15;
		protected int targetedSpeed = 12;
		protected int maxDistanceFromPlayer = 850;
		protected int minDistanceToEnemy = 200;
		protected int animationFrames = 120;

		internal override int BuffId => BuffType<CrystalFistMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crystal Fist");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
			IdleLocationSets.trailingInAir.Add(projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 36;
			projectile.height = 60;
			projectile.tileCollide = false;
			projectile.friendly = false;
			projectile.minionSlots = 0f;
			attackThroughWalls = false;
		}

		public override Vector2? FindTarget()
		{
			if (PlayerTargetPosition(950f, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(950f) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = true;
			return true;
		}
		public override Vector2 IdleBehavior()
		{
			Vector2 idlePosition = player.Top;
			projectile.ai[0] = (projectile.ai[0] + 1) % animationFrames;
			float idleAngle = (float)Math.PI * 2 * projectile.ai[0] / animationFrames;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, projectile);
			idlePosition.Y += -5 + 8 * (float)Math.Sin(idleAngle);
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			int inertia = 10;
			int maxSpeed = 16;
			projectile.tileCollide = false;
			Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			projectile.spriteDirection = -player.direction;
			projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int inertia = targetedInertia;
			int maxSpeed = targetedSpeed;
			// move towards the enemy, but don't get too far from the player
			projectile.spriteDirection = vectorToTargetPosition.X > 0 ? -1 : 1;
			Vector2 vectorFromPlayer = player.Center - projectile.Center;
			if (vectorFromPlayer.Length() > maxDistanceFromPlayer)
			{
				vectorToTargetPosition = vectorFromPlayer;
			}
			else if (vectorToTargetPosition.Length() < minDistanceToEnemy)
			{
				vectorToTargetPosition *= -1;
			}
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= maxSpeed;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
		}
	}
}
