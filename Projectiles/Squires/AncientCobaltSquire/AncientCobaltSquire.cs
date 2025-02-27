using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire
{
	public class AncientCobaltSquireMinionBuff : MinionBuff
	{
		public AncientCobaltSquireMinionBuff() : base(ProjectileType<AncientCobaltSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			Description.SetDefault("An ancient cobalt squire will follow your orders!");
		}
	}

	public class AncientCobaltSquireMinionItem : SquireMinionItem<AncientCobaltSquireMinionBuff, AncientCobaltSquireMinion>
	{
		protected override string SpecialName => "Magic Shotblast";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Crest of Cobalt");
			Tooltip.SetDefault("Summons a squire\nAn ancient cobalt squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 10;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Orange;
		}
	}

	public class AncientCobaltBolt : ModProjectile
	{

		public override string Texture => "Terraria/Projectile_" + ProjectileID.SapphireBolt;

		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.SapphireBolt);
			projectile.minion = true;
			projectile.timeLeft = 30;
		}

		public override void AI()
		{
			base.AI();
			for (int i = 0; i < 2; i++)
			{
				int dustSpawned = Dust.NewDust(projectile.position, projectile.width, projectile.height, 88, projectile.velocity.X, projectile.velocity.Y, 50, default, 1.2f);
				Main.dust[dustSpawned].noGravity = true;
				Main.dust[dustSpawned].velocity *= 0.3f;
			}
			if (projectile.localAI[0] == 0f)
			{
				projectile.localAI[0] = 1f;
				Main.PlaySound(SoundID.Item8, projectile.Center);
			}
		}
		public override void Kill(int timeLeft)
		{
			Main.PlaySound(SoundID.Dig, (int)projectile.position.X, (int)projectile.position.Y);
			for (int i = 0; i < 15; i++)
			{
				int dustCreated = Dust.NewDust(projectile.position, projectile.width, projectile.height, 88, projectile.oldVelocity.X, projectile.oldVelocity.Y, 50, default(Color), 1.2f);
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].scale *= 1.25f;
				Main.dust[dustCreated].velocity *= 0.5f;
			}
		}
	}


	public class AncientCobaltStream : ModProjectile
	{

		public override string Texture => "Terraria/Projectile_" + ProjectileID.WaterStream;

		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.WaterStream);
			projectile.magic = false; //Bandaid fix
		}
	}

	public class AncientCobaltSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<AncientCobaltSquireMinionBuff>();
		protected override int AttackFrames => 8;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/AncientCobaltSquire/AncientCobaltStaff";

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.DIAGONAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override LegacySoundStyle attackSound => new LegacySoundStyle(2, 13);

		protected override LegacySoundStyle SpecialStartSound => new LegacySoundStyle(2, 28);
		protected override float projectileVelocity => 8;

		protected override bool travelRangeCanBeModified => false;

		protected override int SpecialDuration => 60;

		private float weaponAngleOverride = -1;

		public AncientCobaltSquireMinion() : base(ItemType<AncientCobaltSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 32;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}


		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				angleVector *= ModifiedProjectileVelocity();
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<AncientCobaltStream>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			// special frame is 1-indexed because it's a bug and I can't be bothered to fix it
			if(specialFrame % 5 == 1 && specialFrame <= 46 && Main.myPlayer == player.whoAmI)
			{
				float angleOffset = Main.rand.NextFloat(MathHelper.Pi / 16) - MathHelper.Pi / 32;
				Vector2 angleVector = UnitVectorFromWeaponAngle().RotatedBy(angleOffset);
				angleVector *= ModifiedProjectileVelocity() * 2;
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<AncientCobaltBolt>(),
						3 * projectile.damage / 2,
						projectile.knockBack,
						Main.myPlayer);
				}
				weaponAngle += 2 * angleOffset;
				weaponAngleOverride = weaponAngle;
			} else if (weaponAngleOverride != -1)
			{
				weaponAngle = weaponAngleOverride ;
			}
		}

		public override void OnStopUsingSpecial()
		{
			weaponAngleOverride = -1;
		}


		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 50;
	}
}
