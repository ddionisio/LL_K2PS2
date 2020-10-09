using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    [CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
    public class GameData : M8.SingletonScriptableObject<GameData> {
        [System.Serializable]
        public struct LevelData {
            public M8.SceneAssetPath classifyScene;
            public M8.SceneAssetPath playScene;
        }

        [Header("Data")]
        public LevelData[] levels;

        [Header("Gameplay Data")]
        [M8.TagSelector]
        public string HUDClassifyTag;
        [M8.TagSelector]
        public string HUDGameTag;
        [M8.TagSelector]
        public string playerTag;
        [M8.TagSelector]
        public string goalTag;

        public LayerMask placementLayerMask;
        [M8.TagSelector]
        public string placementTag;
        [M8.TagSelector]
        public string[] placementIgnoreTags;

        [M8.TagSelector]
        public string deleteTag;

        [M8.TagSelector]
        public string materialObjectTag;

        public float cameraMoveDelay = 0.5f;
        public DG.Tweening.Ease cameraMoveTween = DG.Tweening.Ease.InOutQuad;

        public float goalDelay = 1f;

        [Header("Material Object Settings")]
        public Color objectGhostValidColor = Color.gray;
        public Color objectGhostInvalidColor = Color.red;

        [Header("Conductive Settings")]
        public LayerMask conductiveLayerMask;
        [M8.TagSelector]
        public string[] conductiveTags;
        public float conductiveRefreshDelay = 0.3f;

        [Header("Scenes")]
        public M8.SceneAssetPath endScene;

        [Header("Modals")]
        public string modalClassifySummary = "classifySummary";
        public string modalVictory = "victory";

        [Header("Signals")]
        public M8.Signal signalDragBegin;
        public M8.Signal signalDragEnd;
        public M8.Signal signalGamePlay;
        public M8.Signal signalGameStop;
        public M8.Signal signalPlayerSpawn;
        public M8.Signal signalPlayerDeath;
        public M8.SignalVector2 signalPlayerMoveTo;
        public M8.Signal signalGoal;
        public M8.Signal signalClassify;
        public M8.Signal signalObjectDespawn;
        public M8.Signal signalObjectReleased;

        public bool isGameStarted { get; private set; } //true: we got through start normally, false: debug

        /// <summary>
        /// Called in start scene
        /// </summary>
        public void Begin() {
            isGameStarted = true;

            Current();
        }

        /// <summary>
        /// Update level index based on current progress, and load scene
        /// </summary>
        public void Current() {
            int curProgress, maxProgress;
            if(LoLManager.isInstantiated) {
                curProgress = LoLManager.instance.curProgress;
                maxProgress = LoLManager.instance.progressMax;
            }
            else {
                curProgress = 0;
                maxProgress = 0;
            }

            //end?
            if(curProgress >= maxProgress) {
                endScene.Load();
            }
            else {
                int curLevelIndex = curProgress / 2;
                bool isClassify = curProgress % 2 == 0;

                var curLevel = levels[curLevelIndex];

                if(isClassify) {
                    curLevel.classifyScene.Load();
                }
                else {
                    curLevel.playScene.Load();
                }
            }
        }

        /// <summary>
        /// Update progress, go to next level-scene
        /// </summary>
        public void Progress() {
            int curProgress;

            if(isGameStarted) {
                if(LoLManager.isInstantiated)
                    curProgress = LoLManager.instance.curProgress;
                else
                    curProgress = 0;
            }
            else {
                //determine our progress based on current scene
                var curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                int levelInd = 0;
                bool isClassify = true;

                for(int i = 0; i < levels.Length; i++) {
                    var lvl = levels[i];
                    if(lvl.classifyScene == curScene) {
                        levelInd = i;
                        isClassify = true;
                        break;
                    }
                    else if(lvl.playScene == curScene) {
                        levelInd = i;
                        isClassify = false;
                        break;
                    }
                }

                curProgress = levelInd * 2;
                if(!isClassify)
                    curProgress++;
            }

            if(LoLManager.isInstantiated)
                LoLManager.instance.ApplyProgress(curProgress + 1);

            //load to new scene
            Current();
        }

        protected override void OnInstanceInit() {
            //compute max progress
            if(LoLManager.isInstantiated) {
                LoLManager.instance.progressMax = levels.Length * 2;
            }

            isGameStarted = false;
        }
    }
}