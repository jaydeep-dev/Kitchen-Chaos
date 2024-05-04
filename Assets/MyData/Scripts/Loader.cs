using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenu,
        Gameplay,
        Loader
    }

    private static Scene targetScene;

    public static void LoadScene(Scene scene)
    {
        targetScene = scene;

        Debug.Log($"<<Loader>> Loading scene: {targetScene}");
        SceneManager.LoadScene(Scene.Loader.ToString());
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
