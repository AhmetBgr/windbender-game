using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDebug : MonoBehaviour
{
    public static bool allLevelsUnlocked = false;

    private void LateUpdate() {
        if (Input.GetKeyDown(KeyCode.U)) {
            UnlockAllLevels();
        }
    }

    public static void UnlockAllLevels() {

        allLevelsUnlocked = true;
        
        SceneLoader.LoadSceneWithName("-OverWorld");
    }
}
