using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [CreateAssetMenu(fileName = "levelData", menuName = "Game/Level Data")]
    public class LevelData : ScriptableObject {
        [System.Serializable]
        public struct ItemData {
            public MaterialObjectData materialObject;
            public int count;
        }

        public MaterialTagData[] tags;
        public ItemData[] items;

        public int spawnedCount {
            get {
                int c = 0;

                for(int i = 0; i < items.Length; i++)
                    c += items[i].materialObject.spawnedCount;

                return c;
            }
        }

        public int placedCount {
            get {
                int c = 0;

                for(int i = 0; i < items.Length; i++)
                    c += items[i].materialObject.placedCount;

                return c;
            }
        }

        public void InitItemPools() {
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];
                itm.materialObject.InitPool(itm.count);
            }
        }

        public void InitItemPools(int itemCount) {
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];
                itm.materialObject.InitPool(itemCount);
            }
        }

        public void DeinitItemPools() {
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];
                itm.materialObject.DeinitPool();
            }
        }

        public bool IsTagMatch(MaterialTagData tag) {
            for(int i = 0; i < tags.Length; i++) {
                if(tags[i] == tag)
                    return true;
            }

            return false;
        }

        public void DespawnAll() {
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];
                itm.materialObject.DespawnAll();
            }
        }

        public bool IsAnyPlaced(MaterialObjectData matObjDat) {
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];
                if(itm.materialObject == matObjDat)
                    return itm.materialObject.placedCount > 0;
            }

            return false;
        }
    }
}