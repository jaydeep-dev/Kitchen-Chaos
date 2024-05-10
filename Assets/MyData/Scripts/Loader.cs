using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenu,
        Gameplay,
        Loader,
        Lobby,
        CharacterSelection,
    }

    private static Scene targetScene;

    public static void LoadScene(Scene scene)
    {
        targetScene = scene;

        Debug.Log($"<<Loader>> Loading scene: {targetScene}");
        SceneManager.LoadScene(Scene.Loader.ToString());
    }

    public static void LoadSceneViaNetwork(Scene scene)
    {
        Debug.Log($"<<Loader>> Loading scene via network: {scene}");
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
