using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Manager : MonoBehaviour
{
    public static Manager manager;
    public LinkedList<Command> Commands;
    private LinkedListNode<Command> currentNode;
    public Fruit CurrentFruit;
    public bool replaymode = false;
    public double timeTag = 0f;
    public List<GameObject> fruitPrefabs;


    private void Awake()
    {
        manager = this;
        Commands = new LinkedList<Command>();
        timeTag = Time.realtimeSinceStartupAsDouble;
    }

    void Start()
    {
        CreateFruit();
    }
    
    void Update()
    {
        
    }
    

    public void CreateFruit()
    {
        if (!replaymode)
        {
            var command = new DropFruitCommand();
            Commands.AddLast(command);
            CurrentFruit = command.Fruit;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void Recording()
    {
        if (Commands.Count > 0) StartCoroutine(Replay());
    }

    public IEnumerator Replay()
    {
        foreach (Fruit child in transform.GetComponentsInChildren<Fruit>())
        {
            Destroy(child.gameObject);
        }

        replaymode = true;
        
        while (Commands.Last.Value.ToString()!="DropFruit")
        {
            Commands.RemoveLast();
        }
        if(Commands.Last.Value.ToString()=="DropFruit")Commands.RemoveLast();

        foreach (var command in Commands)
        {
            if (command.ToString() == "DropFruit")
            {
                yield return new WaitForSecondsRealtime((float)((DropFruitCommand)command).TimeSpan);
                command.Execute();
            }
        }
    }
}

public class DropFruitCommand : Command
{
    public override string ToString()
    {
        return "DropFruit";
    }

    public double TimeSpan = 0;

    public Queue<FruitData> SnapShot;
    public DropFruitCommand()
    {
        _cache = new FruitData
        {
            Size = Random.Range(1,5),
            Pos = Vector3.zero,
            Command = this,
            State = FruitState.Waiting
        };
        Fruit = Restore();
        Fruit.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
    }

    public override void Execute()
    {
        if (Fruit == null)
        {
            Fruit = Restore();
        }
        if (!Manager.manager.replaymode)
        {
            TimeSpan = Time.realtimeSinceStartupAsDouble - Manager.manager.timeTag;
            Manager.manager.timeTag = Time.realtimeSinceStartupAsDouble;
        }
        _cache.Pos = Fruit.transform.position;
        Fruit.Data.State = FruitState.Colliding;
        Fruit.GetComponent<Rigidbody2D>().gravityScale = 1.0f;
    }

    public override void Undo()
    {
        Debug.Log(Fruit.gameObject.name + ": "+Fruit.PubState);
        foreach (var fruit in Manager.manager.GetComponentsInChildren<Fruit>())
        {
            fruit.transform.position = fruit.Data.Pos;
            fruit.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        Fruit.Data.State = FruitState.Waiting;
        Fruit.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
    }
}

public class CombineFruitCommand : Command
{
    private Fruit _father;
    private Fruit _mother;
    private FruitData _fatherCache;
    private FruitData _motherCache;

    public override string ToString()
    {
        return "CombineFruit";
    }

    public CombineFruitCommand(Fruit father, Fruit mother)
    {
        _father = father;
        _mother = mother;
    }

    public override void Execute()
    {
        _cache = new FruitData
        {
            Size = _father.Data.Size + 1,
            Pos = new Vector3(
                (_father.transform.position.x+_mother.transform.position.x)/2, 
                (_father.transform.position.y+_mother.transform.position.y)/2), 
            State = FruitState.Done,
            Command = this
        };
        if (_cache.Size > 13)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        Fruit = Restore();
        _fatherCache = _father.Data;
        _motherCache = _mother.Data;
        GameObject.Destroy(_father.gameObject);
        GameObject.Destroy(_mother.gameObject);
    }

    public override void Undo()
    {
        Debug.Log(Fruit.name + ": "+Fruit.Data.State);
        _cache = Fruit.Data;
        GameObject.Destroy(Fruit.gameObject);
        _father = CreateNewFruit(_fatherCache);
        _mother = CreateNewFruit(_motherCache);
    }
}
