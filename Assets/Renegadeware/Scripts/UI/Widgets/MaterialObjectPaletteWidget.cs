using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectPaletteWidget : MonoBehaviour {
        [Header("Display")]
        public MaterialTagWidget tagWidget;
        public MaterialObjectWidget[] itemWidgets;

        private int mItemCount;

        public void Setup(GamePlayData data, MaterialTagData tag, MaterialObjectDragWidget dragWidget) {
            //initialize display
            tagWidget.Setup(tag);

            //filter objects based on tag
            mItemCount = Mathf.Min(itemWidgets.Length, data.items.Length);

            for(int i = 0; i < mItemCount; i++) {
                var objData = data.items[i].materialObject;
                var objWidget = itemWidgets[i];

                objWidget.Setup(objData, dragWidget);
            }

            //hide excess items
            for(int i = mItemCount; i < itemWidgets.Length; i++)
                itemWidgets[i].gameObject.SetActive(false);

            Refresh();
        }

        public void Refresh() {
            for(int i = 0; i < mItemCount; i++) {
                var objWidget = itemWidgets[i];

                if(objWidget.data.maxCount - objWidget.data.spawnedCount > 0) {
                    objWidget.gameObject.SetActive(true);
                    objWidget.RefreshCountDisplay();
                }
                else
                    objWidget.gameObject.SetActive(false);
            }
        }
    }
}