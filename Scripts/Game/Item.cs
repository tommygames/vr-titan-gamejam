
public abstract class Item
{
    public string type;
    protected float uses;
    protected float duration;
    protected int damage;
    protected float speed;
    public float fireRate;
    
    public abstract void UseItem();

    public void DecrementUses()
    {
        uses -= 1;
    }
    public bool IsOutOfUses()
    {
        return uses < 1;
    }
}
