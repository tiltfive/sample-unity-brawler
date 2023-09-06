# Tilt Five Brawler Sample

### Overview

This is a quick introduction to the structure and purpose of the Battle Brawl (or "Brawler") sample provided by Tilt Five.

### Purpose

The goal of this project is to provide a sample of a game that has reached a playable state for developers to reference if they're getting started developing with the Tilt Five system. None of the Assets included are expected or required to be used in creation of your game, they are provided as an example of how they can be accomplished. All scripts included are available for inclusion in your own applications if you wish, as is modification and redistribution of this project, pursuant the included license.

### Required Unity Versions

This project was built with Unity 2021.3 LTS and should work with later Unity versions.

### Structure

This project includes 3 total scenes, an initial loading and title scene, a character selection scene, and a single arena. 

Also included are 2 different playable characters, a goblin and a large dinosaur monster.

#### Title Scene
The title scene includes a rudimentary version checker, if the player doesn't have an up to date Tilt Five driver, an on screen notification will appear directing them to download the latest drivers from the Tilt Five website.

#### Character Select Scene
The character select was created to showcase how world space UI can be utilized with the Tilt Five Gameboard to give each player a unique view of the same content. 

There are 4 unique gameboards, each assigned to a different Tilt Five Player Index in the Manager. By individually rotating these boards based on the user's location, we can ensure that the content they see is always facing towards the user. Thus, no matter what side of the board they're standing on, it will appear to them as if the content is facing directly towards them, and movement will respect their current facing.

Additionally, the first instance of the Multiplayer Controller is introduced in this scene. The purpose of the multiplayer controller is to track when new players join in to the game session via connecting a new pair of Tilt Five Glasses to the gameboard, and establish all of the necessary components for them to be able to interact with the world. Specifically, when a new glasses connects, a PlayerPrefab is instantiated by the Tilt Five Manager. Because it contains a Player Input component, the Input System recognizes a new player as having joined, and their PlayerPrefab is given a PlayerController Component to act as a facilitator for all inputs. The Player Input component will then send messages directly to the associated PlayerController, which can pass the controls on to whichever moveable piece is currently being controlled. In the case of the Character Select Scene, that moveable piece is the Selector. 

With a spawned selector, each player can choose which character they want to play, or if they want to spectate the game instead. 

#### Arena Scene

This scene is where the players get to interact. When Players join this scene, instead of creating selectors, their selected character is created(Or no character if they chose to spectate instead). Then, the match begins, with various Wand Inputs relating to different attacks such as light attacks, heavy attacks, jumps, blocks, projectiles, etc. As players get hit, their percentage builds up, and the higher the percentage, the greater the knock-back and input lockout.

The system was created as a rudimentary analog to the Super Smash Brothers game series, including launch trajectories, hit stun, hit shake, scaling launch vectors, unique hit boxes based on the attack, and distinct character weights and models. When creating a new playable character, all of the fields are adjustable to control how each attack will impact the target, how the character jumps, and how their projectiles function. In addition to unique models, and changes to the model's Rigidbody, a full suite of distinct characters could easily be created. 

All inputs connect to the controlled characters via an "Event" system. Each button press queues a specific event up for a character to process. This event system allows features such as Hit Stun and Hit Stop to prevent the skipping of inputs, creating a minor input buffer effect. If new animations or reaction are to be added, new events should be created and initiated by the Player Controller.

#### Tilt Five Specific Features

In terms of Tilt Five specific features, this scene contains two useful components. First, this scene relies entirely on a single map that all players view the content based off of. This map's position and scale is modified based on the available characters to contain the action but also keep it at a reasonable scale for all to see. There is also no rotation aspect, meaning all players are seeing the same content from their own unique perspective, and there all virtual objects are co-located relative to the real world.

Additionally, a Disconnection Manager has been implemented to track non-spectator players and their wands. If at any point they leave the game or their wand disconnects, the game will pause and notify the player until they reconnect on enable a new wand.

### MixCast Integration

If the submodule for MixCast is retrieved with `git clone --recursive <this_project_url>` then the MixCast Asset will be built and integrated into the project. If you have already cloned the repo then a `git submodule update --init --recursive` command will pull down the MixCast component. MixCast is a way to view the gameplay overlayed on a webcam image of the user's environment. You will need to install the latest [MixCast client](https://mixcast.me/download) and to accept the MixCast SDK license to use this in your distributed projects. There may be a more recent MixCast SDK for Unity to replace the one pointed to by this repo. See more information at [MixCast.me](https://mixcast.me/).

### Asset Licenses

Brawler components provided by Tilt Five are distributed under the Apache License, Version 2.0. However, some Assets subdirectories for the project as redistributed here have their own license; for example, "Text Mesh Pro" is covered by the ["Unity Companion License for Unity-dependent projects"](https://unity.com/legal/licenses/unity-companion-license) in its [package LICENSE](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/license/LICENSE.html). 

Submodules may be covered under their own proprietary licenses which should be noted at their top level.

### Contacting Tilt Five

If you have questions about integrating Tilt Five into your project, check out our [developer resources](https://www.tiltfive.com/make/home) online.

---

   Copyright 2023 Tilt Five, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

