﻿using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.UI;
using AmuletOfManyMinions.UI.TacticsUI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AmuletOfManyMinions.Core.Minions
{
	/// <summary>
	/// Helper ModPlayer storing the currently selected minion tactic. Handles IO/netcode and works in tandem with the <see cref="TacticsUIMain"/>
	/// </summary>
	public class MinionTacticsPlayer : ModPlayer
	{
		//tag version, increment this if you do breaking changes to the way data is saved/loaded so backwards compat can be done. Write proper code in Load/Save to accomodate
		private const byte LatestVersion = 0;

		public byte TacticID { get; private set; } = TargetSelectionTacticHandler.DefaultTacticID;

		public TargetSelectionTactic SelectedTactic => TargetSelectionTacticHandler.GetTactic(TacticID);

		public PlayerTargetSelectionTactic PlayerTactic;

		private List<byte> TacticIDCycle;

		private byte PreviousTacticID = TargetSelectionTacticHandler.DefaultTacticID;
		private bool isQuickDefending = false;

		internal bool TacticsUnlocked = false;

		// TODO localize
		private static string TacticsUnlockedText = "Minions from the Amulet of Many Minions can now use Advanced Tactics!";

		/// <summary>
		/// Timer used for syncing TacticID when it is changed on the client, only registers the last change done within SyncTimerMax ticks
		/// </summary>
		private int SyncTimer { get; set; } = 0;

		private const int SyncTimerMax = 15;

		private bool syncTactic = false;
		private bool syncConfig = false;

		/// <summary>
		/// Whether or not to choose the target selected by the current minion tactic over
		/// the target selected by the player target reticle.
		/// Set by the mod config on each player. Needs to be synced in multiplayer
		/// </summary>
		internal byte IgnoreVanillaMinionTarget = 0;

		/// <summary>
		/// Use this method to set TacticID during gameplay (through the UI, in netcode)
		/// </summary>
		/// <param name="id">The TacticID to switch to</param>
		/// <param name="fromSync">If this is called within SyncPlayer, so it doesn't send TacticPacket unnecessarily</param>
		public void SetTactic(byte id, bool fromSync = false)
		{
			if (!fromSync && TacticID != id && Main.myPlayer == player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				SyncTimer = 1;
				syncTactic = true;
			}
			TacticID = id;
			PlayerTactic = SelectedTactic.CreatePlayerTactic();
		}

		public override void Initialize()
		{
			TacticID = TargetSelectionTacticHandler.GetTactic(TacticID).ID; //Safe conversion of default tactic
			SyncTimer = 0;
			TacticIDCycle = new List<byte>
			{
				TargetSelectionTacticHandler.GetTactic<ClosestEnemyToMinion>().ID,
				TargetSelectionTacticHandler.GetTactic<ClosestEnemyToPlayer>().ID,
				TargetSelectionTacticHandler.GetTactic<StrongestEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<WeakestEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<LeastDamagedEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<MostDamagedEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<SpreadOut>().ID,
				TargetSelectionTacticHandler.GetTactic<AttackGroups>().ID,
			};
		}

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();

			TagCompound tacticsTag = new TagCompound
			{
				{ "v", (byte)LatestVersion },
				{ "name", SelectedTactic.Name }, //Important to save the name and not the ID as its dynamic. Names are the actual "identifiers"
				{ "unlocked", TacticsUnlocked }
			};

			tag.Add("tactic", tacticsTag);

			return tag;
		}

		public override void Load(TagCompound tag)
		{
			TagCompound tacticsTag = tag.Get<TagCompound>("tactic");

			byte tacticVersion = tacticsTag.GetByte("v");
			//Do special logic here if the system changes drastically (used for backwards compat)

			if (tacticVersion == 0)
			{
				string tacticName = tacticsTag.GetString("name");

				TacticID = TargetSelectionTacticHandler.GetTactic(tacticName).ID;
				if(tacticsTag.ContainsKey("unlocked"))
				{
					TacticsUnlocked = tacticsTag.GetBool("unlocked");
				} else
				{
					TacticsUnlocked = false;
				}
			}
		}

		public override void OnEnterWorld(Player player)
		{
			if (Main.netMode == NetmodeID.Server) return; //Safety check, this hook shouldn't run serverside anyway
			UserInterfaces.tacticsUI.SetSelected(TacticID);
			if(!TacticsUnlocked)
			{
				UserInterfaces.tacticsUI.SetOpenClosedState(OpenedTriState.HIDDEN);
			} else if (UserInterfaces.tacticsUI.opened == OpenedTriState.HIDDEN) 
			{
				UserInterfaces.tacticsUI.SetOpenClosedState(OpenedTriState.FALSE);
			}
			PlayerTactic = SelectedTactic.CreatePlayerTactic();
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			//This hook is ran on the player that enters in two cases:
			//1. another player joins, so it has to send this players info to it
			//    * The server simply sends the data this player has to the joined player
			//2. this player joins, having to send its info to everyone
			//    * The client sends his info to the server
			//    * Server sends that info to all other players (note that the method is called again, the packet itself doesn't broadcast)
			new SyncMinionTacticsPlayerPacket(player, TacticID, IgnoreVanillaMinionTarget).Send(toWho, fromWho);
		}

		public override void PreUpdate()
		{
			if(Main.myPlayer == player.whoAmI)
			{
				byte newIgnoreTargetStatus =(byte)(ClientConfig.Instance.IgnoreVanillaTargetReticle ? 1 : 0);
				if(IgnoreVanillaMinionTarget != newIgnoreTargetStatus)
				{
					syncConfig = true;
					SyncTimer = Math.Max(SyncTimer, 1);
				}
				IgnoreVanillaMinionTarget = newIgnoreTargetStatus;
			}
			//Client sends his newly selected tactic after a small delay
			if (Main.myPlayer == player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				if (SyncTimer > 0)
				{
					SyncTimer++; //If it has been set to 1, increment
					if (SyncTimer > SyncTimerMax)
					{
						SyncTimer = 0; //Stop timer from incrementing
						if(syncTactic)
						{
							new TacticPacket(player, TacticID).Send();
							syncTactic = false;
						}
						if(syncConfig)
						{
							new ConfigPacket(player, IgnoreVanillaMinionTarget).Send();
							syncConfig = false;
						}
					}
				}
			}
			PlayerTactic?.PreUpdate();
		}

		public override void PostUpdate()
		{
			CheckForAoMMItem();
			PlayerTactic?.PostUpdate();
		}

		private void CheckForAoMMItem()
		{
			// only check once per second on the client player
			if(player.whoAmI != Main.myPlayer || TacticsUnlocked || Main.GameUpdateCount % 60 != 0)
			{
				return;
			}
			foreach(Item item in player.inventory)
			{
				if(item.type == ItemID.None || item.modItem == null)
				{
					continue;
				}
				if(item.modItem.mod.Name == mod.Name && item.summon)
				{
					TacticsUnlocked = true;
					break;
				}
			}
			if(TacticsUnlocked)
			{
				Main.NewText(TacticsUnlockedText);
				UserInterfaces.tacticsUI.SetOpenClosedState(OpenedTriState.TRUE);
			}

		}
		private void CycleTactic()
		{
			int nextTacticIndex = (TacticIDCycle.IndexOf(TacticID) + 1) % TacticIDCycle.Count;
			SetTactic(TacticIDCycle[nextTacticIndex]);
		}

		private void StartQuickDefending()
		{
			isQuickDefending = true;
			PreviousTacticID = TacticID;
			SetTactic(TargetSelectionTacticHandler.GetTactic<ClosestEnemyToPlayer>().ID);
			UserInterfaces.tacticsUI.SetSelected(TacticID);
		}

		private void StopQuickDefending()
		{
			isQuickDefending = false;
			SetTactic(PreviousTacticID);
			UserInterfaces.tacticsUI.SetSelected(TacticID);
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			// this is probably only run client side, but to be safe
			if(player.whoAmI != Main.myPlayer || !TacticsUnlocked)
			{
				return;
			}
			if(AmuletOfManyMinions.CycleTacticHotKey.JustPressed)
			{
				CycleTactic();
				// for testing, also toggle grid snapping
				if(SpriteCompositionHelper.posResolution == 2)
				{
					SpriteCompositionHelper.posResolution = 1; 
					SpriteCompositionHelper.frameResolution = 1;
				}  else
				{
					SpriteCompositionHelper.posResolution = 2;
					SpriteCompositionHelper.frameResolution = 5;
				}
				
				UserInterfaces.tacticsUI.SetSelected(TacticID);
			}
			if(AmuletOfManyMinions.QuickDefendHotKey.JustPressed )
			{
				if(ClientConfig.Instance.QuickDefendHotkeyStyle == ClientConfig.QuickDefendToggle)
				{
					if(isQuickDefending)
					{
						StopQuickDefending();
					} else
					{
						StartQuickDefending();
					}
				} else
				{
					StartQuickDefending();
				}
			} else if (AmuletOfManyMinions.QuickDefendHotKey.JustReleased && ClientConfig.Instance.QuickDefendHotkeyStyle == ClientConfig.QuickDefendHold)
			{
				StopQuickDefending();
			}
			
		}
	}
}
