using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [CreateAssetMenu(fileName = "gamePlayData", menuName = "Game/Play Data")]
    public class GamePlayData : ScriptableObject {
        [System.Serializable]
        public struct ItemData {
            public MaterialObjectData materialObject;
            public int count;
        }

        public ItemData[] items;

        public void InitItemPools() {
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];
                itm.materialObject.InitPool(itm.count);
            }
        }

        public void DeinitItemPools() {
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];
                itm.materialObject.DeinitPool();
            }
        }
    }
}