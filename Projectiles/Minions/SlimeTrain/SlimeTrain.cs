﻿using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.Projectiles.Minions.SlimeTrain.SlimeTrainMarkerProjectile;
using System.Collections.Generic;
using AmuletOfManyMinions.Projectiles.Minions.Slimecart;
using AmuletOfManyMinions.Core.Minions.Effects;

namespace AmuletOfManyMinions.Projectiles.Minions.SlimeTrain
{
	public class SlimeTrainMinionBuff : MinionBuff
	{
		public SlimeTrainMinionBuff() : base(ProjectileType<SlimeTrainCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Celestial Slime Train");
			Description.SetDefault("A celestial train and its passengers will fight for you!");
		}
	}

	public class SlimeTrainMinionItem : MinionItem<SlimeTrainMinionBuff, SlimeTrainCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Staff of the Celestial Slime Train");
			Tooltip.SetDefault("Summons a celestial train to fight for you!");

		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.damage = 120;
			item.height = 34;
			item.value = Item.sellPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.Red;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.LunarBar, 12);
			recipe.AddIngredient(ItemType<SlimecartMinionItem>(), 1);
			recipe.AddRecipeGroup("AmuletOfManyMinions:StardustDragons", 1);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class SlimeTrainCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<SlimeTrainMinionBuff>();
		protected override int MinionType => ProjectileType<SlimeTrainMinion>();
	}
	public class SlimeTrainMinion : WormMinion
	{
		internal override int BuffId => BuffType<SlimeTrainMinionBuff>();
		protected override int CounterType => ProjectileType<SlimeTrainCounterMinion>();
		protected override int dustType => DustType<StarDust>();


		private int potentialTargetCount = 0;
		private int lastSpawnedSlimeFrame;
		private int nextSlimeIndex;
		private bool inOpenAir;
		

		private int SubProjectileType; 
		private Projectile currentMarker = null;

		private Texture2D SlimeTexture;

		private SlimeTrainRotationTracker rotationTracker;

		private List<int> summonedSlimes = new List<int>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Celestial Steam Train");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = false;
			attackThroughWalls = true;
			frameSpeed = 8;
			if(SlimeTexture == null)
			{
			}
			SubProjectileType = ProjectileType<SlimeTrainMarkerProjectile>();
			rotationTracker = new SlimeTrainRotationTracker();
			wormDrawer = new SlimeTrainDrawer();
		}
		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			int radius = Math.Abs(player.velocity.X) < 4 ? 120 : 24;
			float idleAngle = 2 * PI * groupAnimationFrame / groupAnimationFrames;
			idlePosition.X += radius * (float)Math.Cos(idleAngle);
			idlePosition.Y += radius * (float)Math.Sin(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.75f);
			// find the SlimeTrainMarker that's still in its spawn animation, if any
			currentMarker = null;
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == SubProjectileType && 
					(int)p.localAI[0] < SetupTime)
				{
					currentMarker = p;
					break;
				}
			}
			// spawn generic baby slime minions if there's a lot of enemies nearby
			SpawnReinforcements();
			return vectorToIdlePosition;
		}

		private void SpawnReinforcements()
		{
			Tile tile = Framing.GetTileSafely(new Point((int)projectile.position.X / 16, (int)projectile.position.Y / 16));
			inOpenAir = !tile.active();
			int slimeType = ProjectileType<SlimeTrainSlimeMinion>();
			int currentSlimeCount = player.ownedProjectileCounts[slimeType];
			summonedSlimes.Clear();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == slimeType)
				{
					summonedSlimes.Add((int)p.ai[1]);
				}
			}
			// If there's a lot of nearby enemies, sspawn in a slime buddy to help out
			if (Main.myPlayer == player.whoAmI && inOpenAir 
				&& animationFrame - lastSpawnedSlimeFrame > 120 &&
				potentialTargetCount > 1 && currentSlimeCount < EmpowerCount)
			{
				lastSpawnedSlimeFrame = animationFrame;
				for (int i = 0; i < summonedSlimes.Count; i++)
				{
					if(summonedSlimes.Contains(nextSlimeIndex +1))
					{
						nextSlimeIndex = (nextSlimeIndex + 1) % EmpowerCount;
					} else
					{
						break;
					}
				}
				Vector2 angle = Vector2.Zero;
				int dist = 48 + 30 * (currentSlimeCount + 1);
				Vector2 spawnPos = wormDrawer.PositionLog.PositionAlongPath(dist, ref angle);
				Projectile.NewProjectile(
					spawnPos,
					projectile.velocity,
					slimeType,
					(int)(projectile.damage * 0.75f),
					projectile.knockBack,
					Main.myPlayer,
					ai1: nextSlimeIndex + 1);
				nextSlimeIndex = (nextSlimeIndex + 1) % EmpowerCount;
			}
			potentialTargetCount = 0;

		}

		public override bool ShouldIgnoreNPC(NPC npc)
		{
			// ignore any npc with a marker actively placed on it
			bool shouldIgnore = base.ShouldIgnoreNPC(npc) || Main.projectile.Any(p =>
				p.active && p.owner == player.whoAmI &&
				p.type == SubProjectileType && (int)p.ai[0] == npc.whoAmI);
			// this is a bit hacky, but it's the easiest existing hook to get
			// info on every possible target
			int subSpawnRadius = 700 * 700;
			if(!shouldIgnore && inOpenAir
				&& Vector2.DistanceSquared(projectile.Center, npc.Center) < subSpawnRadius 
				&& Collision.CanHitLine(projectile.Center, 1, 1, npc.Center, 1, 1))
			{
				potentialTargetCount += 1;
			}
			return shouldIgnore;
		}

		protected override float ComputeSearchDistance()
		{
			return 1400 + 50 * GetSegmentCount();
		}

		protected override float ComputeInertia()
		{
			return Math.Max(12, 22 - GetSegmentCount());
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(22, 14 + GetSegmentCount());
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 4;
		}

		private void DoMarkerSpawnMovement()
		{
			int frame = (int)currentMarker.localAI[0];
			projectile.velocity = rotationTracker.GetNPCTargetRadius(frame) - projectile.position;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(currentMarker != null)
			{
				DoMarkerSpawnMovement();
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(currentMarker != null)
			{
				DoMarkerSpawnMovement();
			} else if(targetNPCIndex is int idx)
			{
				rotationTracker.SetRotationInfo(Main.npc[idx], projectile);
				Vector2 startOffset = rotationTracker.GetStartOffset(vectorToTargetPosition);
				if(startOffset.LengthSquared() < 32 * 32 && player.whoAmI == Main.myPlayer)
				{
					Projectile.NewProjectile(
						projectile.Center,
						projectile.velocity * 0.25f,
						SubProjectileType,
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer,
						ai0: idx,
						ai1: EmpowerCount);
				}
				base.TargetedMovement(startOffset);
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override Vector2? FindTarget()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, searchDistance, losCenter: player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, searchDistance, losCenter: player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			((SlimeTrainDrawer)wormDrawer).Update(projectile.frame, summonedSlimes);
			projectile.friendly = false;
		}
	}

	internal class SlimeTrainDrawer : WormDrawer
	{
		int dustType => DustType<StarDust>();
		private int SlimeFrameTop(int i) => 40 * (i % nSlimes) + 4;
		private int FrameHeight => 34;

		private int nSlimes = 7;
		private Texture2D SlimeTexture;

		private List<int> summonedSlimes = new List<int>();
		private int YFrameTop => 40 * frame + 4;


		public void Update(int frame, List<int> summonedSlimes)
		{
			base.Update(frame);
			this.summonedSlimes = summonedSlimes;
		}
		public override void Draw(Texture2D texture, SpriteBatch spriteBatch, Color lightColor)
		{
			SlimeTexture = GetTexture(texture + "_Slimes");
			base.Draw(texture, spriteBatch, lightColor);
		}
		protected override void DrawHead()
		{
			Rectangle slime = new Rectangle(0, SlimeFrameTop(0), 52, FrameHeight);
			Texture2D mainTexture = texture;
			texture = SlimeTexture;
			AddSprite(2, slime);
			Rectangle head = new Rectangle(70, YFrameTop, 52, FrameHeight);
			texture = mainTexture;
			AddSprite(2, head);
		}

		protected override void DrawBody()
		{
			Rectangle body;
			Texture2D mainTexture = texture;
			for (int i = 0; i < SegmentCount + 1; i++)
			{
				if (i == 0)
				{
					body = new Rectangle(36, YFrameTop, 30, FrameHeight);
				}
				else 
				{
					if(summonedSlimes.IndexOf(i) == -1)
					{
						Rectangle slime = new Rectangle(0, SlimeFrameTop(i), 30, FrameHeight);
						texture = SlimeTexture;
						AddSprite(48+30 * i, slime);
					}
					body = new Rectangle(2, YFrameTop, 30, FrameHeight);
				}
				texture = mainTexture;
				AddSprite(48 + 30 * i, body);
			}
		}

		protected override void DrawTail()
		{
			// no tail
			lightColor = Color.White * 0.85f;
			lightColor.A = 128;
		}

		protected override void AddSprite(float dist, Rectangle bounds, Color c = default)
		{
			Vector2 origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
			Vector2 angle = new Vector2();
			Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
			float r = angle.ToRotation();
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, c == default ? lightColor : c, r,
				origin, 1, GetEffects(r), 0);
			if (Main.rand.Next(20) == 0)
			{
				int dustId = Dust.NewDust(pos, bounds.Width, bounds.Height, dustType, 0f, 0f, 0, default, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
				Main.dust[dustId].velocity = Vector2.Zero;
			}
		}

	}

}
