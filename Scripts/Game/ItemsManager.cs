using Bolt;
using UnityEngine;


public class ItemsManager : MonoBehaviour
{
    private Item[] itemArray;

    public enum Items
    {
        None,
        Bazooka,
        Shield,
        Rifle,                // slower, more powerful
        MachineGun,
        
        Pistol,            // basic
        Num
    }

    private void Awake()
    {
        CreateItemAtlas();
    }

    private void Start()
    {
        if (BoltNetwork.IsServer)
        {
//            var bazookaPosition = GetSpawnLocation(2.5f);
//            bazookaPosition.y = 1.08f;
//            var bazooka = BoltNetwork.Instantiate(BoltPrefabs.BazookaDrop, bazookaPosition, Quaternion.identity);
//
//            var shieldPosition = GetSpawnLocation(2.5f);
//            shieldPosition.y = 1.08f;
//            var shield = BoltNetwork.Instantiate(BoltPrefabs.ShieldDrop, shieldPosition, Quaternion.identity);
//
//            var pistolPosition = GetSpawnLocation(2.5f);
//            pistolPosition.y = 1.08f;
//            var pistol = BoltNetwork.Instantiate(BoltPrefabs.PistolDrop, pistolPosition, Quaternion.identity);
//
//            var riflePosition = GetSpawnLocation(2.5f);
//            riflePosition.y = 1.08f;
//            var rifle = BoltNetwork.Instantiate(BoltPrefabs.RifleDrop, riflePosition, Quaternion.identity);
//            
//            var machineGunPosition = GetSpawnLocation(2.5f);
//            machineGunPosition.y = 1.08f;
//            var machineGun = BoltNetwork.Instantiate(BoltPrefabs.MachineGunDrop, machineGunPosition, Quaternion.identity);
        }

    }

    public BoltEntity CreateItem( Items itemType, Vector3 pos )
    {
        var prefab = GetBoltPrefabFromItem( itemType );
        BoltEntity item = BoltNetwork.Instantiate(prefab, pos, Quaternion.identity);
        return item;
    }
    
    public void CreateItemAtlas()
    {
        itemArray = new Item[]
        {
            null,
            new Bazooka("powerup", 1, 1000, 1),
            new Shield("powerup", 1, 20),
            new PlayerGun("gun", 0.6f, 40, 6),
            new PlayerGun("gun", 0.15f, 16, 3),
            new PlayerGun("gun", 0.4f, 15, 3)
        };
    }

//    private void AssignPlayersInitialWeapons()
//    {
//        foreach (var boltEntity in BoltManager.Instance.playerList)
//        {
//            var player = boltEntity.GetComponent<Player>();
//            if (player)
//            {
//                player.SetCurrentWeapon(GetItem(Items.Pistol));
//            }
//        }
//    }

    public PrefabId GetBoltPrefabFromItem(Items item)
    {
        switch (item)
        {
            case Items.Bazooka:
                return BoltPrefabs.BazookaDrop;
            case Items.Shield:
                return BoltPrefabs.ShieldDrop;
            case Items.Pistol:
                return BoltPrefabs.PistolDrop;
            case Items.Rifle:
                return BoltPrefabs.RifleDrop;
            case Items.MachineGun:
                return BoltPrefabs.MachineGunDrop;
            case Items.None:
                return BoltPrefabs.PistolDrop;
            default:
                return BoltPrefabs.PistolDrop;
        }
    }

    public int GetEnumForItem(Item item)
    {
        for (int i = 0; i < itemArray.Length; i++)
        {
            if (itemArray[i] != null && itemArray[i].Equals(item))
            {
                return i;
            }
        }

        return 0;
    }

    public Vector3 GetSpawnLocation(float radius)
    {
        var randomAngle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        return new Vector3(radius * Mathf.Sin(randomAngle), 1.0f, radius * Mathf.Cos(randomAngle));
    }
    
    public Item GetItem(Items type)
    {
        return itemArray[(int) type];
    }
}
