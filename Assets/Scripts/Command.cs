using UnityEngine;

public abstract class Command
{
    public Fruit Fruit;
    protected FruitData _cache;
    public Fruit CreateNewFruit(FruitData data)
    { 
        var newFruit = GameObject.Instantiate(Manager.manager.fruitPrefabs[data.Size - 1]).GetComponent<Fruit>();
        newFruit.Data = data;
        newFruit.transform.parent = Manager.manager.transform;
        return newFruit;
    }

    protected Fruit Restore()
    {
        return CreateNewFruit(_cache);
    }

    public abstract void Execute();
    public abstract void Undo();
}