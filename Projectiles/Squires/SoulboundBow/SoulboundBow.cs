using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundBow
{
	public class SoulboundBowMinionBuff : MinionBuff
	{
		public SoulboundBowMinionBuff() : base(ProjectileType<SoulboundBowMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Soulbound Bow");
			Description.SetDefault("A soulbound bow will follow your orders!");
		}
	}

	public class SoulboundBowMinionItem : SquireMinionItem<SoulboundBowMinionBuff, SoulboundBowMinion>
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundBow";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Bow");
			Tooltip.SetDefault("Summons a squire\nAn enchanted bow will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 35;
			item.value = Item.sellPrice(0, 0, 50, 0);
			item.rare = ItemRarityID.LightRed;
			item.noUseGraphic = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.PearlwoodBow, 1);
			recipe.AddIngredient(ItemID.SoulofLight, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool CanUseItem(Player player)
		{
			var canUse = base.CanUseItem(player);
			item.noUseGraphic = true;
			return canUse;
		}
	}


	public class SoulboundArrow : ModProjectile
	{

        protected virtual Color LightColor => new Color(1f, 0f, 0.8f, 1f);
        public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.ranged = false; //Bandaid fix
            projectile.minion = true;
		}
        
		public override void AI()
		{
			base.AI();
            Lighting.AddLight(projectile.Center, Color.LightPink.ToVector3() * 0.5f);
		}
        
        //Maybe in the future turn them bright pink instead of yellow. Will need a custom debuff though.
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Ichor, 120);
        }
		
        public override void Kill(int timeLeft)
		{
			Main.PlaySound(SoundID.Item10, (int)projectile.position.X, (int)projectile.position.Y);
            // don't spawn an arrow on kill
            for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
                int dustCreated = Dust.NewDust(projectile.position, 1, 1, 255, velocity.X, velocity.Y, 50, default(Color), Scale: 1.4f);
                Main.dust[dustCreated].noGravity = true;
                Main.dust[dustCreated].velocity *= 0.8f;
			}
		}
	}

	public class SoulboundBowMinion : WeaponHoldingSquire<SoulboundBowMinionBuff>
	{
		protected override int AttackFrames => 25;
		protected override string WingTexturePath => null;
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundBow";

		protected override float IdleDistanceMulitplier => 3;
		public override string Texture => "Terraria/Item_0";

		protected override LegacySoundStyle attackSound => new LegacySoundStyle(2, 5);
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override bool travelRangeCanBeModified => false;

		protected override float projectileVelocity => 18;
		public SoulboundBowMinion() : base(ItemType<SoulboundBowMinionItem>()) { }

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
			projectile.tileCollide = false;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(projectile.Center, Color.LightPink.ToVector3() * 0.5f);
			return base.IdleBehavior();
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (!usingWeapon && attackFrame == 0)
			{
				weaponAngle = projectile.velocity.X * -projectile.spriteDirection * 0.05f;
				Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
				DrawWeapon(spriteBatch, translucentColor);
			}
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{

			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			base.PostDraw(spriteBatch, translucentColor);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= ModifiedProjectileVelocity();
					Projectile.NewProjectile(
						projectile.Center,
						angleVector,
						ProjectileType<SoulboundArrow>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
				Main.PlaySound(SoundID.Item39, projectile.Center);
			}
		}

		protected override float WeaponDistanceFromCenter() => 1;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 48;
	}
}
