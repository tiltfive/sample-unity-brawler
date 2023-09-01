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

public class PauseTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public TMPro.TMP_Text pauseWarning;
    void Start()
    {
        GlassesDisconnectManager.OnDisconnect += OnDisconnect;
        GlassesDisconnectManager.OnReconnect += OnReconnect;
        this.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        GlassesDisconnectManager.OnDisconnect -= OnDisconnect;
        GlassesDisconnectManager.OnReconnect -= OnReconnect;
    }

    public void OnDisconnect()
    {
        this.gameObject.SetActive(true);
        pauseWarning.text = "Glasses or Wand disconnected. Please reconnect to continue.";
        Time.timeScale = 0.0f;
    }

    public void OnReconnect()
    {
        pauseWarning.text = "Press the T5 Button to unpause.";
        this.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
