using Assets.Scripts.Singleton;
using UnityEngine;

public class GameManager : PersistenSingleton<GameManager> {

    public void ContinueGame() {
        Debug.Log("Continuing Game...");
        // Add logic to continue the game from the game over scene
    }
}

