using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

//[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class Main : MonoBehaviour
{
    // Static
    static protected Main instance;
    static public Main Instance
    {
        get { return instance; }
        set { instance = value; }
    }


    // Members
    public bool m_debugThrowableEnabled = true;
    
    public int    m_initialBreakableCount = 10; 
    public int    m_initialItemCount = 5;
    
    public List<GameObject> items;

    private GameObject m_throwableMgr;
    private ItemsManager m_itemMgr;

    private CityManager m_cityMgr;

    private System.Random m_rand = new System.Random();
    
    // Methods
    void Awake()
    {
        Init();

    }
    
    public void Init()
    {
        instance = this;
        
        GameObject itemMgrPre = Resources.Load<GameObject>("Prefabs/ItemManager");
        m_itemMgr = GameObject.Instantiate(itemMgrPre, Vector3.zero, Quaternion.identity).GetComponent<ItemsManager>();

        if ( BoltNetwork.IsServer )
        {
            GameObject throwableMgrPre = Resources.Load<GameObject>("Prefabs/ThrowableManager");
            m_throwableMgr = GameObject.Instantiate( throwableMgrPre, Vector3.zero, Quaternion.identity );
         
            if ( m_cityMgr == null )
            {
                m_cityMgr = new CityManager();
                m_cityMgr.Init();
            }
            
            for (var i = 0; i < m_initialBreakableCount; i++)
            {
                GameObject posObj = m_cityMgr.GetRandomItemSpawnPosObject();
                Vector3 spawnPos = posObj.transform.position;
                var lookAt = Vector3.zero - spawnPos;
                lookAt.y = 0;
                int type = i % 4;    // TODO numTypes isnt' really 4 
                BoltEntity breakableBoltEntity = CreateBreakable(type, spawnPos, Quaternion.LookRotation(lookAt.normalized) );
                breakableBoltEntity.transform.parent = posObj.transform;
                Debug.LogWarning( "Main - Breakable Spawned = " + type + " at " + spawnPos);
            }
            
            if ( m_itemMgr != null )
            {
                
                for ( int i = 0; i < m_initialItemCount; ++i )
                {
                    GameObject itemPosObj = m_cityMgr.GetRandomItemSpawnPosObject();
                    CreateRandomItem( itemPosObj );
                }
            }
        }

#if UNITY_IOS
        var go = new GameObject();
        go.AddComponent<TouchManager>();
        go.name = "TouchManager";
#endif
    }

    private void CreateRandomItem( GameObject posObj )
    {
        ItemsManager.Items type = (ItemsManager.Items) m_rand.Next(1, (int) ItemsManager.Items.Pistol );
        BoltEntity itemBoltEntity = m_itemMgr.CreateItem( type, posObj.transform.position + Vector3.up * 0.5f );   
        itemBoltEntity.transform.parent = posObj.transform;
        itemBoltEntity.transform.localPosition = Vector3.zero;
        Debug.LogWarning( "Main - Item Spawned = " + type + " at " + posObj.transform.position);
        items.Add( itemBoltEntity.gameObject );

    }
    
    private BoltEntity CreateBreakable( int type, Vector3 pos, Quaternion rot )
    {
        PrefabId prefabId; 
        switch( type )
        {
            case 0: prefabId = BoltPrefabs.Breakable_Bus; break;
            case 1: prefabId = BoltPrefabs.Breakable_CopCar; break;
            case 2: prefabId = BoltPrefabs.Breakable_FireTruck; break;
            case 3: prefabId = BoltPrefabs.Breakable_Sedan; break;
            default:
                return null;
        }
        BoltEntity breakable = BoltNetwork.Instantiate( prefabId, pos, rot );
        return breakable;
    }

    private float m_DebugAutoThrowDuration = 0f;
    
    void Update()
    {
        if ( BoltNetwork.IsServer )
        {
            // DEBUG - Auto Throw
            if (m_debugThrowableEnabled)
            {
                m_DebugAutoThrowDuration -= Time.deltaTime;
                if ( m_DebugAutoThrowDuration <= 0 )
                {
                    m_DebugAutoThrowDuration = 2.0f;
                
                    BoltEntity boltEntity = BoltNetwork.Instantiate( BoltPrefabs.fireBall_01_pre, new Vector3(0, 1.3f, 1.0f), Quaternion.identity );
                    Throwable throwable = boltEntity.gameObject.GetComponent<Throwable>();
                    throwable.DebugSetThrown(true);
                    Rigidbody rb = boltEntity.gameObject.GetComponent<Rigidbody>();
                    if ( rb != null )
                    {
                        rb.useGravity = false;
                        rb.velocity = Vector3.forward;
                    }
                }
            }

        }
        
        if ( items.Count < m_initialItemCount )
        {
            GameObject itemPosObj = m_cityMgr.GetRandomItemSpawnPosObject();
            if ( itemPosObj.transform.childCount != 0 )
            {
                itemPosObj = m_cityMgr.GetNextItemSpawnPosObject();
            }
            CreateRandomItem( itemPosObj );
        }
    }
    
    public void Reset()
    {
              
    }
    
    public GameObject GetThrowableManager()
    {
        return m_throwableMgr;
    }
    
    public ItemsManager GetItemManager()
    {
        return m_itemMgr;
    }
        
}