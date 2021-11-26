﻿using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class SharkPupMinionBuff : CombatPetVanillaCloneBuff
	{
		public SharkPupMinionBuff() : base(ProjectileType<SharkPupMinion>()) { }
		public override string VanillaBuffName => "SharkPup";
		public override int VanillaBuffId => BuffID.SharkPup;
	}

	public class SharkPupMinionItem : CombatPetMinionItem<SharkPupMinionBuff, SharkPupMinion>
	{
		internal override string VanillaItemName => "SharkBait";
		internal override int VanillaItemID => ItemID.SharkBait;
	}

	public class SharkPupBubble : BaseMinionBubble
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}
	}

	public class SharkPupMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<SharkPupMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SharkPup;
		internal override int? FiredProjectileId => ProjectileType<SharkPupBubble>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;

		internal override int GetProjectileVelocity(CombatPetLevelInfo info) => Math.Min(8, 4 + info.Level);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(4, 8);
		}

	}
}
