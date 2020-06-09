
public class Bazooka : Item
{
    public Bazooka(string type, float uses, int dmg, float spd)
    {
        this.type = type;
        this.uses = uses;
        damage = dmg;
        speed = spd;
    }

    public override void UseItem()
    {
        foreach (var boltEntity in BoltManager.Instance.playerList)
        {
            if (boltEntity.IsOwner)
            {
                var player = boltEntity.GetComponent<Player>();
                var fireEvent = FireBulletEvent.Create();
                fireEvent.Damage = damage;
                fireEvent.Speed = speed;
                fireEvent.SpawnLocation = player.entity.transform.position;
                fireEvent.Entity = player.entity;
                fireEvent.Send();
            }
        }
    }
}
