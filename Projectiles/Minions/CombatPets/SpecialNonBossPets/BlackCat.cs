﻿using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using AmuletOfManyMinions.Core.Minions.Effects;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using AmuletOfManyMinions.Core.BackportUtils;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BlackCatMinionBuff : CombatPetVanillaCloneBuff
	{
		public BlackCatMinionBuff() : base(ProjectileType<BlackCatMinion>()) { }
		public override int VanillaBuffId => BuffID.BlackCat;
		public override string VanillaBuffName => "BlackCat";
	}

	public class BlackCatMinionItem : CombatPetMinionItem<BlackCatMinionBuff, BlackCatMinion>
	{
		internal override int VanillaItemID => ItemID.UnluckyYarn;
		internal override string VanillaItemName => "UnluckyYarn";
		internal override int AttackPatternUpdateTier => 6;
	}

	public abstract class BlackCatRicochetProjectile : BackportModProjectile
	{

		int bouncesLeft;
		public override string Texture => "Terraria/Projectile_" + 0;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = 180;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.penetrate = 10;
			Projectile.tileCollide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			bouncesLeft = 6;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Math.Abs(Projectile.velocity.Y) < Math.Abs(oldVelocity.Y))
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			else if (Math.Abs(Projectile.velocity.X) < Math.Abs(oldVelocity.X))
			{
				Projectile.velocity.X = -oldVelocity.X;
			}
			else
			{
				// don't really understand what's going on in this case but that's ok
				return false;
			}
			return !(bouncesLeft-- > 0);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.damage = (int)(Projectile.damage * 0.95f);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}
	}

	public class BlackCatWaterBolt : BlackCatRicochetProjectile
	{
		public override void AI()
		{
			for (int i = 0; i < 5; i++)
			{
				Vector2 dustOffset = i * Projectile.velocity / 3f;
				int dustId = Dust.NewDust(Projectile.position, Projectile.width / 2, Projectile.height / 2, 172, Scale: 1.2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.1f;
				Main.dust[dustId].position -= dustOffset;
			}
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 30; i++)
			{
				int dustId = Dust.NewDust(
					Projectile.position, Projectile.width, Projectile.height,
					172, Projectile.velocity.X / 10, Projectile.velocity.Y / 10, 100);
				Main.dust[dustId].noGravity = true;
			}
		}
	}

	public class BlackCatMeowsicalNote : BlackCatRicochetProjectile
	{
		int textureType = 0;

		public override void AI()
		{
			if (textureType == 0)
			{
				textureType = new int[] { ProjectileID.QuarterNote, ProjectileID.EighthNote, ProjectileID.TiedEighthNote }[Main.rand.Next(3)];
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;
			Projectile.spriteDirection = -Math.Sign(Projectile.velocity.X);
			if (Main.rand.Next(3) == 0)
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 27, Alpha: 80);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.2f;
			}
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 27, Alpha: 80);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.4f;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Color translucentColor = Color.White * 0.75f;
			Texture2D texture = Main.projectileTexture[textureType];
			spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition,
				texture.Bounds, translucentColor, Projectile.rotation, texture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}
	}

	public class BlackCatShadowBeam : BlackCatRicochetProjectile
	{

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.extraUpdates = 30;
		}
		public override void AI()
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 dustPos = Projectile.position;
				dustPos -= Projectile.velocity * i / 4f;
				int dustId = Dust.NewDust(dustPos, 1, 1, 173);
				Main.dust[dustId].scale = Main.rand.NextFloat(0.75f, 1.15f);
				Main.dust[dustId].position = dustPos;
				Main.dust[dustId].velocity *= 0.2f;
			}
		}
	}


	class CatPetLevelInfo
	{
		internal int MinLevel;
		internal int ItemId;
		internal int ProjectileId;
		internal int ProjectileVelocity;
		internal WeaponSpriteOrientation Orientation;


		public CatPetLevelInfo(int minLevel, int itemId, int projId, int projVelocity, WeaponSpriteOrientation orientation = WeaponSpriteOrientation.VERTICAL)
		{
			MinLevel = minLevel;
			ItemId = itemId;
			ProjectileId = projId;
			ProjectileVelocity = projVelocity;
			Orientation = orientation;
		}
	}

	public abstract class WeaponHoldingCatMinion : CombatPetGroundedRangedMinion
	{
		internal override int? ProjId => levelInfo?.ProjectileId ?? 0;

		internal WeaponHoldingDrawer weaponDrawer;
		internal CatPetLevelInfo levelInfo;
		internal abstract CatPetLevelInfo[] CatPetLevels { get; }

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			SetAttackStyleSpecificBehaviors();
			weaponDrawer.Update(Projectile, animationFrame);
			return target;
		}

		private void SetAttackStyleSpecificBehaviors()
		{
			for (int i = CatPetLevels.Length - 1; i >= 0; i--)
			{
				CatPetLevelInfo levelInfo = CatPetLevels[i];
				if (levelInfo.MinLevel <= leveledPetPlayer.PetLevel)
				{
					this.levelInfo = levelInfo;
					break;
				}
			}
			launchVelocity = levelInfo.ProjectileVelocity;
			weaponDrawer.spriteOrientation = levelInfo.Orientation;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			weaponDrawer = new WeaponHoldingDrawer()
			{
				WeaponOffset = Vector2.Zero,
				WeaponHoldDistance = 16,
				ForwardDir = -1,
				yOffsetScale = 0.5f
			};
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (animationFrame == lastFiredFrame)
			{
				weaponDrawer.StartAttack(vectorToTargetPosition);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (!(levelInfo?.ItemId is int itemId) || itemId == 0)
			{
				return true;
			}
			Texture2D texture = Main.itemTexture[itemId];
			weaponDrawer.Draw(spriteBatch, texture, lightColor);
			return true;
		}
	}

	public class BlackCatMinion : WeaponHoldingCatMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BlackCat;
		internal override int BuffId => BuffType<BlackCatMinionBuff>();

		// scale attack type rather than attack speed
		internal override int GetAttackFrames(ICombatPetLevelInfo info) => Math.Max(45, 60 - 4 * info.Level);

		internal override int? ProjId => levelInfo?.ProjectileId ?? 0;

		private static CatPetLevelInfo[] BlackCatLevelInfo;
		internal override CatPetLevelInfo[] CatPetLevels => BlackCatLevelInfo;

		public override void SetStaticDefaults()
		{
			LoadAssets();
			base.SetStaticDefaults();
		}

		public static void LoadAssets()
		{
			//Main.instance.LoadItem(ItemID.WandofSparking);
			//Main.instance.LoadItem(ItemID.WaterBolt);
			//Main.instance.LoadItem(ItemID.MagicalHarp);
			//Main.instance.LoadItem(ItemID.ShadowbeamStaff);
			if(!Main.dedServ)
			{
				Main.instance.LoadProjectile(ProjectileID.QuarterNote);
				Main.instance.LoadProjectile(ProjectileID.EighthNote);
				Main.instance.LoadProjectile(ProjectileID.TiedEighthNote);
			}
			BlackCatLevelInfo = new CatPetLevelInfo[]
			{
				new CatPetLevelInfo(-1, ItemID.WandofSparking, 0, 8, WeaponSpriteOrientation.DIAGONAL),
				new CatPetLevelInfo(0, ItemID.WaterBolt, ProjectileType<BlackCatWaterBolt>(), 6),
				new CatPetLevelInfo(5, ItemID.MagicalHarp, ProjectileType<BlackCatMeowsicalNote>(), 8),
				// lots of extra updates
				new CatPetLevelInfo(6, ItemID.ShadowbeamStaff, ProjectileType<BlackCatShadowBeam>(), 6, WeaponSpriteOrientation.DIAGONAL),
			};
		}

		public static void Unload()
		{
			BlackCatLevelInfo = null;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 24, -20, -16, -1);
			ConfigureFrames(11, (0, 0), (1, 5), (1, 1), (6, 10));
		}
	}
}
