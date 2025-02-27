﻿using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class ParrotPetMinionBuff : CombatPetVanillaCloneBuff
	{
		public ParrotPetMinionBuff() : base(ProjectileType<ParrotPetMinion>()) { }
		public override string VanillaBuffName => "PetParrot";
		public override int VanillaBuffId => BuffID.PetParrot;
	}

	public class ParrotPetMinionItem : CombatPetMinionItem<ParrotPetMinionBuff, ParrotPetMinion>
	{
		internal override string VanillaItemName => "ParrotCracker";
		internal override int VanillaItemID => ItemID.ParrotCracker;
	}

	public class ParrotPetMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<ParrotPetMinionBuff>();
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Parrot;
		internal override int? FiredProjectileId => null;
		internal override bool DoBumblingMovement => true;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 5;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(1, 5);
		}

	}
}
