using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Map
{    
    public static UnityCharacter Player{get;set;}
    public static List<UnityCharacter> Characters{get;set;}
    public static List<UnityCharacter> Enemies{get;set;}
    public static List<UnityGate> GateList{get;set;} = new List<UnityGate>();
    public static List<UnityMapItem> MapItemList{get;set;} = new List<UnityMapItem>();
    
    public static List<UnityCharacter> DefeatedEnemyList{get;set;} = new List<UnityCharacter>();
    public static List<GameObject> Trees{get;set;} = new List<GameObject>();
    public static List<GameObject> Rocks{get;set;} = new List<GameObject>();

    public static void GetCharacters(){
        Player = new UnityCharacter(GameObject.FindGameObjectWithTag("Player"));
        Characters = new List<UnityCharacter>();
        foreach (GameObject character in GameObject.FindGameObjectsWithTag("Character")){
            UnityCharacter newCharacter = new UnityCharacter(character);
            Characters.Add(newCharacter);
        }
        Enemies = new List<UnityCharacter>();
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")){
            UnityCharacter newEnemy = new UnityCharacter(enemy);
            Enemies.Add(newEnemy);
        }
    }

    public static void GetObjects(){
        Rocks.Clear();
        Trees.Clear();
        foreach (GameObject rock in GameObject.FindGameObjectsWithTag("Rock")){
            if (Rocks.Contains(rock) == false){
                Rocks.Add(rock);
            }
        }
        foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree")){
            if (Trees.Contains(tree) == false){
                Trees.Add(tree);
            }
        }
    }

    public static void DiscardAllObjects()
    {
        Map.ShotProjectiles.Clear();
        Map.SpentProjectiles.Clear();
        Map.Characters.Clear();
        Map.Enemies.Clear();
    }

    public static void FastTravel(int sceneNumber){
        if (Saves.GameData.Progress >= sceneNumber && Saves.GameData.CurrentLocation != sceneNumber){ 
            Saves.GameData.CurrentLocation = sceneNumber;
            World.RockList.Clear();
            World.TreeList.Clear();
            World.MapItemList.Clear();
            SceneManager.LoadScene(Scenes[sceneNumber], LoadSceneMode.Single);
            Saves.GameData.InventoryItems = GameController.Instance.Player.Storage.StoredItems;
            Saves.GameData.Funds = GameController.Instance.Player.Coins;
            Saves.SaveGame(Saves.GameData);
        }
    }
}

public enum BarrierTypes{
    Barrier,
    Gate
}

public abstract class Barrier{
    public BarrierTypes Type{get;set;}
    public int Durability{get;set;}

    public Barrier(int setDurability = 0){
        Type = BarrierTypes.Barrier;
        Durability = setDurability;
    }
}

public abstract class Gate : Barrier{
    public int Destination{get;set;}
    public bool Locked{get;set;}

    public Gate(bool setLocked = false){
        Type = BarrierTypes.Gate;
        Locked = setLocked;
    }
}

public class UnityBarrier : Barrier{
    public GameObject Object{get;set;}
    public Rigidbody Rigidbody{get;set;}
    public Collider Collider{get;set;}
    public int Identifier{get;set;}

    public UnityBarrier(string barrierPrefab){
        Object = (GameObject) GameObject.Instantiate(Resources.Load("Objects/" + barrierPrefab, typeof(GameObject)));
        Rigidbody = Object.GetComponent<Rigidbody>();
        Collider = Object.GetComponent<Collider>();
        Identifier = Object.GetInstanceID();
    }
}

public class UnityGate : Gate{
    public GameObject Object{get;set;}
    public Rigidbody Rigidbody{get;set;}
    public Collider Collider{get;set;}
    public int Identifier{get;set;}

    public UnityGate(GameObject gate){
        Object = gate;
        Rigidbody = Object.GetComponent<Rigidbody>();
        Collider = Object.GetComponent<Collider>();
        Identifier = Object.GetInstanceID();
    }
}

public class UnityMapItem{
    public GameObject Object{get;set;}
    public Rigidbody Rigidbody{get;set;}
    public Collider Collider{get;set;}
    public int Identifier{get;set;}
    public Item linkedItem{get;set;}

    public UnityMapItem(GameObject mapItem, Item item){
        Object = mapItem;
        Rigidbody = Object.GetComponent<Rigidbody>();
        Collider = Object.GetComponent<Collider>();
        Identifier = Object.GetInstanceID();
        linkedItem = item;
    }
}

[System.Serializable]
public class AlteredObject{
    public string Change{get;set;}
    public string startPrefab{get;set;}
    public string endPrefab{get;set;}
    public string Scene{get;set;}
    public float PositionX{get;set;}
    public float PositionY{get;set;}
    public float PositionZ{get;set;}
    public int DaysAltered{get;set;}
    public int Identifier{get;set;}

    public AlteredObject(string setChange, string setPrefab, Scene setScene, Vector3 setPosition, int setIdentifier, int setDaysAltered = 0){
        Change = setChange;
        startPrefab = setPrefab.Replace("(Clone)","");
        endPrefab = startPrefab.Replace("Start","End");
        Scene = setScene.name;
        PositionX = setPosition.x;
        PositionY = setPosition.y;
        PositionZ = setPosition.z;
        DaysAltered = setDaysAltered;
        Identifier = setIdentifier;
    }
}