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
using TiltFive;
using UnityEngine;

public class GlassesDisconnectManager : SingletonComponent<GlassesDisconnectManager>
{
    [SerializeField] bool logging;

    public static event System.Action OnDisconnect;
    public static event System.Action OnReconnect;
    public static bool AnyDisconnected { get; private set; }

    public List<PlayerIndex> playersToWatch = new List<PlayerIndex>();
    public List<PlayerIndex> wandsToWatch = new List<PlayerIndex>();
    public List<PlayerIndex> disconnectedPlayers = new List<PlayerIndex>();
    public List<PlayerIndex> disconnectedWands = new List<PlayerIndex>();

    void Update()
    {
        // Detect new wands to watch
        foreach (PlayerIndex player in playersToWatch)
        {
            if (!wandsToWatch.Contains(player))
            {
                if (Wand.TryCheckConnected(out bool connected, player, ControllerIndex.Right) && connected)
                {
                    wandsToWatch.Add(player);
                    if (logging) Debug.Log($"GlassesDisconnectManager: Added wand to watch (Player {player})");
                }
            }
        }

        // Detect player connection changes
        foreach (PlayerIndex player in playersToWatch)
        {
            if (Player.IsConnected(player))
            {
                if (disconnectedPlayers.Contains(player))
                {
                    disconnectedPlayers.Remove(player);
                    AlertReconnect();
                    if (logging) Debug.Log($"GlassesDisconnectManager: Player reconnected (Player {player})");
                }
            }
            else if (!disconnectedPlayers.Contains(player))
            {
                disconnectedPlayers.Add(player);
                AlertDisconnect();
                if (logging) Debug.Log($"GlassesDisconnectManager: Player disconnected (Player {player})");
            }
        }

        // Detect wand connection changes
        foreach (PlayerIndex player in wandsToWatch)
        {
            if (Wand.TryCheckConnected(out bool connected, player, ControllerIndex.Right) && connected)
            {
                if (disconnectedWands.Contains(player))
                {
                    disconnectedWands.Remove(player);
                    AlertReconnect();
                    if (logging) Debug.Log($"GlassesDisconnectManager: Wand reconnected (Player {player})");
                }
            }
            else if(!disconnectedWands.Contains(player))
            {
                disconnectedWands.Add(player);
                AlertDisconnect();
                if (logging) Debug.Log($"GlassesDisconnectManager: Wand disconnected (Player {player})");
            }
        }
    }

    void AlertDisconnect()
    {
        if (disconnectedPlayers.Count + disconnectedWands.Count == 1)
        {
            AnyDisconnected = true;
            OnDisconnect?.Invoke();
        }
    }

    void AlertReconnect()
    {
        if (disconnectedPlayers.Count + disconnectedWands.Count == 0)
        {
            AnyDisconnected = false;
            OnReconnect?.Invoke();
        }
    }

    public void AddPlayerToWatch(PlayerIndex playerToWatch)
    {
        if (!playersToWatch.Contains(playerToWatch))
        {
            playersToWatch.Add(playerToWatch);
            if (logging) Debug.Log($"GlassesDisconnectManager: Added player to watch (Player {playerToWatch})");
        }
    }

    public void RemovePlayerToWatch(PlayerIndex playerToWatch)
    {
        if (playersToWatch.Contains(playerToWatch))
        {
            playersToWatch.Remove(playerToWatch);
            if (logging) Debug.Log($"GlassesDisconnectManager: Removed player to watch (Player {playerToWatch})");
        }
    }
}