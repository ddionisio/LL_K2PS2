using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectPaletteWidget : MonoBehaviour {
        [Header("Display")]
        public MaterialTagWidget tagWidget;
        public Transform itemsRoot;
        public MaterialObjectDragWidget dragWidget;

        public bool isFull { get { return mItemCache.Count == 0; } }

        private M8.CacheList<MaterialObjectWidget> mItemActives;
        private M8.CacheList<MaterialObjectWidget> mItemCache;

        public void Setup(MaterialTagData tag, LevelData.ItemData[] itemData) {
            //setup tag
            tagWidget.Setup(tag);

            //setup items
            RemoveAll();

            if(itemData != null) {
                for(int i = 0; i < itemData.Length; i++) {
                    var objData = itemData[i].materialObject;

                    if(objData.CompareTag(tag))
                        AddItem(objData);
                }
            }
        }

        public void Refresh(bool removeEmpty) {
            for(int i = mItemActives.Count - 1; i >= 0; i--) {
                var itm = mItemActives[i];
                var itmDat = itm.data;

                if(itmDat.maxCount - itmDat.spawnedCount > 0) {
                    itm.gameObject.SetActive(true);
                    itm.RefreshCountDisplay();
                }
                else {
                    itm.gameObject.SetActive(false);

                    if(removeEmpty) {
                        mItemActives.RemoveAt(i);
                        mItemCache.Add(itm);
                    }
                }
            }
        }

        public void AddItem(MaterialObjectData dat) {
            if(mItemCache.Count == 0)
                return;

            //just refresh if already exists
            for(int i = 0; i < mItemActives.Count; i++) {
                var itm = mItemActives[i];
                if(itm.data == dat) {
                    itm.RefreshCountDisplay();
                    return;
                }
            }

            var newItem = mItemCache.RemoveLast();

            newItem.Setup(dat, this, dragWidget);
            newItem.RefreshCountDisplay();

            newItem.transform.SetAsLastSibling();

            newItem.gameObject.SetActive(true);

            mItemActives.Add(newItem);
        }

        public void RemoveItem(MaterialObjectData dat) {
            for(int i = 0; i < mItemActives.Count; i++) {
                var itm = mItemActives[i];
                if(itm.data == dat) {
                    itm.gameObject.SetActive(false);

                    mItemActives.RemoveAt(i);
                    mItemCache.Add(itm);
                    break;
                }
            }
        }

        public void RemoveAll() {
            for(int i = 0; i < mItemActives.Count; i++) {
                var itm = mItemActives[i];

                itm.gameObject.SetActive(false);

                mItemCache.Add(itm);
            }

            mItemActives.Clear();
        }

        void Awake() {
            var itemCapacity = itemsRoot.childCount;

            //setup item cache
            mItemCache = new M8.CacheList<MaterialObjectWidget>(itemCapacity);
            mItemActives = new M8.CacheList<MaterialObjectWidget>(itemCapacity);

            for(int i = 0; i < itemCapacity; i++) {
                var matObjWidget = itemsRoot.GetChild(i).GetComponent<MaterialObjectWidget>();
                matObjWidget.gameObject.SetActive(false);
                mItemCache.Add(matObjWidget);
            }
        }
    }
}