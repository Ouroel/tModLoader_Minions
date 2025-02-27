﻿using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Necromancer
{
	public class NecromancerMinionBuff : MinionBuff
	{
		public NecromancerMinionBuff() : base(ProjectileType<NecromancerMinion>(), ProjectileType<NecromancerSkeletonMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Necromancer");
			Description.SetDefault("A necromancer and his skeletal servants will fight for you!");
		}
	}

	public class NecromancerMinionItem : MinionItem<NecromancerMinionBuff, NecromancerSkeletonMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Necro Doll");
			Tooltip.SetDefault("Summons a neromancer to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.damage = 60;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.Yellow;
			item.useStyle = ItemUseStyleID.HoldingUp;
		}
	}

	public class BoneSphereBoneProjectile : ModProjectile
	{

		public override string Texture => "Terraria/Projectile_" + ProjectileID.Bone;
		public override void SetStaticDefaults()
		{
			// this is a bit sneaky, doesn't set any of the SimpleMinion defaults
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Bone);
			projectile.penetrate = 1;
			projectile.ranged = false;
			projectile.minion = true;
		}
	}

	public class BoneSphereProjectile : SimpleMinion
	{
		internal override int BuffId => -1;
		static int TimeToLive = 240;
		private Vector2 velocity = default;
		private int lastShotFrame = 0;
		public override void SetStaticDefaults()
		{
			// this is a bit sneaky, doesn't set any of the SimpleMinion defaults
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}
		public override void SetDefaults()
		{
			// this is a bit sneaky, doesn't set any of the SimpleMinion defaults
			projectile.width = 30;
			projectile.height = 30;
			projectile.friendly = true;
			projectile.penetrate = 15;
			projectile.tileCollide = false;
			projectile.timeLeft = TimeToLive;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 60;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];
			Rectangle bounds = texture.Bounds;
			Vector2 origin = bounds.Center.ToVector2();
			int timeElapsed = TimeToLive - projectile.timeLeft;
			float scale = timeElapsed > 20 ? 1 : timeElapsed / 20f;
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, lightColor, projectile.rotation,
				origin, scale, 0, 0);
			return false;
		}
		public override void AI()
		{
			if (velocity == default)
			{
				velocity = projectile.velocity;
			}
			player = Main.player[projectile.owner];
			projectile.rotation += 0.05f;
			int timeElapsed = TimeToLive - projectile.timeLeft;
			if (timeElapsed % 2 == 0)
			{
				int dustIdx = Dust.NewDust(projectile.position, projectile.width, projectile.height, 203);
				Main.dust[dustIdx].velocity = Vector2.Zero;
				Main.dust[dustIdx].noGravity = true;
			}
			if (timeElapsed < 30)
			{
				Projectile parent = GetMinionsOfType(ProjectileType<NecromancerMinion>()).FirstOrDefault();
				if (parent != default)
				{
					projectile.velocity = Vector2.Zero;
					projectile.Center = parent.Top + new Vector2(0, -8);
				}
			}
			else
			{
				projectile.tileCollide = true;
				if (projectile.velocity == Vector2.Zero)
				{
					projectile.velocity = velocity;
				}
				if (projectile.owner == Main.myPlayer && timeElapsed - lastShotFrame > 20 && AnyEnemyInRange(400f) is Vector2 target)
				{
					lastShotFrame = timeElapsed;
					Vector2 vectorToTarget = target - projectile.Center;
					vectorToTarget.Normalize();
					vectorToTarget *= 12;
					vectorToTarget += Main.npc[(int)targetNPCIndex].velocity;
					Projectile.NewProjectile(
						projectile.Center,
						vectorToTarget,
						ProjectileType<BoneSphereBoneProjectile>(),
						3 * projectile.damage / 4,
						projectile.knockBack,
						projectile.owner);
				}
			}
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			for (int i = 0; i < 5; i++)
			{
				Dust.NewDust(projectile.position, projectile.width, projectile.height, 203);
			}
			for (int i = 0; i < 4; i++)
			{
				Vector2 angle = Main.rand.NextFloat(MathHelper.Pi).ToRotationVector2();
				angle *= 8;
				Projectile.NewProjectile(
					projectile.Center,
					angle,
					ProjectileType<BoneSphereBoneProjectile>(),
					3 * projectile.damage / 4,
					projectile.knockBack,
					projectile.owner);
			}
		}

		public override Vector2 IdleBehavior()
		{
			// no op
			return default;
		}

		public override Vector2? FindTarget()
		{
			// no op
			return default;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// no op
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// no op
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return true;
		}
	}

	public class NecromancerMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<NecromancerMinionBuff>();
		protected override int CounterType => ProjectileType<NecromancerSkeletonMinion>();

		private int framesSinceLastHit;
		protected override int dustType => DustID.Shadowflame;
		int projId;

		int rateOfFire => Math.Max(90, 125 - 5 * EmpowerCount);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Necromancer");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 8;
			IdleLocationSets.trailingInAir.Add(projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 36;
			projectile.height = 46;
			projectile.tileCollide = false;
			framesSinceLastHit = 0;
			projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
			frameSpeed = 15;
		}


		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, projectile);
			idlePosition.Y += -24 + 8 * (float)Math.Sin(MathHelper.TwoPi * (animationFrame % 120) / 120);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			if (vectorToTarget is null)
			{
				framesSinceLastHit = rateOfFire;
			}
			Lighting.AddLight(projectile.position, Color.White.ToVector3() * 0.5f);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(vectorToIdle);
			framesSinceLastHit++;
			int projectileVelocity = 6 + EmpowerCount / 2;
			if (framesSinceLastHit++ > rateOfFire)
			{
				// try to predict the position at the time of impact a bit
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= projectileVelocity;
				Vector2 pos = projectile.Center;
				framesSinceLastHit = 0;
				projectile.spriteDirection = vectorToTargetPosition.X > 0 ? 1 : -1;
				Main.PlaySound(new LegacySoundStyle(2, 8), projectile.position);
				if (Main.myPlayer == player.whoAmI)
				{
					projId = Projectile.NewProjectile(
						pos,
						VaryLaunchVelocity(vectorToTargetPosition),
						ProjectileType<BoneSphereProjectile>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
			}
			else if (Main.myPlayer == player.whoAmI && framesSinceLastHit == 30 && Main.projectile[projId].active)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= projectileVelocity;
				Main.projectile[projId].velocity = vectorToTargetPosition;
			}
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 12) * EmpowerCount; // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}
		public override Vector2? FindTarget()
		{
			Vector2? target = GetTargetVector();
			return target;
		}

		protected override float ComputeSearchDistance()
		{
			return 800 + 20 * EmpowerCount;
		}

		protected override float ComputeInertia() => 5;

		protected override float ComputeTargetedSpeed() => 16;

		protected override float ComputeIdleSpeed() => 16;

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 4;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (vectorToTarget != null && framesSinceLastHit < 75)
			{
				projectile.frame = 5;
			}
			else
			{
				base.Animate(minFrame, maxFrame);
			}
			if (Math.Abs(projectile.velocity.X) > 2 && vectorToTarget is null)
			{
				projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;
			}
		}
		public override void CheckActive()
		{
			base.CheckActive();
			if(player.ownedProjectileCounts[CounterType] == 0 && animationFrame > 2)
			{
				projectile.Kill();
			}
		}
	}

	public class NecromancerSkeletonMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<NecromancerMinionBuff>();
		const int explosionRespawnTime = 60;
		const int explosionRadius = 96;
		const int explosionAttackRechargeTime = 96;
		int lastExplosionFrame = 0;
		private Vector2 explosionLocation;
		private bool isDropping = true;

		private bool canAttack => animationFrame - lastExplosionFrame >= explosionAttackRechargeTime;
		private bool isRespawning => animationFrame - lastExplosionFrame < explosionRespawnTime;

		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (4, 4),
			[GroundAnimationState.JUMPING] = (4, 4),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (0, 4),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Necromancer Skeleton");
			Main.projFrames[projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 28;
			projectile.height = 30;
			drawOriginOffsetY = -10;
			drawOriginOffsetX = -1;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
			searchDistance = 900;
			maxSpeed = 14;
			idleInertia = 6;
			projectile.timeLeft = 3;
		}

		public override Vector2 IdleBehavior()
		{
			isDropping &= !canAttack;
			return base.IdleBehavior();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			isDropping = false;
			return base.OnTileCollide(oldVelocity);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// use a rectangular hitbox for the explosion. Easier than the alternative
			projHitbox = new Rectangle(
				(int)explosionLocation.X - explosionRadius,
				(int)explosionLocation.Y - explosionRadius,
				2 * explosionRadius,
				2 * explosionRadius);
			if (Vector2.DistanceSquared(explosionLocation, targetHitbox.Center.ToVector2()) < explosionRadius * explosionRadius)
			{
				return true;
			}
			return projHitbox.Intersects(targetHitbox);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (Main.rand.Next(2) == 0)
			{
				target.AddBuff(BuffID.ShadowFlame, 180);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (!DoInactiveMovement())
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		private bool DoInactiveMovement()
		{
			if (isRespawning)
			{
				Projectile summoner = GetMinionsOfType(ProjectileType<NecromancerMinion>()).FirstOrDefault();

				// clamp to the summoner while respawning
				if (summoner != default)
				{
					projectile.Top = summoner.Center;
					projectile.velocity = summoner.velocity;
				}
				else
				{
					projectile.position = player.position;
					projectile.velocity = player.velocity;
				}
				return true;
			}
			else if (isDropping)
			{
				if (projectile.velocity.Y < 16)
				{
					projectile.velocity.Y += 0.6f;
				}
				projectile.tileCollide = true;
				return true;
			}
			return false;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 3f : 8;
			float xMaxSpeed = 14f;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (DoInactiveMovement() || isDropping)
			{
				return;
			}
			else if (vectorToTargetPosition.Length() < explosionRadius / 2 && !usingBeacon)
			{
				lastExplosionFrame = animationFrame;
				explosionLocation = projectile.Center;
				isDropping = true;
				Main.PlaySound(SoundID.Item62, projectile.Center);
				DoExplosionEffects();
			}
			else
			{
				base.TargetedMovement(canAttack ? vectorToTargetPosition : vectorToIdle);
			}
		}

		public override Vector2? FindTarget()
		{

			return canAttack ? base.FindTarget() : null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return !isRespawning;
		}

		private void DoExplosionEffects()
		{
			Vector2 position = projectile.position;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 30; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 31, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 1.4f;
			}
			for (int i = 0; i < 20; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 3f;
			}
			for (float goreVel = 0.4f; goreVel < 0.8f; goreVel += 0.4f)
			{
				foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
				{
					int goreIdx = Gore.NewGore(position, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
				}
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (isDropping)
			{
				projectile.rotation = 0;
				projectile.frame = 4;
				return;
			}
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if (state == GroundAnimationState.FLYING && animationFrame % 3 == 0)
			{
				int idx = Dust.NewDust(projectile.Bottom, 8, 8, 16, -projectile.velocity.X / 2, -projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			}
			return;
		}

		public override void AfterMoving()
		{
			projectile.friendly = isRespawning && animationFrame - lastExplosionFrame <= 15;
			// Lifted from EmpoweredMinion.cs
			int minionType = ProjectileType<NecromancerMinion>();
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[minionType] == 0)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(projectile.type)[0].whoAmI == projectile.whoAmI)
				{
					Projectile.NewProjectile(player.Top, Vector2.Zero, minionType, projectile.damage, projectile.knockBack, Main.myPlayer);
				}
			}
		}
	}
}
