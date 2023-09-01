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
using TMPro;

public class BattleManager : MonoBehaviour
{
    public List<Transform> spawnPoints;

    public GameObject pauseMenu1;
    public GameObject pauseMenu2;
    public GameObject pauseMenu3;
    public GameObject pauseMenu4;

    public bool isPaused = false;

    public MultiplayerController mc;

    public void PauseGame()
    {
        isPaused = true;
        pauseMenu1.SetActive(true);
        pauseMenu2.SetActive(true);
        pauseMenu3.SetActive(true);
        pauseMenu4.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void UnpauseGame()
    {
        isPaused = false;
        pauseMenu1.SetActive(false);
        pauseMenu2.SetActive(false);
        pauseMenu3.SetActive(false);
        pauseMenu4.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
