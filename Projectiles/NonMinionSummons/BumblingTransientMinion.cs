﻿
using AmuletOfManyMinions.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.NonMinionSummons
{
	/**
     * Transient minion that stays in the approximate vicinity of the player while there is no enemy around
     */
	public abstract class BumblingTransientMinion : TransientMinion
	{
		protected float maxSpeed = default;
		protected Vector2 initialVelocity = Vector2.Zero;
		protected int lastHitFrame;
		protected virtual float inertia => default;
		protected virtual float idleSpeed => default;

		protected virtual int timeToLive => default;

		protected virtual float distanceToBumbleBack => default;

		protected virtual float searchDistance => default;
		protected virtual float noLOSSearchDistance => 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.localNPCHitCooldown = 30;
			Projectile.timeLeft = timeToLive;
			lastHitFrame = timeToLive + Projectile.localNPCHitCooldown;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = transformLight(lightColor);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;


			int height = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * height, texture.Width, height);

			SpriteEffects effects = GetSpriteEffects();
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, translucentColor, Projectile.rotation,
				bounds.GetOrigin(), 1, effects, 0);
			return false;
		}

		protected virtual SpriteEffects GetSpriteEffects()
		{
			if (Projectile.velocity.X > 0)
			{
				return SpriteEffects.FlipHorizontally;
			}
			return 0;
		}

		protected virtual void Move(Vector2 vector2Target, bool isIdle = false)
		{
			if((isIdle && vector2Target.LengthSquared() > idleSpeed * idleSpeed) ||
				(!isIdle && vector2Target.LengthSquared() > maxSpeed * maxSpeed))
			{
				vector2Target.SafeNormalize();
				vector2Target *= isIdle ? idleSpeed : maxSpeed;
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vector2Target) / inertia;
			base.TargetedMovement(vector2Target);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			Move(vectorToTargetPosition);
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			Move(vectorToIdlePosition, true);
		}

		public override Vector2 IdleBehavior()
		{
			if (maxSpeed == default)
			{
				maxSpeed = Projectile.velocity.Length();
				initialVelocity = Projectile.velocity;
			}
			Vector2 vector2Player = Player.Center - Projectile.Center;
			if (lastHitFrame - Projectile.timeLeft > Projectile.localNPCHitCooldown &&
				vector2Player.Length() > distanceToBumbleBack)
			{
				vector2Player.SafeNormalize();
				initialVelocity = vector2Player * maxSpeed;
			}
			return initialVelocity;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y != 0 && Projectile.velocity.Y == 0)
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			if (oldVelocity.X != 0 && Projectile.velocity.X == 0)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}
			initialVelocity = Projectile.velocity;
			return false;
		}

		protected virtual bool onAttackCooldown => lastHitFrame - Projectile.timeLeft < Projectile.localNPCHitCooldown;
		public override Vector2? FindTarget()
		{
			if (onAttackCooldown)
			{
				return null;
			}
			if (SelectedEnemyInRange(searchDistance, noLOSSearchDistance, maxRangeFromPlayer: false) is Vector2 closest)
			{
				return closest - Projectile.position;
			}
			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			lastHitFrame = Projectile.timeLeft;
		}
		protected virtual Color transformLight(Color color)
		{
			return color;
		}
	}
}
