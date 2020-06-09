using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class CityManager
{
    private int m_segCount = 20; 
    
    private List<GameObject> m_itemSpawnPosList;
    private List<GameObject> m_specialSpawnPosList;
    private List<GameObject> m_playerSpawnPosList;

    private System.Random m_rand;
    
    public void Init()
    {
        if ( BoltNetwork.IsServer )
        {
            m_itemSpawnPosList    = new List<GameObject>();
            m_specialSpawnPosList = new List<GameObject>();
            m_playerSpawnPosList  = new List<GameObject>();
            
            m_rand = new System.Random(); 
            
            float segDegrees = 360 / m_segCount;
            for ( int i = 0; i < m_segCount; ++i )
            {
                Quaternion rot = Quaternion.AngleAxis( segDegrees * i, Vector3.up );
                BoltEntity entity = BoltNetwork.Instantiate( BoltPrefabs.CitySegment, Vector3.zero, rot );
                entity.gameObject.name = "CitySegment" + i;
                CitySegment citySeg = entity.GetComponent<CitySegment>();
                citySeg.Init( i );    // TODO: Random segment type?
                
                Transform posRoot = entity.gameObject.transform.GetChild(0);
                
                for ( int c = 0; c < posRoot.childCount; ++c )
                {
                    Transform t = posRoot.GetChild(c);

                    if ( t.name.Contains("ItemSpawnPos") )
                    {
                        m_itemSpawnPosList.Add( t.gameObject );
                    }
                    else if ( t.name.Contains("SpecialSpawnPos") )
                    {
                        m_specialSpawnPosList.Add( t.gameObject );
                    }
                    else if ( t.name.Contains("PlayerSpawnPos") )
                    {
                        m_playerSpawnPosList.Add( t.gameObject );
                    }
                }
            }
        }
    }
    
    public GameObject GetNextItemSpawnPosObject()
    {
        for ( int i = 0; i < m_itemSpawnPosList.Count; ++i )
        {
            if ( m_itemSpawnPosList[i].transform.childCount == 0 )
            {
                return m_itemSpawnPosList[i];
            }
        }
        return null;
    }

    public GameObject GetNextItemSpecialPosObject()
    {
        for ( int i = 0; i < m_specialSpawnPosList.Count; ++i )
        {
            if ( m_specialSpawnPosList[i].transform.childCount == 0 )
            {
                return m_specialSpawnPosList[i];
            }
        }
        return null;
    }

    public GameObject GetRandomItemSpawnPosObject()
    {
        if ( m_itemSpawnPosList.Count > 0 )
        {
            int index = m_rand.Next(0, m_itemSpawnPosList.Count);
            return m_itemSpawnPosList[index];
        }
        return null;
    }
    
    public GameObject GetRandomSpecialSpawnPosObject()
    {
        if ( m_specialSpawnPosList.Count > 0 )
        {
            int index = m_rand.Next(0, m_specialSpawnPosList.Count);
            return m_specialSpawnPosList[index];
        }
        return null;
    }
    
    public GameObject GetRandomPlayerSpawnPosObject()
    {
        if ( m_playerSpawnPosList.Count > 0 )
        {
            int index = m_rand.Next(0, m_playerSpawnPosList.Count);
            return m_playerSpawnPosList[index];
        }
        return null;
    }

    public void Reset()
    {
        // TODO
    }
}
