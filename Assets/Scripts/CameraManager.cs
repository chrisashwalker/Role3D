using UnityEngine;

public static class CameraManager{
    public static Camera MainCamera{get;set;}
    public static float standardCameraSize{get;set;}
    public static bool cameraIsStandardSized{get;set;} = true;

    public static void CameraFollow(){
        MainCamera.transform.position = new Vector3(GameController.Instance.Player.Rigidbody.position.x - 7.5f, GameController.Instance.Player.Rigidbody.position.y + 10f, GameController.Instance.Player.Rigidbody.position.z - 15.0f);
    }

    public static void MapToggle(){
        if (cameraIsStandardSized){
            MainCamera.orthographicSize = standardCameraSize * 4;
            cameraIsStandardSized = false;
            } else {
            MainCamera.orthographicSize = standardCameraSize;
            cameraIsStandardSized = true;
        }
    }
}
