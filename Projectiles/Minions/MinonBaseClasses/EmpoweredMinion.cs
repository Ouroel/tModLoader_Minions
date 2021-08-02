﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	// Invisible minion that exists to track the empower count of empowered minions
	// Works around many of the issues involved with changing a projectiles minionSlots
	public abstract class CounterMinion : SimpleMinion
	{
		public override string Texture => "Terraria/Images/Item_0";

		protected virtual int MinionType => default;
		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}


		public override void Behavior()
		{
			Projectile.friendly = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.position = player.Center;
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[MinionType] == 0)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(Projectile.type)[0].whoAmI == Projectile.whoAmI)
				{
					Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), player.Top, Vector2.Zero, MinionType, Projectile.damage, Projectile.knockBack, Main.myPlayer);
				}
			} else
			{
				// do this to prevent NPC projectile reflections from insta-killing the player
				Projectile.damage = 0;
				Projectile.hostile = false;
			}
		}
		public override Vector2? FindTarget()
		{
			// no op
			return null;
		}

		public override Vector2 IdleBehavior()
		{
			// no op
			return Vector2.Zero;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// no op
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// no op
		}

	}

	public abstract class EmpoweredMinion : SimpleMinion
	{
		protected abstract int ComputeDamage();
		protected abstract float ComputeSearchDistance();
		protected abstract float ComputeInertia();
		protected abstract float ComputeTargetedSpeed();
		protected abstract float ComputeIdleSpeed();

		protected int baseDamage = -1;
		protected int previousEmpowerCount = 0;
		protected virtual int dustType => DustID.Confetti;
		protected virtual int dustCount => 3;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// the empowered minion is technically a sub-minion if its counter minion, not a main minion
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minion = false;
			Projectile.minionSlots = 0;
		}

		protected abstract void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame);

		protected virtual int CounterType => default;
		protected int EmpowerCount
		{
			get => player == null ? 0 : player.ownedProjectileCounts[CounterType];
		}

		public virtual void OnEmpower()
		{
			// little visual effect on empower
			for (int i = 0; i < dustCount; i++)
			{
				Dust.NewDust(Projectile.Center, 16, 16, dustType);
			}
		}

		public override Vector2 IdleBehavior()
		{
			if (baseDamage == -1)
			{
				baseDamage = Projectile.damage;
			}
			if (EmpowerCount > previousEmpowerCount)
			{
				OnEmpower();
				previousEmpowerCount = EmpowerCount;
			}
			Projectile.damage = ComputeDamage();
			return Vector2.Zero;
		}

		public override Vector2? FindTarget()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}
		}


		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = ComputeInertia();
			float speed = ComputeTargetedSpeed();
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}


		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// alway clamp to the idle position
			float inertia = ComputeInertia();
			float maxSpeed = ComputeIdleSpeed();
			Vector2 speedChange = vectorToIdlePosition - Projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + speedChange) / inertia;
		}


		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int max = 0;
			SetMinAndMaxFrames(ref minFrame, ref max);
			maxFrame = max;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= (maxFrame ?? Main.projFrames[Projectile.type]) ||
					Projectile.frame < minFrame)
				{
					Projectile.frame = minFrame;
				}
			}
		}
	}
}
