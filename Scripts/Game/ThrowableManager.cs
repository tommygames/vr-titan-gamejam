using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableManager : MonoBehaviour
{
	// Types
	public enum ThrowableType
	{
		None = -1,
		Earth,
		Wind,
		Fire,
		Water,
		Heart,
		
		Num
	}
	
	class SpawnSpot
	{
		public float			timer;
		public ThrowableType	type;
		public BoltEntity		throwableObj;
		public GameObject		spawnPosObj;
		public int				index;
	}
	
	
	// Members
	List<SpawnSpot> m_spawnSpots;

	public float respawnTime = 5.0f; 	// TODO: Should be based on obj type?

	// Methods
	void Start()
	{
		m_spawnSpots = new List<SpawnSpot>();
		
		GameObject[] spots = GameObject.FindGameObjectsWithTag("ThrowableSpawn");
		for ( int i = 0; i < spots.Length; ++i )
		{
			SpawnSpot spot = new SpawnSpot();
			spot.timer = 0;
			spot.type = (ThrowableType) ( i % (int)ThrowableType.Num); 	// Currently each is assigned a type
			spot.throwableObj = null;
			spot.spawnPosObj = spots[i];
			spot.index = i;
			m_spawnSpots.Add( spot );
			Debug.LogWarning( "ThrowableSpawn at " + spot.spawnPosObj.transform.position );
			CreateThrowable( spot );
		}
	}
	
	void Update() 
	{
		// Only on Server
		for ( int i = 0; i < m_spawnSpots.Count; ++i )
		{
			SpawnSpot spot = m_spawnSpots[i];
			if ( spot.timer > 0)
			{
				spot.timer -= Time.deltaTime;
				if ( spot.timer <= 0 )
				{
					CreateThrowable( spot );
				}
			}
		}
	}
	
	private void CreateThrowable( SpawnSpot spot )
	{
		Vector3 spawnPos = spot.spawnPosObj.transform.position;
		spot.throwableObj = BoltNetwork.Instantiate( BoltPrefabs.fireBall_01_pre, spot.spawnPosObj.transform.position, Quaternion.identity );
		spot.throwableObj.transform.parent = transform;
		spot.throwableObj.transform.position = spot.spawnPosObj.transform.position;
		spot.timer = 0;
		
		Throwable throwable = spot.throwableObj.gameObject.GetComponent<Throwable>();
		throwable.m_sourceIndex = spot.index;
	}
	
	public void TakeThrowable( int index )
	{
		if ( index >= 0 && index < m_spawnSpots.Count )
		{
			SpawnSpot spot = m_spawnSpots[index];
			spot.timer = respawnTime;
			spot.throwableObj = null;
			Debug.Log("Take Throwable");
		}
		else
		{
			Debug.Log("WTF: throwable index too beaucoup or negative! In fact, it's " + index );
		}
	}
}
