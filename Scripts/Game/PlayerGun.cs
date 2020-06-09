using UnityEngine;

public class PlayerGun : Item
{
    
    public PlayerGun(string type, float rate, int dmg, float spd)
    {
        this.type = type;
        fireRate = rate;
        damage = dmg;
        speed = spd;
    }

    public override void UseItem()
    {
        foreach (var boltEntity in BoltManager.Instance.playerList)
        {
            if (boltEntity.IsOwner)
            {
                var offset = new Vector3(0, 0.15f, 0.12f);
                var worldPos = boltEntity.transform.localToWorldMatrix.MultiplyPoint(offset);
                var spawnPos = worldPos;//player.entity.transform.position;
                var fireEvent = FireBulletEvent.Create();
                fireEvent.Damage = damage;
                fireEvent.Speed = speed;
                fireEvent.SpawnLocation = spawnPos;
                fireEvent.Entity = boltEntity;
                fireEvent.Send();
                break;
            }
        }
    }
}