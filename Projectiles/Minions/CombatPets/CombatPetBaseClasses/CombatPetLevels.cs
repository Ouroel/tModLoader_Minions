﻿using AmuletOfManyMinions.Core.BackportUtils;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetEmblems;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	internal enum CombatPetTier: int
	{
		Base = 0,
		Golden = 1,
		Demonite = 2,
		Skeletal = 3,
		Soulful = 4,
		Hallowed = 5,
		Spectre = 6,
		Stardust = 7,
		Celestial = 8
	}

	internal interface ICombatPetLevelInfo
	{
		int Level { get; }
		int BaseDamage { get; } // The base damage done by combat pets at this level of progression
		int BaseSearchRange { get; } // The base distance combat pets will seek from the player 
		float BaseSpeed { get; } // How the AI actually uses speed varies quite a bit from type to type ...
		int MaxPets { get; } // Maximum # of unique combat pets available
		string Description { get; }  // Used in combat pet item tooltips referring to level up points
	}
	internal struct CombatPetLevelInfo : ICombatPetLevelInfo
	{
		public int Level { get; private set;  }
		public int BaseDamage { get; private set; } // The base damage done by combat pets at this level of progression
		public int BaseSearchRange { get; private set; } // The base distance combat pets will seek from the player 
		public float BaseSpeed { get; private set; } // How the AI actually uses speed varies quite a bit from type to type ...
		public int MaxPets { get; private set; } // Maximum # of unique combat pets available
		public string Description { get; private set; }  // Used in combat pet item tooltips referring to level up points

		public CombatPetLevelInfo(int level, int damage, int searchRange, int baseSpeed, int maxPets, string description)
		{
			Level = level;
			BaseDamage = damage;
			BaseSearchRange = searchRange;
			BaseSpeed = baseSpeed;
			MaxPets = maxPets;
			Description = description;
		}
	}

	internal class PlayerCombatPetLevelInfo : ICombatPetLevelInfo
	{
		public int Level { get; private set; } = 0;
		public int BaseDamage => RawInfo.BaseDamage + Player.PetDamageBonus;
		public int BaseSearchRange => RawInfo.BaseSearchRange + Player.SearchRangeBonus;
		public float BaseSpeed => RawInfo.BaseSpeed + Player.PetSpeedBonus;
		public int MaxPets => RawInfo.MaxPets;
		public string Description => RawInfo.Description;

		private readonly LeveledCombatPetModPlayer Player;
		private ICombatPetLevelInfo RawInfo => CombatPetLevelTable.PetLevelTable[Level];

		public PlayerCombatPetLevelInfo(LeveledCombatPetModPlayer player)
		{
			Player = player;
		}

		public PlayerCombatPetLevelInfo WithLevel(int level)
		{
			Level = level;
			return this;
		}
	}
	class CombatPetLevelTable
	{
		internal static ICombatPetLevelInfo[] PetLevelTable;

		public static void Load()
		{
			PetLevelTable = new ICombatPetLevelInfo[]{
				new CombatPetLevelInfo(0, 7, 550, 8, 1, "Base"), // Base level
				new CombatPetLevelInfo(1, 11, 600, 8, 1, "Golden"), // ore tier
				new CombatPetLevelInfo(2, 15, 700, 9, 1, "Demonite"), // EoC - tier
				new CombatPetLevelInfo(3, 18, 750, 10, 2, "Skeletal"), // Dungeon Tier
				new CombatPetLevelInfo(4, 30, 900, 12, 2, "Soulful"), // Post WoF
				new CombatPetLevelInfo(5, 36, 950, 14, 2, "Hallowed"), // Post Mech
				new CombatPetLevelInfo(6, 42, 1000, 15, 3, "Spectre"), // Post Plantera
				new CombatPetLevelInfo(7, 52, 1050, 16, 4, "Stardust"), // Post Pillars
				new CombatPetLevelInfo(8, 80, 1100, 18, 6, "Celestial") // Post Moon Lord
			};
		}

		public static void Unload()
		{
		}
	}

	class LeveledCombatPetModPlayer : BackportModPlayer
	{
		internal int PetLevel { get; set; }
		internal int PetDamage { get; set; }

		// todo this may be too many constructors, but it's a struct so I think it's ok
		private PlayerCombatPetLevelInfo CustomInfo;
		internal ICombatPetLevelInfo PetLevelInfo => CustomInfo.WithLevel(PetLevel);

		public int PetDamageBonus { get; internal set; }
		public int SearchRangeBonus { get; internal set; }
		public int PetSpeedBonus { get; internal set; }

		public int PetSlotsUsed { get; internal set; }

		public int ExtraPetSlots { get; internal set; } = 0;

		internal int AvailablePetSlots { get; private set; }

		private readonly List<int> BuffFlagsToReset = new List<int>();
		private int buffResetCountdown;

		public void UpdatePetLevel(int newLevel, int newDamage, bool fromSync = false)
		{
			bool didUpdate = newLevel != PetLevel || PetDamage != newDamage;
			PetLevel = newLevel;
			PetDamage = newDamage;
			if(didUpdate && !fromSync)
			{
				// TODO MP packet
				new CombatPetLevelPacket(Player, (byte)PetLevel, (short)PetDamage).Send();
			}
		}

		public override void PreUpdate()
		{
			CustomInfo = CustomInfo ?? new PlayerCombatPetLevelInfo(this);
			PetDamageBonus = 0;
			PetSpeedBonus = 0;
			SearchRangeBonus = 0;
			ExtraPetSlots = 0;
		}

		public override void PostUpdate()
		{
			CheckForCombatPetEmblem();
			UpdateCombatPetCount();
			ReflagPetBuffs();
		}

		private void UpdateCombatPetCount()
		{
			AvailablePetSlots = Math.Min(Player.maxMinions, PetLevelInfo.MaxPets) + ExtraPetSlots;
			PetSlotsUsed = 0;
			int buffCount = Player.CountBuffs();
			for(int i = 0; i < buffCount; i++)
			{
				if(CombatPetBuff.CombatPetBuffTypes.Contains(Player.buffType[i])) 
				{
					if(PetSlotsUsed < AvailablePetSlots)
					{
						PetSlotsUsed += 1;
					} else
					{
						Player.ClearBuff(Player.buffType[i]);
					}
				}
			}
			// extra pet slots don't count against minions
			int minionSlotsUsed = Math.Max(0, PetSlotsUsed - ExtraPetSlots);
			if(minionSlotsUsed > 0 && !ServerConfig.Instance.CombatPetsMinionSlots)
			{
				minionSlotsUsed -= 1;
			}
			Player.maxMinions = Math.Max(0, Player.maxMinions - minionSlotsUsed);
		}

		// look for the best Combat Pet Emblem in the player's inventory, use that
		// to set the player's combat pet's damage
		private void CheckForCombatPetEmblem()
		{
			// don't run every frame
			if(Main.GameUpdateCount % 10 != 0)
			{
				return;
			}
			int maxLevel = 0;
			int maxDamage = CombatPetLevelTable.PetLevelTable[0].BaseDamage;
			for(int i = 0; i < Player.inventory.Length; i++)
			{
				Item item = Player.inventory[i];
				if(!item.IsAir && item.modItem != null && item.modItem is CombatPetEmblem petEmblem)
				{
					// choose max tier rather than max damage
					if(petEmblem.PetLevel > maxLevel)
					{
						maxLevel = petEmblem.PetLevel;
						maxDamage = item.damage;
					}
				}
			}
			for(int i = 0; i < Player.bank.item.Length; i++)
			{
				Item item = Player.bank.item[i];
				if(!item.IsAir && item.modItem != null && item.modItem is CombatPetEmblem petEmblem)
				{
					// choose max tier rather than max damage
					if(petEmblem.PetLevel > maxLevel)
					{
						maxLevel = petEmblem.PetLevel;
						maxDamage = item.damage;
					}
				}
			}
			UpdatePetLevel(maxLevel, maxDamage);
		}

		private void ReflagPetBuffs()
		{
			if(buffResetCountdown -- == 0)
			{
				for(int i = 0; i < BuffFlagsToReset.Count; i++)
				{
					int buffId = BuffFlagsToReset[i];
					Main.vanityPet[buffId] = true;
				}
				BuffFlagsToReset.Clear();
			}
		}

		public void TemporarilyUnflagPetBuff(int buffId)
		{
			if(!ServerConfig.Instance.AllowMultipleCombatPets)
			{
				return;
			}

			if(PetSlotsUsed < AvailablePetSlots)
			{
				Main.vanityPet[buffId] = false;
				BuffFlagsToReset.Add(buffId);
				buffResetCountdown = 4;
			} 
			else if (PetSlotsUsed == AvailablePetSlots && AvailablePetSlots > 1)
			{
				// unmark all but the most recent buff, so that only one pet gets deleted
				int unmarkCount = 0;
				int buffCount = Player.CountBuffs();
				for(int i = 0; i < buffCount; i++)
				{
					buffId = Player.buffType[i];
					if(CombatPetBuff.CombatPetBuffTypes.Contains(buffId))
					{
						Main.vanityPet[buffId] = false;
						BuffFlagsToReset.Add(buffId);
						buffResetCountdown = 4;
						unmarkCount++;
						if(unmarkCount >= AvailablePetSlots - 1)
						{
							break;
						}
					}
				}
			}
		}
	}

	public abstract class CombatPetBuff : MinionBuff
	{

		internal static HashSet<int> CombatPetBuffTypes;

		public static void Load()
		{
			CombatPetBuffTypes = new HashSet<int>();
		}

		public static void Unload()
		{
			CombatPetBuffTypes = null;
		}

		public CombatPetBuff(params int[] projIds) : base(projIds) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.vanityPet[Type] = true;
			Main.buffNoSave[Type] = false;
			CombatPetBuffTypes.Add(Type);
		}

		public override void Update(Player player, ref int buffIndex)
		{
			for(int i = 0; i < projectileTypes.Length; i++)
			{
				int projType = projectileTypes[i];
				if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
				{
					var p = Projectile.NewProjectileDirect(player.Center, Vector2.Zero, projType, 0, 0, player.whoAmI);
					// p.originalDamage is updated in each frame by the minion itself
				}
			}
			if (projectileTypes.Select(p => player.ownedProjectileCounts[p]).Sum() > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.buffTime[buffIndex] = Math.Min(player.buffTime[buffIndex], 2);
			}
		}
	}

	public abstract class CombatPetVanillaCloneBuff : CombatPetBuff
	{
		public abstract int VanillaBuffId { get; }
		public abstract string VanillaBuffName { get; }

		public CombatPetVanillaCloneBuff(params int[] projIds) : base(projIds) { }

		public override string Texture => "Terraria/Buff_" + VanillaBuffId;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName." + VanillaBuffName) + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription." + VanillaBuffName));
		}

	}

	internal static class CombatPetItemUtils
	{
		public static void AddCombatPetTooltip(Mod mod,int attackPatternUpdateTier, List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(mod, "CombatPetDescription", 
				"This pet's fighting spirit has been awakened!\n" +
				"It can be powered up by holding a Combat Pet Emblem.")
			{
				overrideColor = Color.LimeGreen
			});


			LeveledCombatPetModPlayer player = Main.player[Main.myPlayer].GetModPlayer<LeveledCombatPetModPlayer>();
			if(attackPatternUpdateTier == 0)
			{
				return;
			} else if (attackPatternUpdateTier > player.PetLevel)
			{
				tooltips.Add(new TooltipLine(mod, "CombatPetNotLeveledUp", 
					"This pet will gain a stronger attack pattern if you hold a\n" +
					CombatPetLevelTable.PetLevelTable[attackPatternUpdateTier].Description + 
					" Combat Pet Emblem or stronger!")
				{
					overrideColor = Color.Gray
				});
			} else
			{
				tooltips.Add(new TooltipLine(mod, "CombatPetLeveledUp", 
					"Your emblem enables this pet's stronger attack pattern!")
				{
					overrideColor = Color.LimeGreen
				});
			}
		}

		// hack to temporarily un-flag buffs as pet type to prevent vanilla removal code from running
		// depending on how many open combat pet slots the player has
		public static bool CanUseItem(Player player, Item item)
		{
			LeveledCombatPetModPlayer petPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			petPlayer.TemporarilyUnflagPetBuff(item.buffType);
			return true;
		}
	}

	/**
	 * Bit of a roundabout naming scheme, non-vanilla-clone combat pets came second
	 */
	public abstract class CombatPetCustomMinionItem<TBuff, TProj> : MinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		internal virtual int AttackPatternUpdateTier => 0;

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.ZephyrFish); // via ExampleMod
			base.SetDefaults();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) =>
			CombatPetItemUtils.AddCombatPetTooltip(mod, AttackPatternUpdateTier, tooltips);

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			player.AddBuff(item.buffType, 3);
			return false;
		}

		public override bool CanUseItem(Player player) => CombatPetItemUtils.CanUseItem(player, Item);

	}

	public abstract class CombatPetMinionItem<TBuff, TProj> : VanillaCloneMinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		internal virtual int AttackPatternUpdateTier => 0;

		public override void ModifyTooltips(List<TooltipLine> tooltips) =>
			CombatPetItemUtils.AddCombatPetTooltip(mod, AttackPatternUpdateTier, tooltips);

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			player.AddBuff(item.buffType, 3);
			return false;
		}

		public override bool CanUseItem(Player player) => CombatPetItemUtils.CanUseItem(player, Item);
	}
}
