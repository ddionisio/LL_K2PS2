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

        private string mLabelText;

        private M8.PoolController mPool;

        private M8.CacheList<MaterialObjectEntity> mSpawnedEntities;

        public void InitPool(int count) {
            mPool = M8.PoolController.CreatePool("materialObjectPool");
            mPool.AddType(template, count, count);

            if(mSpawnedEntities != null) {
                if(mSpawnedEntities.Capacity < count)
                    mSpawnedEntities.Resize(count);
            }
            else
                mSpawnedEntities = new M8.CacheList<MaterialObjectEntity>(count);
        }

        public void DeinitPool() {
            if(mSpawnedEntities != null)
                mSpawnedEntities.Clear();

            if(mPool) {
                mPool.RemoveType(template.name);
            }
        }

        public MaterialObjectEntity Spawn(M8.GenericParams parms) {
            if(!mPool)
                return null;

            var ent = mPool.Spawn<MaterialObjectEntity>(template.name, template.name, null, parms);
            ent.poolDataCtrl.despawnCallback += OnDespawn;
            
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
            poolDataCtrl.despawnCallback -= OnDespawn;

            for(int i = 0; i < mSpawnedEntities.Count; i++) {
                var ent = mSpawnedEntities[i];
                if(ent && ent.poolDataCtrl == poolDataCtrl) {
                    mSpawnedEntities.RemoveAt(i);
                    return;
                }
            }
        }
    }
}