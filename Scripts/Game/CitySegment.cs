using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class CitySegment : EntityBehaviour<ICitySegment>
{
	public enum SegmenetType
	{
		Err = -1,
		None,
		Bridge,
		In,
		Out,
		InOut,
		
		Num
	}
	
	public SegmenetType type = SegmenetType.None;
	public Material m_sunkMaterial;
	public Vector2 m_sunkMaterialScrollDelta = new Vector2(0.1f,0.1f);
	public float sinkHeight = -0.2f;
	public float sinkRate = 0.02f;
	
	private Material[] m_materials;
	private Renderer m_renderer;
	
	private bool m_sinking = false;
	private bool m_sunk = false;


	public override void Attached()
	{
		state.SetTransforms(state.Transform, transform);
		if (entity.isOwner)
		{
			state.SegmentType = -1;
		}
		else
		{
			state.AddCallback( "SegmentType", SetSegmentType );
		}
	}

	public void Reset()
	{
		// TODO
	}
	
	public void Init( int type )
	{
		state.SegmentType = type % (int)SegmenetType.Num;
		
		if ( entity.IsOwner )
		{
			SetSegmentType();
		}
	}
	
	private void SetSegmentType()
	{
		type = (SegmenetType) state.SegmentType;
		
		string strPrefab = "Prefabs/TEMP_SEGMENT";
		
		switch ( state.SegmentType )
		{
			
			case (int)SegmenetType.None:	{ strPrefab = "Prefabs/CitySegment_None";	break; }
			case (int)SegmenetType.Bridge:	{ strPrefab = "Prefabs/CitySegment_Bridge";	break; }
			case (int)SegmenetType.In:		{ strPrefab = "Prefabs/CitySegment_In";		break; }
			case (int)SegmenetType.Out:		{ strPrefab = "Prefabs/CitySegment_Out";	break; }
			case (int)SegmenetType.InOut:	{ strPrefab = "Prefabs/CitySegment_InOut";	break; }
			defualt: { strPrefab = "Prefabs/TEMP_SEGMENT"; break; } 
		}

		// Create Prefab
		GameObject prefab = Resources.Load<GameObject>(strPrefab);
		if ( prefab != null )
		{
			GameObject obj = GameObject.Instantiate( prefab, state.Transform.Position, state.Transform.Rotation );
			obj.transform.parent = transform;
		
			m_renderer = obj.GetComponentInChildren<Renderer>();
			if ( m_renderer != null )
			{
				Material[] materials = m_renderer.materials;
				m_materials = new Material[materials.Length];
				for ( int i = 0; i < materials.Length; ++i ) 
				{
					m_materials[i] = new Material( materials[i] );	
				}
				m_renderer.materials = m_materials;
			}
		}
		else
		{
			Debug.LogError("ERROR! Could not Create City Segment! : " + strPrefab );
		}
	}
	
	public void SinkSegment()
	{
		if ( entity.IsOwner )
		{
			m_sinking = true;
		}
		
		if ( m_sunkMaterial != null )
		{
			for ( int i = 0; i < m_materials.Length; ++i )
			{
				m_materials[i] = new Material( m_sunkMaterial );
			}
			m_renderer.materials = m_materials;
		}
	}

	void Update()
	{
		if ( entity.IsOwner )
		{
			if ( m_sinking )
			{
				Vector3 newPos = transform.position;
				
				if ( transform.position.y > sinkHeight )
				{
					newPos += Vector3.down * sinkRate;
				}
				else
				{
					newPos.y = sinkHeight;
					m_sinking = false;
					m_sunk = true;
				}
				transform.position = newPos;
				
			}
			
			if ( m_sunk )
			{
				
			}
		}
		else if ( transform.position.y < 0.0f )	// TODO: Kluge
		{
			if ( !m_sunk )
			{
				SinkSegment();
			}
			m_sunk = true;
		}
		
		if ( m_sinking || m_sunk )
		{
			// Scroll offset
			for ( int i = 0; i < m_materials.Length; ++i )
			{
				m_materials[i].mainTextureOffset += m_sunkMaterialScrollDelta;
			}
			m_renderer.materials = m_materials;
		}

	}
}
