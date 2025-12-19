using System;
using UnityEngine;

public class LevelManager : MonoBehaviour , IGameStateListener
{
    public static LevelManager instance;

    [Header("Data")]
    [SerializeField] private Level[] levels;
    private const string levelKey = "LevelReached";
    private int levelIndex;
    public Item[] Items => currentLevel.GetItems();
    public Transform ItemParent => currentLevel.ItemParent;


    [Header("Settings")]
    private Level currentLevel;

    [Header("Action")]
    public static Action<Level> levelSpawned;



    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);

        LoadData();
    }


    private void SpawnLevel()
    {
        transform.Clear();

        int validatedLevelIndex = levelIndex % levels.Length;

        currentLevel = Instantiate(levels[validatedLevelIndex], transform);

        levelSpawned?.Invoke(currentLevel);
    }


    private void LoadData()
    {
        levelIndex = PlayerPrefs.GetInt(levelKey);
    }


    private void SaveData()
    {
        PlayerPrefs.SetInt(levelKey, levelIndex);
    }

    public void GameStateChangedCallBack(EGameState gameState)
    {
        if (gameState == EGameState.GAME)
            SpawnLevel();
        else if (gameState == EGameState.LEVELCOMPLETE)
        {
            levelIndex++;
            SaveData();
        }
    }

}

