using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdpKit;

[BoltGlobalBehaviour]
public class BoltManager : Bolt.GlobalEventListener
{
	public List<BoltEntity> playerList = new List<BoltEntity>();
	public BoltEntity vrPlayer;

	public System.Action OnVREntityRegistered;
	public System.Action<BoltEntity> OnPlayerEntityRegistered;
	public System.Action<string> OnGameOver;
	public System.Action<ItemsManager.Items> OnItemChanged;
	public System.Action OnGameStateRegistered;


	public static BoltManager Instance
	{
		get; private set;
	}

	void Awake()
	{
		Instance = this;
	}


	public static bool IsHeadlessMode()
	{
		return Environment.CommandLine.Contains("-batchmode") && Environment.CommandLine.Contains("-nographics");
	}

	public override void BoltStartDone()
	{
		if (BoltNetwork.IsServer)
		{
			
			
			string matchName = Environment.MachineName;
			if (Application.platform == RuntimePlatform.Android)
			{
				matchName = "Quest";
			}

			BoltNetwork.SetServerInfo(matchName, null);
			BoltNetwork.LoadScene("1_GameScene");
		}
	}

	public override void Connected(BoltConnection connection)
	{
	}

	public override void Disconnected(BoltConnection connection)
	{
	}
	
	public void RegisterEntity(BoltEntity registeredEntity)
	{
		playerList.Add(registeredEntity);
		if (OnPlayerEntityRegistered != null)
		{
			OnPlayerEntityRegistered(registeredEntity);
		}
	}

	public void RegisterVREntity(BoltEntity ent)
	{
		vrPlayer = ent;
		if (OnVREntityRegistered != null)
		{
			OnVREntityRegistered();
		}
	}

	public void RegisterGameState(BoltEntity ent)
	{
		if (OnGameStateRegistered != null)
		{
			OnGameStateRegistered();
		}
	}
	
	public Vector3 GetSpawnLocation(float radius)
	{
		var randomAngle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
		return new Vector3(radius * Mathf.Sin(randomAngle), 1.0f, radius * Mathf.Cos(randomAngle));
	}
	
	public override void SceneLoadLocalDone(string map)
	{
		if (BoltNetwork.IsClient)
		{
			var spawnPosition = GetSpawnLocation(2.5f);
			spawnPosition.y = 1.0f;
			var player = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPosition, Quaternion.identity);
			FindObjectOfType<CameraController>().player = player;
		}

		if (BoltNetwork.IsServer)
		{
			BoltNetwork.Instantiate(BoltPrefabs.GameState, Vector3.zero, Quaternion.identity);
		}
	}

	public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
	{
		Debug.LogFormat("Session list updated: {0} total sessions", sessionList.Count);

		foreach (var session in sessionList)
		{
			UdpSession photonSession = session.Value as UdpSession;

			if (photonSession.Source == UdpSessionSource.Photon)
			{
				if ((Application.platform == RuntimePlatform.OSXEditor
				    || Application.platform == RuntimePlatform.OSXPlayer)
				    && (photonSession.HostName != Environment.MachineName) 
				    && photonSession.HostName != "Quest")
				{
					Debug.LogWarning("SessionList skip host " + photonSession.HostName);
					continue;
				}
	
				BoltNetwork.Connect(photonSession);
				return;
			}
		}
		
		Debug.LogWarning("SessionListUpdated but no host matching " + Environment.MachineName + " was found.");
	}
	
	public override void OnEvent(FireBulletEvent evnt)
	{
		if (BoltNetwork.IsServer)
		{
            var target = new Vector3(0, 1.0f, 0);
            var vec = target - evnt.SpawnLocation;
            vec.Normalize();
            var prefab = BoltPrefabs.PlayerBasicBullet; // TODO: change prefab based on weapon type
            var spawn = evnt.SpawnLocation;
            var bullet = BoltNetwork.Instantiate(prefab, spawn, Quaternion.LookRotation(vec));
            var bulletComp = bullet.gameObject.GetComponent<PlayerBullet>();

            bulletComp.Init(target, evnt.Damage, evnt.Speed);
		}
	}

	public override void OnEvent(ItemChanged evnt)
	{
		if (BoltNetwork.IsClient)
		{
			var item = Main.Instance.GetItemManager().GetItem((ItemsManager.Items) evnt.Item);

			var player = evnt.Entity.GetComponent<Player>();
			if (player != null)
			{
				if (item == null)
				{
					player.SetCurrentPowerUp(null);
				} 
				else if (item.type.Contains("gun"))
				{
					player.SetCurrentWeapon(item);
				}
				else
				{
					player.SetCurrentPowerUp(item);
				}
				
				if (OnItemChanged != null)
				{
					OnItemChanged((ItemsManager.Items) evnt.Item);
				}
			}
		}
	}

	
	
//	public override void OnEvent(CreateItemEvent evnt)
//	{
//		if (BoltNetwork.IsServer)
//		{
//			var itemPos = evnt.Entity.transform.position + (-evnt.Entity.transform.forward.normalized * 0.2f);
//			itemPos.y = 1.3f;
//
//			var prefab = Main.Instance.GetItemManager().GetBoltPrefabFromItem((ItemsManager.Items) evnt.Item);
//			BoltNetwork.Instantiate(prefab, itemPos, Quaternion.identity);
//		}
//	}

	public override void OnEvent(DamageEvent evnt)
	{
		var player = evnt.HitEntity.GetComponent<Player>();
		if (player != null)
		{
			player.OnHit(evnt.Damage);
		}

		var vrPlayer = evnt.HitEntity.gameObject.GetComponent<VRPlayer>();
		if (vrPlayer != null)
		{
			vrPlayer.OnHit(evnt.Damage);
		}

		var breakable = evnt.HitEntity.gameObject.GetComponent<BreakableObject>();
		if (breakable != null)
		{
			breakable.OnHit(evnt.Damage);
		}
	}

	public override void OnEvent(GameOverEvent evnt)
	{
		if (OnGameOver != null)
		{
			OnGameOver(evnt.WinType);
		}
	}
	
}
