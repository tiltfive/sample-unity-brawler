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

public class UIController : UICore
{
    public List<TMP_Text> playerPercentages;

    public GameObject winningText;
    public GameObject continueText;

    private void Start()
    {

        //winningText.meshRenderer.enabled = false;
        winningText.SetActive(false);
        continueText.SetActive(false);
    }

    public void ActivateEndText(int winningNumber)
    {

        winningText.SetActive(true);
        continueText.SetActive(true);
        winningText.GetComponent<TMP_Text>().text = "Player " + winningNumber + " wins! ";
    }

    new private void LateUpdate()
    {
        base.LateUpdate();
        if (currentPlayer)
        {
            //Display HP and lives only for connected players
            if (mc.activePlayers.ContainsKey(TiltFive.PlayerIndex.One) && mc.activePlayers[TiltFive.PlayerIndex.One].selectedCharacter != null)
            {
                    playerPercentages[0].gameObject.SetActive(true);
                    playerPercentages[0].text = mc.activePlayers[TiltFive.PlayerIndex.One].damagePercentage.ToString("F1") + "%";
                    for (int i = 0; i < 3 - mc.activePlayers[TiltFive.PlayerIndex.One].currentLives; i++)
                    {
                        playerPercentages[0].gameObject.transform.GetChild(2 - i).gameObject.SetActive(false);
                    }
            }
            else
            {
                playerPercentages[0].gameObject.SetActive(false);
            }
            if (mc.activePlayers.ContainsKey(TiltFive.PlayerIndex.Two) && mc.activePlayers[TiltFive.PlayerIndex.Two].selectedCharacter != null)
            {
                    playerPercentages[1].gameObject.SetActive(true);
                    playerPercentages[1].text = mc.activePlayers[TiltFive.PlayerIndex.Two].damagePercentage.ToString("F1") + "%";
                    for (int j = 0; j < 3 - mc.activePlayers[TiltFive.PlayerIndex.Two].currentLives; j++)
                    {
                        playerPercentages[1].gameObject.transform.GetChild(2 - j).gameObject.SetActive(false);
                    }
            }
            else
            {
                playerPercentages[1].gameObject.SetActive(false);
            }
            if (mc.activePlayers.ContainsKey(TiltFive.PlayerIndex.Three) && mc.activePlayers[TiltFive.PlayerIndex.Three].selectedCharacter != null)
            {
                    playerPercentages[2].gameObject.SetActive(true);
                    playerPercentages[2].text = mc.activePlayers[TiltFive.PlayerIndex.Three].damagePercentage.ToString("F1") + "%";
                    for (int k = 0; k < 3 - mc.activePlayers[TiltFive.PlayerIndex.Three].currentLives; k++)
                    {
                        playerPercentages[2].gameObject.transform.GetChild(2 - k).gameObject.SetActive(false);
                    }
            }
            else
            {
                playerPercentages[2].gameObject.SetActive(false);
            }
            if (mc.activePlayers.ContainsKey(TiltFive.PlayerIndex.Four) && mc.activePlayers[TiltFive.PlayerIndex.Four].selectedCharacter != null)
            {
                    playerPercentages[3].gameObject.SetActive(true);
                    playerPercentages[3].text = mc.activePlayers[TiltFive.PlayerIndex.Four].damagePercentage.ToString("F1") + "%";
                    for (int l = 0; l < 3 - mc.activePlayers[TiltFive.PlayerIndex.Four].currentLives; l++)
                    {
                        playerPercentages[3].gameObject.transform.GetChild(2 - l).gameObject.SetActive(false);
                    }
            }
            else
            {
                playerPercentages[3].gameObject.SetActive(false);
            }
        }
    }
}
