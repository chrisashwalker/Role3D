using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour{
    public static GameController Instance{get;set;}
    public UnityCharacter Player{get;set;}
    private int RegenDays = 3;
    public GameObject ShortcutCanvas{get;set;}
    public GameObject[] AllShortcutToggles{get;set;}
    public Animator anim;
    public GameObject HealthBar;

    void Awake(){
        Instance = this;
        CameraManager.MainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        CameraManager.standardCameraSize = CameraManager.MainCamera.orthographicSize;
        TimeManager.Sunlight = GameObject.FindWithTag("Sunlight").GetComponent<Light>();
        Actions.InteractiveLayer = LayerMask.GetMask("Interactive");
        if (World.SceneList.Count == 0){
            World.BuildScenes();
        }
        if (Items.GameItemList.Count == 0){
            Items.GetItems();
        }
        Saves.GameData = Saves.LoadGame();
        if (Saves.GameData != null){
            if (Saves.Loaded == false){
                if (World.SceneList[Saves.GameData.CurrentLocation] != SceneManager.GetActiveScene().name){
                    SceneManager.LoadScene(World.SceneList[Saves.GameData.CurrentLocation]);
                }
                Saves.Loaded = true;
            }
            World.FindCharacters();
            Player.Storage.StoredItems = Saves.GameData.InventoryItems;
        } else {
            World.FindCharacters();
            Saves.GameData = new Saves.SaveData();
            Saves.GameData.GameDay = 1;
            Saves.GameData.GameTime = 300.0f;
            Saves.GameData.Progress = 1;
            Saves.GameData.CurrentLocation = 1;
            Items.LoadStandardItems();
        }
        Player.Storage.EquippedItemIndex = 0;
        ShortcutCanvas = GameObject.FindWithTag("ShortcutCanvas");
        AllShortcutToggles = GameObject.FindGameObjectsWithTag("ShortcutToggle");
        Items.UpdateToggles();
        World.FindObjects();
    }

    void Start(){
        List<AlteredObject> SpentRemovals = new List<AlteredObject>();
        foreach (AlteredObject po in Saves.GameData.AlteredObjects){
            if (po.Scene == SceneManager.GetActiveScene().name){
                if (po.Change == "Addition"){
                    GameObject loadedPo;
                    if (po.DaysAltered >= RegenDays){
                        loadedPo = Instantiate(Resources.Load("Plants/" + po.endPrefab, typeof(GameObject))) as GameObject;
                    } else {
                    loadedPo = Instantiate(Resources.Load("Plants/" + po.startPrefab, typeof(GameObject))) as GameObject;
                    }
                    po.Identifier = loadedPo.GetInstanceID();
                    loadedPo.transform.position = new Vector3(po.PositionX, po.PositionY, po.PositionZ);
                } else if (po.Change == "Removal"){
                    Vector3 poPosition = new Vector3(po.PositionX, po.PositionY, po.PositionZ);
                    if (po.startPrefab == "MapItem"){
                        foreach (UnityMapItem mapItem in World.MapItemList){
                            if (mapItem.Object.transform.position.x == poPosition.x && mapItem.Object.transform.position.z == poPosition.z){
                                GameObject.Destroy(mapItem.Object);
                                World.MapItemList.Remove(mapItem);
                                break;
                            }
                        }
                    } else if (po.DaysAltered < RegenDays){
                        if (po.startPrefab == "Tree"){
                            foreach (GameObject tree in World.TreeList){
                                if (tree.transform.position.x == poPosition.x && tree.transform.position.z == poPosition.z){
                                    GameObject.Destroy(tree);
                                    World.TreeList.Remove(tree);
                                    break;
                                }
                            }
                        } else if (po.startPrefab == "Rock"){
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
        HealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(10 * Player.MaxHealth, 10);
        HealthBar.GetComponent<Slider>().maxValue = Player.MaxHealth;
        HealthBar.GetComponent<Slider>().value = Player.Health;
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
        Items.ItemUseCheck();
        foreach (UnityProjectile projectile in Actions.ShotProjectiles){
            if ((projectile.Rigidbody.transform.position - projectile.Origin).magnitude >= projectile.Distance || ((projectile.Rigidbody.transform.position - projectile.Origin).magnitude >= 0.1 && (projectile.Rigidbody.velocity - Vector3.zero).magnitude <= 1)){
                Actions.SpentProjectiles.Add(projectile);
            }
        }
        foreach (UnityProjectile projectile in Actions.SpentProjectiles){
            Actions.ShotProjectiles.Remove(projectile);
            GameObject.Destroy(projectile.Object);
        }
        Actions.SpentProjectiles.Clear();
        foreach (UnityCharacter enemy in World.EnemyList){
            Actions.FollowCharacter(Player, enemy);
            if (enemy.Health <= 0){
                World.DefeatedEnemyList.Add(enemy);
            }
        }
        foreach (UnityCharacter defeated in World.DefeatedEnemyList){
            World.EnemyList.Remove(defeated);
            GameObject.Destroy(defeated.Object);
        }
        HealthBar.GetComponent<Slider>().value = GameController.Instance.Player.Health;
        World.DefeatedEnemyList.Clear();
        if (Player.Health <= 0){
            Actions.ShotProjectiles.Clear();
            Actions.SpentProjectiles.Clear();
            World.EnemyList.Clear();
            SceneManager.LoadScene(World.SceneList[Saves.GameData.CurrentLocation]);
        }
    }
}
