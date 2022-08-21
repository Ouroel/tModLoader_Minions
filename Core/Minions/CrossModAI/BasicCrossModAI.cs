﻿using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	internal class ProjectileStateCache
	{
		internal Vector2 InitialPosition { get; private set; }
		internal Vector2 InitialVelocity { get; private set; }
		internal Vector2? Position { get; private set; }
		internal Vector2? Velocity { get; private set; }

		internal Vector2? PlayerPosition { get; private set; }
		internal bool? TileCollide { get; private set; }

		// this one is a bit contentious, appears to get set erroneously
		internal float? GfxOffY { get; private set; }

		public void ClearProjectile()
		{
			Position = null;
			Velocity = null;
			TileCollide = null;
			GfxOffY = null;
		}

		public void Clear()
		{
			ClearProjectile();
			PlayerPosition = null;
		}

		public void Cache(Projectile proj)
		{
			Position ??= proj.position;
			Velocity ??= proj.velocity;
			TileCollide ??= proj.tileCollide;
			GfxOffY ??= proj.gfxOffY;
			PlayerPosition ??= Main.player[proj.owner].position;
		}

		public void Rollback(Projectile proj)
		{
			proj.position = InitialPosition;
			proj.velocity = InitialVelocity;
			ClearProjectile();
		}

		public void CacheInitial(Projectile proj)
		{
			InitialPosition = proj.position;
			InitialVelocity = proj.velocity;
		}

		public void Uncache(Projectile proj)
		{
			proj.position = Position ?? proj.position;
			proj.velocity = Velocity ?? proj.velocity;
			proj.tileCollide = TileCollide ?? proj.tileCollide;
			proj.gfxOffY = GfxOffY ?? proj.gfxOffY;
			Main.player[proj.owner].position = PlayerPosition ?? Main.player[proj.owner].position;
			Clear();
		}
	}

	/// <summary>
	/// Basic cross-mod AI that allows for exposing AoMM-tracked state to other mods. Also include
	/// the basic "follow waypoint" code here, since it's pretty simple.
	/// </summary>
	internal class BasicCrossModAI : ICrossModSimpleMinion
	{

		public Projectile Projectile { get; set; }

		public Player Player => Main.player[Projectile.owner];
		public int BuffId { get; set; }

		public SimpleMinionBehavior Behavior { get; set; }

		[CrossModProperty]
		public int MaxSpeed { get; internal set; }

		[CrossModProperty]
		public int Inertia { get; internal set; }

		[CrossModProperty]
		public int SearchRange { get; internal set; }

		internal bool UseDefaultPathfindingMovement { get; set; }

		internal ProjectileStateCache ProjCache { get; set; } = new();

		public WaypointMovementStyle WaypointMovementStyle => WaypointMovementStyle.IDLE;

		// A block of properties used exclusively for generating the cross mod state dictionary
		// TODO it is a bit roundabout to implement this

		[CrossModProperty]
		public Vector2? NextPathfindingTaret => Behavior.NextPathfindingTarget;

		[CrossModProperty]
		public Vector2? PathfindingDestination => Behavior.PathfindingDestination;

		[CrossModProperty]
		public NPC TargetNPC => Behavior.TargetNPCIndex is int idx ? Main.npc[idx] : default;

		// TODO this is a lazy implementation, doesn't sort wholly correctly
		// TODO enemies will gradually disappear from this list as they die, until a full refresh is done
		[CrossModProperty]
		public List<NPC> PossibleTargetNPCs => Behavior.PossibleTargets?
			.Where(npc=>npc.active)
			.OrderBy(npc => Vector2.DistanceSquared(npc.Center, TargetNPC?.Center ?? Player.Center))
			.ToList();

		[CrossModProperty]
		public bool IsPet { get; set; }

		[CrossModProperty]
		public int PetLevel => Player.GetModPlayer<LeveledCombatPetModPlayer>().PetLevelInfo.Level;

		[CrossModProperty]
		public int PetDamage => Player.GetModPlayer<LeveledCombatPetModPlayer>().PetLevelInfo.BaseDamage;

		// Basic state variables for the three things a minion can do (attack, pathfind, and idle)
		[CrossModProperty]
		public bool IsPathfinding => Behavior.IsFollowingBeacon;

		[CrossModProperty]
		public bool IsAttacking => Behavior.VectorToTarget != default && !IsPathfinding;

		[CrossModProperty]
		public bool IsIdle => !IsAttacking && !IsPathfinding;


		// Cache the names of cross mod properties for faster lookup
		private Dictionary<string, PropertyInfo> CrossModProperties { get; set; }

		// Cache for the values of cross mod properties, reset every frame
		private Dictionary<string, object> CrossModStateDict { get; set; }




		public BasicCrossModAI(
			Projectile projectile, 
			int buffId, bool defaultPathfinding = false, bool isPet = false)
		{
			Projectile = projectile;
			BuffId = buffId;
			UseDefaultPathfindingMovement = defaultPathfinding;
			IsPet = isPet;
			Behavior = new(this);
			FindCrossModProperties();
		}

		private void FindCrossModProperties()
		{
			CrossModProperties = GetType().GetProperties()
				.Where(p => p.IsDefined(typeof(CrossModProperty), false))
				.ToDictionary(p => p.Name, p => p);
		}



		internal void FakePlayerFlyingHeight()
		{
			ProjCache.Cache(Projectile);
			Player.position.Y = Projectile.position.Y - 320;
		}

		internal virtual void ApplyPetDefaults()
		{
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
		}

		internal virtual void UpdatePetState()
		{
			var leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			var info = CombatPetLevelTable.PetLevelTable[leveledPetPlayer.PetLevel];
			SearchRange = info.BaseSearchRange;
			Inertia = info.Level < 6 ? 10 : 15 - info.Level;
			MaxSpeed = (int)info.BaseSpeed;
			Projectile.originalDamage = leveledPetPlayer.PetDamage;
		}

		public virtual void OnSpawn()
		{
			if(IsPet) { ApplyPetDefaults(); }
		}

		public virtual Vector2 IdleBehavior()
		{
			CrossModStateDict = null;
			ProjCache.CacheInitial(Projectile);
			if(IsPet) { UpdatePetState(); }
			// no op
			return default;
		}

		public virtual void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// Only take over idle movement if pathfinding
			if(!Behavior.IsFollowingBeacon || !UseDefaultPathfindingMovement) { return; }

			if(vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= MaxSpeed;
			} 
			if (!Behavior.Pathfinder.InTransit && vectorToIdlePosition.LengthSquared() < MaxSpeed * MaxSpeed)
			{
				Projectile.velocity = vectorToIdlePosition;
			} else
			{
				Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			}
			// trick to force many vanilla-styled minions into their flying animation
			// Would be surprised if this did not have unintended side effects
			FakePlayerFlyingHeight();
		}
		
		public virtual Vector2? FindTarget()
		{
			return Behavior.SelectedEnemyInRange(SearchRange) is Vector2 target ? target - Projectile.Center : null;
		}


		public virtual void PostAI()
		{
			ProjCache.Uncache(Projectile);
		}

		public virtual void AfterMoving() 
		{
			// no op
		}

		public virtual bool DoVanillaAI() => true;


		public virtual bool MinionContactDamage() => false;

		public virtual void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// No op
		}

		// Utility method that can be called from within the AI() of a cross mod minion
		// to prevent AoMM from overriding changes made to its velocity this frame
		public void ReleaseControl()
		{
			ProjCache.Rollback(Projectile);
		}

		public Dictionary<string, object> GetCrossModState()
		{
			// TODO evaluate the efficiency of using reflection here
			// TODO make some of these writable in some capacity
			CrossModStateDict ??= CrossModProperties.ToDictionary(kv => kv.Key, kv => kv.Value.GetValue(this, null));
			return CrossModStateDict;
		}
			
	}
}
