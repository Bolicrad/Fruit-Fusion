using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using Vector3 = UnityEngine.Vector3;

public class Fruit : MonoBehaviour
{
    // Start is called before the first frame update
    public FruitData Data;

    public int PubState => (int)Data.State;


    void Awake()
    {

    }
    

    void Start()
    {
        transform.position = Data.Pos;
    }

    // Update is called once per frame
    void Update()
    {
        switch (Data.State)
        {
            case FruitState.Waiting:
            {
                if (Input.GetMouseButtonDown(0)) Data.State = FruitState.Moving;
                break;
            }
            case FruitState.Moving:
            {
                if (Input.GetMouseButtonUp(0))
                {
                    Data.Command.Execute();
                    Data.State = FruitState.Colliding;
                }
                else
                {
                    Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (Math.Abs(mouse.x) > 3.3) mouse = Vector3.right * Math.Sign(mouse.x) * 3.3f;
                    transform.position = new Vector3(mouse.x, transform.position.y);
                }

                break;
            }
            case FruitState.Colliding:
                break;
            case FruitState.Done:
            {
                break;
            }
            default: break;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Fruit") )
        {
            var otherFruit = col.gameObject.GetComponent<Fruit>();
            if (otherFruit.Data.Size == this.Data.Size 
                && Data.Size <= 10 
                && otherFruit.Data.State>FruitState.Moving 
                && Data.State > FruitState.Moving)
            {
                if (col.gameObject.GetHashCode() < gameObject.GetHashCode())
                {
                    var command = new CombineFruitCommand(
                        col.gameObject.GetComponent<Fruit>(), 
                        GetComponent<Fruit>());
                    if(!Manager.manager.replaymode)Manager.manager.Commands.AddLast(command);
                    command.Execute();
                }
            }
        }
        if ((col.gameObject.CompareTag("Floor")||col.gameObject.CompareTag("Fruit") )
            && Data.State < FruitState.Done
           )
        {
            Data.State = FruitState.Done;
            Manager.manager.CreateFruit();
        }
    }
}
public struct FruitData
{
    public int Size;
    public Vector3 Pos;
    public FruitState State;
    public Command Command;
}

public enum FruitState{
    Waiting = 0,
    Moving,
    Colliding,
    Done
}
