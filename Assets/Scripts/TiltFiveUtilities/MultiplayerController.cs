/*
 * Copyright (C) 2020-2023 Tilt Five, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System;

[DefaultExecutionOrder(-1000)]
public class MultiplayerController : MonoBehaviour
{ 
    //The default Character prefab to spawn
    public GameObject defaultCharacter;
    public BattleManager bm;
    public GlassesDisconnectManager dm;

    public int numPlayers = 0;

    //The color for each player indicator.
    public List<Material> playerIndicatorColors;

    //The color for each player indicator.
    public List<Material> playerSelectorColors;

    //This maintains a list of actively connected Players.
    public Dictionary<TiltFive.PlayerIndex, PlayerController> activePlayers;

    public Dictionary<TiltFive.PlayerIndex, ControllableObject> activeObjects;

    public static Dictionary<TiltFive.PlayerIndex, GameObject> currentCharacters;

    //The set of Layers each player should have exclusively visible to them.
    public Dictionary<TiltFive.PlayerIndex, string> playerLayers;

    //The UI Prefab to pass to the Player.
    public GameObject uiCore;
    public GameObject secondUiCore;
    public GameObject displayText;

    public int totalDead = 0;
    public int winningNumber = 0;

    public static MultiplayerController instance = null;

    public static bool scene1Loaded = false;
    public static bool scene2Loaded = false;
    public bool gameEnded = false;
    public bool allPlayersLoaded = false;
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        activePlayers = new Dictionary<TiltFive.PlayerIndex, PlayerController>();
        activeObjects = new Dictionary<TiltFive.PlayerIndex, ControllableObject>();
        if(currentCharacters == null)
        {
            currentCharacters = new Dictionary<TiltFive.PlayerIndex, GameObject>();
        }
        playerLayers = new Dictionary<TiltFive.PlayerIndex, string>();

        playerLayers.Add(TiltFive.PlayerIndex.One, "Player1");
        playerLayers.Add(TiltFive.PlayerIndex.Two, "Player2");
        playerLayers.Add(TiltFive.PlayerIndex.Three, "Player3");
        playerLayers.Add(TiltFive.PlayerIndex.Four, "Player4");
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (!scene1Loaded)
            {
                scene1Loaded = true;
                displayText = GameObject.Find("DisplayText");
                bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
            }
            bool playersReady = true;
            
            foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
            {
                if (p.Value.selectedCharacter == null && !p.Value.isSpectator)
                {
                    playersReady = false;
                }
            }
            if (playersReady)
            {
                displayText.GetComponent<TMP_Text>().text = "Press 2 to begin";
            }
            else
            {
                displayText.GetComponent<TMP_Text>().text = "Press A to select character";
            }
            
        }
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            if (!scene2Loaded && !gameEnded)
            {
                if (!allPlayersLoaded)
                {
                    foreach(KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
                    {
                        if(p.Value == null)
                        {
                            return;
                        }
                    }
                    allPlayersLoaded = true;
                }
                else
                {
                    dm = GlassesDisconnectManager.Instance;
                    bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
                    foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
                    {
                        if (p.Value.selectedCharacter != null)
                        {
                            dm.AddPlayerToWatch(p.Key);
                            p.Value.BeginSpawn();
                        }
                        p.Value.bm = bm;
                        p.Value.uiCore = secondUiCore;
                        p.Value.SpawnUI();
                    }
                    scene2Loaded = true;
                }
            }
            else
            {
                if (!gameEnded)
                {
                    if (checkWin())
                    {
                        gameEnded = true;
                        foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
                        {
                            if (p.Value.currentLives > 0 && p.Value.selectedCharacter != null)
                            {
                                winningNumber = (int)p.Value.playerNumber;
                            }
                        }

                        foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
                        {
                            UIController cont = p.Value.uiReference.GetComponent<UICore>() as UIController;
                            cont.ActivateEndText(winningNumber);
                        }
                    }
                }
            }
            
        }
    }

    public bool checkWin()
    {
        totalDead = 0;
        foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
        {
            if(p.Value.currentLives <= 0 || p.Value.selectedCharacter == null)
            {
                totalDead++;
            }
        }
        if(totalDead >= activePlayers.Count - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DeletePlayers()
    {
        foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
        {
            Destroy(p.Value);
        }
    }
    public void DeleteObjects()
    {
        foreach (KeyValuePair<TiltFive.PlayerIndex, ControllableObject> p in activeObjects)
        {
            Destroy(p.Value);
        }
    }

    public void StartGame()
    {
        currentCharacters.Clear();
        numPlayers = activePlayers.Count;
        scene2Loaded = false;
        foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> p in activePlayers)
        {
            currentCharacters.Add(p.Key, p.Value.selectedCharacter);
        }
        SceneManager.LoadScene(2);
    }

    public void ResetGame()
    {
        scene2Loaded = false;
        SceneManager.LoadScene(2);
    }

    public void SpawnCharacter(TiltFive.PlayerIndex p, PlayerController playerObject)
    {
        playerObject.uiCore = uiCore;
        playerObject.StartCoroutine("SpawnUI");
        GameObject spawnedChar = Instantiate(defaultCharacter, bm.spawnPoints[((int)p - 1)].position, Quaternion.identity);
        var component = spawnedChar.GetComponent<ControllableObject>();
        playerObject.controlledCharacter = component;
        playerObject.controlledCharacter.controller = playerObject;
        activeObjects.Add(p, component);
        return;
    }
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        foreach (TiltFive.PlayerIndex p in Enum.GetValues(typeof(TiltFive.PlayerIndex)))
        {
            if (TiltFive.Player.IsConnected(p))
            {
                if (!(activePlayers.ContainsKey(p)))
                {
                    PlayerController playerOne = playerInput.gameObject.AddComponent<PlayerController>();
                    playerOne.playerNumber = p;
                    activePlayers.Add(p, playerOne);
                    playerOne.bm = bm;
                    if (SceneManager.GetActiveScene().buildIndex == 1)
                    {
                        SpawnCharacter(p, playerOne);
                        GlassesDisconnectManager.Instance.AddPlayerToWatch(p);
                    }
                    else if(SceneManager.GetActiveScene().buildIndex == 2)
                    {
                        if(currentCharacters[playerOne.playerNumber] != null)
                        {
                            playerOne.selectedCharacter = currentCharacters[playerOne.playerNumber];
                            GlassesDisconnectManager.Instance.AddPlayerToWatch(p);
                        }
                    }
                    return;
                }
            }
        }
        
    }
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        var p = playerInput.gameObject.GetComponent<PlayerController>().playerNumber;
        activePlayers.Remove(p);
        activeObjects.Remove(p);
        return;
    }
}
