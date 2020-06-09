using UnityEngine;

public class Shield : Item
{
    public Shield(string type, float uses, float dur)
    {
        this.type = type;
        this.uses = uses;
        duration = dur;
    }

    public override void UseItem()
    {
        foreach (var boltEntity in BoltManager.Instance.playerList)
        {
            if (boltEntity.IsOwner)
            {
                var player = boltEntity.GetComponent<Player>();
                var spawnLocation = player.entity.transform.position + (player.entity.transform.forward.normalized * 0.2f);
                var shield = BoltNetwork.Instantiate(BoltPrefabs.ShieldBlocker, spawnLocation, Quaternion.LookRotation(player.entity.transform.forward));
            }
        }
    }
}