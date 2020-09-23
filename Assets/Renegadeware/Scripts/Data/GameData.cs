using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
    public class GameData : M8.SingletonScriptableObject<GameData> {
        [Header("Gameplay Data")]
        public LayerMask placementLayerMask;
        [M8.TagSelector]
        public string placementTag;
        [M8.TagSelector]
        public string[] placementIgnoreTags;

        [Header("Material Object Settings")]
        public Color objectGhostValidColor = Color.gray;
        public Color objectGhostInvalidColor = Color.red;

        protected override void OnInstanceInit() {

        }
    }
}