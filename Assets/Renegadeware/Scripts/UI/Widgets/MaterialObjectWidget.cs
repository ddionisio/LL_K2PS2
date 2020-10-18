using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [Header("Display")]
        public Image icon;
        public TMP_Text titleText;
        public GameObject countRootGO;
        public TMP_Text countText;
        public M8.UI.Graphics.ColorGroup colorGroup;

        public MaterialObjectData data { get; private set; }

        public MaterialObjectPaletteWidget palette { get; private set; }

        public bool isDragging { get; private set; }

        private MaterialObjectDragWidget mDragWidget;

        private MaterialObjectEntity mEntGhost;

        public void Setup(MaterialObjectData aData, MaterialObjectPaletteWidget palette, MaterialObjectDragWidget dragWidget) {
            data = aData;

            mDragWidget = dragWidget;

            icon.sprite = data.icon;
            titleText.text = data.label;

            this.palette = palette;

            if(colorGroup)
                colorGroup.Revert();
        }

        public void RefreshCountDisplay() {
            int curCount = data.maxCount - data.spawnedCount;

            if(curCount > 1) {
                if(countRootGO)
                    countRootGO.SetActive(true);

                if(countText)
                    countText.text = curCount.ToString();
            }
            else {
                if(countRootGO)
                    countRootGO.SetActive(false);
            }
        }

        void OnApplicationFocus(bool focus) {
            if(!focus) {
                EndDrag();
            }
        }

        void OnDisable() {
            EndDrag();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            isDragging = true;

            var pos = eventData.position;

            //setup drag widget display
            mDragWidget.Setup(data);
            mDragWidget.transform.position = pos;

            //spawn entity ghost
            var cam = Camera.main;
            var entPos = cam.ScreenToWorldPoint(pos);
            mEntGhost = data.Spawn(entPos, mDragWidget);

            if(colorGroup)
                colorGroup.ApplyColor(GameData.instance.draggingColor);

            GameData.instance.signalDragBegin.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            var pos = eventData.position;

            //update entity ghost position
            var cam = Camera.main;
            var entPos = cam.ScreenToWorldPoint(pos);

            var valid = mEntGhost.UpdateGhostPosition(entPos);

            //update drag widget position
            mDragWidget.transform.position = pos;
            mDragWidget.SetValid(valid);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            var gameDat = GameData.instance;

            var cast = eventData.pointerCurrentRaycast;

            //check ui contact
            if(cast.isValid && (gameDat.uiLayerMask & (1<<cast.gameObject.layer)) != 0) {
                var go = cast.gameObject;

                //check if it's another material object widget
                if(go.CompareTag(gameDat.materialObjectTag)) {
                    //swap?
                    var otherMatObjWidget = go.GetComponent<MaterialObjectWidget>();
                    if(otherMatObjWidget && palette != otherMatObjWidget.palette) {
                        var otherData = otherMatObjWidget.data;

                        otherMatObjWidget.palette.RemoveItem(otherMatObjWidget.data);
                        palette.RemoveItem(data);

                        otherMatObjWidget.palette.AddItem(data);
                        palette.AddItem(otherData);
                    }
                }
                //check if it's palette
                else if(go.CompareTag(gameDat.placementTag)) {
                    //add to the palette
                    var toPalette = go.GetComponent<MaterialObjectPaletteWidget>();
                    if(toPalette && !toPalette.isFull) {
                        palette.RemoveItem(data);
                        toPalette.AddItem(data);
                    }
                }
            }
            //check placement
            else if(mEntGhost.isPlaceable) {
                mEntGhost.state = MaterialObjectEntity.State.Spawning;
                mEntGhost = null;
            }

            EndDrag();
        }

        private void EndDrag() {
            if(!isDragging)
                return;

            isDragging = false;

            //release entity ghost if it wasn't placed
            if(mEntGhost) {
                mEntGhost.Release();
                mEntGhost = null;
            }

            if(colorGroup)
                colorGroup.Revert();

            GameData.instance.signalDragEnd.Invoke();
        }
    }
}