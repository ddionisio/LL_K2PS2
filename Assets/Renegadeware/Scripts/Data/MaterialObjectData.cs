using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [CreateAssetMenu(fileName = "materialObject", menuName = "Game/Material Object")]
    public class MaterialObjectData : ScriptableObject {
        [Header("Display")]
        [M8.Localize]
        public string labelRef;
        public Sprite icon;

        [Header("Tags")]
        public MaterialTagData[] tags;

        [Header("Data")]
        public GameObject template;        

        public string label {
            get {
                if(string.IsNullOrEmpty(mLabelText))
                    mLabelText = M8.Localize.Get(labelRef);
                return mLabelText;
            }
        }

        public int spawnedCount {
            get {
                return mSpawnedEntities != null ? mSpawnedEntities.Count : 0;
            }
        }

        public int maxCount { get; private set; }

        private string mLabelText;

        private M8.PoolController mPool;

        private M8.CacheList<MaterialObjectEntity> mSpawnedEntities;

        private M8.GenericParams mSpawnParms = new M8.GenericParams();

        public bool CompareTag(MaterialTagData tagData) {
            for(int i = 0; i < tags.Length; i++) {
                if(tags[i] == tagData)
                    return true;
            }

            return false;
        }

        public void InitPool(int count) {
            mPool = M8.PoolController.CreatePool("materialObjectPool");
            mPool.AddType(template, count, count);

            mPool.despawnCallback += OnDespawn;

            if(mSpawnedEntities != null) {
                if(mSpawnedEntities.Capacity < count)
                    mSpawnedEntities.Resize(count);
            }
            else
                mSpawnedEntities = new M8.CacheList<MaterialObjectEntity>(count);

            maxCount = count;
        }

        public void DeinitPool() {
            if(mSpawnedEntities != null)
                mSpawnedEntities.Clear();

            if(mPool) {
                mPool.despawnCallback -= OnDespawn;

                mPool.RemoveType(template.name);
                mPool = null;
            }
        }

        public MaterialObjectEntity Spawn(Vector2 pos, MaterialObjectDragWidget dragWidget) {
            if(!mPool)
                return null;

            mSpawnParms[MaterialObjectEntity.parmData] = this;
            mSpawnParms[MaterialObjectEntity.parmDragWidget] = dragWidget;
            mSpawnParms[MaterialObjectEntity.parmState] = MaterialObjectEntity.State.Ghost;

            var ent = mPool.Spawn<MaterialObjectEntity>(template.name, template.name, null, mSpawnParms);
            ent.transform.position = pos;
            ent.transform.rotation = Quaternion.identity;

            if(mSpawnedEntities.IsFull) //fail-safe
                mSpawnedEntities.Expand(maxCount * 2);

            mSpawnedEntities.Add(ent);

            return ent;
        }

        public void DespawnAll() {
            if(mSpawnedEntities != null) {
                for(int i = 0; i < mSpawnedEntities.Count; i++) {
                    var ent = mSpawnedEntities[i];
                    if(ent)
                        ent.state = MaterialObjectEntity.State.Despawning;
                }
            }
        }

        void OnDespawn(M8.PoolDataController poolDataCtrl) {
            for(int i = 0; i < mSpawnedEntities.Count; i++) {
                var ent = mSpawnedEntities[i];
                if(ent && ent.poolDataCtrl == poolDataCtrl) {
                    mSpawnedEntities.RemoveAt(i);

                    GameData.instance.signalObjectReleased.Invoke();

                    return;
                }
            }
        }
    }
}