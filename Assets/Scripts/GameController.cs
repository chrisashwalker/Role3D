using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour{
    public static GameController Instance{get;set;}
    public UnityCharacter Player{get;set;}
    public int RegenDays = 3;
    public GameObject FullCanvas{get;set;}
    public GameObject[] AllItemToggles{get;set;}

    void Awake(){
        Instance = this;
        CameraManager.MainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        CameraManager.standardCameraSize = CameraManager.MainCamera.orthographicSize;
        TimeManager.Sunlight = GameObject.FindWithTag("Sunlight").GetComponent<Light>();
        Actions.InteractiveLayer = LayerMask.GetMask("Interactable");
        if (World.SceneList.Count == 0){
            World.BuildScenes();
        }
        if (Inventory.GameItemList.Count == 0){
            Inventory.LoadGameItems();
        }
        Saves.GameData = Saves.LoadGame();
        if (Saves.GameData != null){
            if (Saves.Loaded == false){
                if (World.SceneList[Saves.GameData.CurrentLocation] != SceneManager.GetActiveScene().name){
                    SceneManager.LoadScene(World.SceneList[Saves.GameData.CurrentLocation]);
                }
                Saves.Loaded = true;
            }
            Inventory.StoredItems = Saves.GameData.InventoryItems;
        } else {
            Saves.GameData = new Saves.SaveData();
            Saves.GameData.GameDay = 1;
            Saves.GameData.GameTime = 0.0f;
            Saves.GameData.FarthestLocation = 1;
            Saves.GameData.CurrentLocation = 1;
            Inventory.LoadStandardItems();
        }
        Inventory.EquippedItemIndex = 0;
        FullCanvas = GameObject.FindWithTag("FullCanvas");
        AllItemToggles = GameObject.FindGameObjectsWithTag("ItemToggle");
        Inventory.UpdateToggles();
        World.FindCharacters();
        World.FindObjects();
    }

    void Start(){
        List<AlteredObject> SpentRemovals = new List<AlteredObject>();
        foreach (AlteredObject po in Saves.GameData.AlteredObjects){
            if (po.Scene == SceneManager.GetActiveScene().name){
                if (po.Change == "Addition"){
                GameObject loadedPo = Instantiate(Resources.Load(po.Prefab, typeof(GameObject))) as GameObject;
                po.Identifier = loadedPo.GetInstanceID();
                loadedPo.transform.position = new Vector3(po.PositionX, po.PositionY, po.PositionZ);
                } else if (po.Change == "Removal"){
                    Vector3 poPosition = new Vector3(po.PositionX, po.PositionY, po.PositionZ);
                    if (po.Prefab == "MapItem"){
                        foreach (UnityMapItem mapItem in World.MapItemList){
                            if (mapItem.Object.transform.position.x == poPosition.x && mapItem.Object.transform.position.z == poPosition.z){
                                GameObject.Destroy(mapItem.Object);
                                World.MapItemList.Remove(mapItem);
                                break;
                            }
                        }
                    } else if (po.DaysAltered < RegenDays){
                        if (po.Prefab == "Tree"){
                            foreach (GameObject tree in World.TreeList){
                                if (tree.transform.position.x == poPosition.x && tree.transform.position.z == poPosition.z){
                                    GameObject.Destroy(tree);
                                    World.TreeList.Remove(tree);
                                    break;
                                }
                            }
                        } else if (po.Prefab == "Rock"){
                            foreach (GameObject rock in World.RockList){
                                if (rock.transform.position.x == poPosition.x && rock.transform.position.z == poPosition.z){
                                    GameObject.Destroy(rock);
                                    World.RockList.Remove(rock);
                                    break;
                                }
                            }
                        }
                    } else {
                        SpentRemovals.Add(po);
                    }
                }
            }
        }
        foreach (AlteredObject po in SpentRemovals){
            Saves.GameData.AlteredObjects.Remove(po);
        }
        SpentRemovals.Clear();
    }

    void FixedUpdate(){
        Controls.MoveCharacter(Player);
        CameraManager.CameraFollow();
        Actions.FindTarget();
    }

    void Update(){
        TimeManager.ClockTick();
        if (Input.GetKeyDown(Controls.MapZoom)){
            CameraManager.MapToggle();
        } else if (Input.GetKeyDown(KeyCode.Alpha1)){
            World.FastTravel(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)){
            World.FastTravel(2);
        }
        Inventory.ItemUseCheck();
        foreach (UnityProjectile projectile in Actions.ShotProjectiles){
            if ((projectile.Rigidbody.transform.position - projectile.Origin).magnitude >= projectile.Distance){
                Actions.SpentProjectiles.Add(projectile);
            }
        }
        foreach (UnityProjectile projectile in Actions.SpentProjectiles){
            Actions.ShotProjectiles.Remove(projectile);
            GameObject.Destroy(projectile.Object);
        }
        Actions.SpentProjectiles.Clear();
    }
}
