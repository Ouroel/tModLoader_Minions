using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.TitaniumSquire
{
	public class TitaniumSquireMinionBuff : MinionBuff
	{
		public TitaniumSquireMinionBuff() : base(ProjectileType<TitaniumSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Titanium Squire");
			Description.SetDefault("A titanium squire will follow your orders!");
		}
	}

	public class TitaniumSquireMinionItem : SquireMinionItem<TitaniumSquireMinionBuff, TitaniumSquireMinion>
	{
		protected override string SpecialName => "Titanium Drone";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Titanium Crest");
			Tooltip.SetDefault("Summons a squire\nA titanium squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 8f;
			item.width = 24;
			item.height = 38;
			item.damage = 51;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.TitaniumBar, 14);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class TitaniumDroneDamageHitbox : ModProjectile
	{
		public override string Texture => "Terraria/Item_0";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 1;
			projectile.tileCollide = false;
			projectile.friendly = true;
		}
	}

	public class TitaniumSquireDrone : SquireAccessoryMinion
	{
		protected override bool IsEquipped(SquireModPlayer player) => player.HasSquire() && 
			player.GetSquire().type == ProjectileType<TitaniumSquireMinion>();
		private static int AnimationFrames = 80;

		private int attackRate => (int)Math.Max(15f, 30f / player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 18;
			projectile.height = 18;
			frameSpeed = 10;
		}

		public override Vector2 IdleBehavior()
		{
			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			float radius = 36;
			Vector2 angleVector = radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			SquireModPlayer modPlayer = player.GetModPlayer<SquireModPlayer>();
			if(modPlayer.HasSquire())
			{
				projectile.spriteDirection = modPlayer.GetSquire().spriteDirection;
			}
			// offset downward vertically a bit
			// the scale messes with the positioning in some way
			return base.IdleBehavior() + angleVector + new Vector2(0, 8);
		}
		public override Vector2? FindTarget()
		{
			if (animationFrame % attackRate == 0 && SquireAttacking() &&
				SelectedEnemyInRange(180, maxRangeFromPlayer: false) is Vector2 target)
			{
				return target - projectile.Center;
			}
			return null;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			for(int i = 0; i < 3; i++)
			{
				int dustId = Dust.NewDust(projectile.position, 20, 20, 160);
				ColorDust(dustId);
			}
		}

		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 3; i++)
			{
				int dustId = Dust.NewDust(projectile.position, 20, 20, 160);
				ColorDust(dustId);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.IdleMovement(vectorToIdle);
			if (animationFrame % attackRate == 0 )
			{
				if(player.whoAmI == Main.myPlayer)
				{
					Projectile.NewProjectile(
						projectile.Center + vectorToTargetPosition,
						Vector2.Zero,
						ProjectileType<TitaniumDroneDamageHitbox>(),
						projectile.damage,
						0,
						player.whoAmI);
				}
				Vector2 targetVector = vectorToTargetPosition;
				Vector2 stepVector = targetVector;
				stepVector.Normalize();

				for(int i = 12; i < targetVector.Length(); i++)
				{
					Vector2 posVector = projectile.Center + stepVector * i;
					int dustId = Dust.NewDust(posVector, 1, 1, 160);
					ColorDust(dustId);
					Main.dust[dustId].scale = Main.rand.NextFloat(0.9f, 1.3f);
					Main.dust[dustId].velocity *= 0.2f;
				}
				Main.PlaySound(new LegacySoundStyle(2, 92), projectile.Center);
			}
		}

		private void ColorDust(int dustId)
		{
			int dustColorIdx = Main.rand.Next(4);
			if (dustColorIdx == 0)
			{
				Main.dust[dustId].color = Color.LimeGreen;
			}
			else if (dustColorIdx == 1)
			{
				Main.dust[dustId].color = Color.Purple;
			} else
			{
				Main.dust[dustId].color = Color.LightSteelBlue;
			}
		}
	}

	public class TitaniumSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<TitaniumSquireMinionBuff>();
		protected override int AttackFrames => 38;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/TitaniumSquire/TitaniumSquireSpear";

		protected override Vector2 WingOffset => new Vector2(-6, 6);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;
		public TitaniumSquireMinion() : base(ItemType<TitaniumSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Titanium Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 32;
		}

		protected override float WeaponDistanceFromCenter()
		{
			//All of this is based on the weapon sprite and AttackFrames above.
			int reachFrames = AttackFrames / 2; //A spear should spend half the AttackFrames extending, and half retracting by default.
			int spearLength = GetTexture(WeaponTexturePath).Width; //A decent aproximation of how long the spear is.
			int spearStart = (spearLength / 3); //Two thirds of the spear starts behind by default.
			float spearSpeed = spearLength / reachFrames; //A calculation of how quick the spear should be moving.
			if (attackFrame <= reachFrames)
			{
				return spearSpeed * attackFrame - spearStart;
			}
			else
			{
				return (spearSpeed * reachFrames - spearStart) - spearSpeed * (attackFrame - reachFrames);
			}
		}

		public override void OnStartUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(
					projectile.Center,
					Vector2.Zero,
					ProjectileType<TitaniumSquireDrone>(),
					projectile.damage,
					0,
					player.whoAmI);
			}
		}

		public override void OnStopUsingSpecial()
		{
			int projType = ProjectileType<TitaniumSquireDrone>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.owner == player.whoAmI && p.type == projType)
				{
					p.Kill();
					break;
				}
			}
		}

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 35;

		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 45;

		public override float MaxDistanceFromPlayer() => 290;

		public override float ComputeTargetedSpeed() => 11;

		public override float ComputeIdleSpeed() => 11;
	}
}
