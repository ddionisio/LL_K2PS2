using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
    public class GameData : M8.SingletonScriptableObject<GameData> {
        [Header("Gameplay Data")]
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

        public float cameraMoveDelay = 0.5f;
        public DG.Tweening.Ease cameraMoveTween = DG.Tweening.Ease.InOutQuad;

        public float goalDelay = 1f;

        [Header("Material Object Settings")]
        public Color objectGhostValidColor = Color.gray;
        public Color objectGhostInvalidColor = Color.red;

        [Header("Modals")]
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

        protected override void OnInstanceInit() {

        }
    }
}