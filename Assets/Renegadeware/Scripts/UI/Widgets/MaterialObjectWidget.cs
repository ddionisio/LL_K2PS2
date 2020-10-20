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
        public GameObject errorRootGO;
        public GameObject highlightGO;

        public MaterialObjectData data { get; private set; }

        public MaterialObjectPaletteWidget palette { get; private set; }

        public bool isDragging { get; private set; }

        private MaterialObjectDragWidget mDragWidget;

        private MaterialObjectEntity mEntGhost;

        private MaterialObjectPaletteWidget mHighlightPalette;
        private MaterialObjectWidget mHighlightObjectWidget;

        public void Setup(MaterialObjectData aData, MaterialObjectPaletteWidget palette, MaterialObjectDragWidget dragWidget) {
            data = aData;

            mDragWidget = dragWidget;

            icon.sprite = data.icon;
            titleText.text = data.label;

            this.palette = palette;

            if(colorGroup)
                colorGroup.Revert();

            if(highlightGO)
                highlightGO.SetActive(false);

            if(errorRootGO)
                errorRootGO.SetActive(false);
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

        public void SetError(bool error) {
            if(errorRootGO)
                errorRootGO.SetActive(error);
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
            mEntGhost = data.Spawn(entPos, MaterialObjectEntity.State.Ghost, mDragWidget);

            if(colorGroup)
                colorGroup.ApplyColor(GameData.instance.draggingColor);

            if(errorRootGO)
                errorRootGO.SetActive(false);

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
            //mDragWidget.SetValid(valid);

            //update palette highlight
            var gameDat = GameData.instance;

            var cast = eventData.pointerCurrentRaycast;

            MaterialObjectPaletteWidget highlightPalette = null;
            MaterialObjectWidget highlightObjWidget = null;

            if(cast.isValid && (gameDat.uiLayerMask & (1 << cast.gameObject.layer)) != 0) {
                var go = cast.gameObject;

                //check if it's another material object widget in another palette
                if(go.CompareTag(gameDat.materialObjectTag)) {
                    var otherMatObjWidget = go.GetComponent<MaterialObjectWidget>();
                    if(otherMatObjWidget && otherMatObjWidget.palette != palette)
                        highlightObjWidget = otherMatObjWidget;
                }
                //check if it's another palette
                else if(go.CompareTag(gameDat.placementTag)) {
                    if(go != palette.gameObject) {
                        var otherPalette = go.GetComponent<MaterialObjectPaletteWidget>();
                        if(otherPalette && !otherPalette.isFull)
                            highlightPalette = otherPalette;
                    }
                }
            }

            if(highlightObjWidget) {
                if(mHighlightObjectWidget != highlightObjWidget) {
                    ClearHighlight();

                    mHighlightObjectWidget = highlightObjWidget;

                    if(mHighlightObjectWidget.highlightGO)
                        mHighlightObjectWidget.highlightGO.SetActive(true);
                }
            }
            else if(highlightPalette) {
                if(mHighlightPalette != highlightPalette) {
                    ClearHighlight();

                    mHighlightPalette = highlightPalette;

                    if(mHighlightPalette.highlightGO)
                        mHighlightPalette.highlightGO.SetActive(true);
                }
            }
            else
                ClearHighlight();
            //
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

            //release entity ghost if it wasn't placed
            if(mEntGhost) {
                mEntGhost.Release();
                mEntGhost = null;
            }

            if(colorGroup)
                colorGroup.Revert();

            ClearHighlight();

            isDragging = false;

            GameData.instance.signalDragEnd.Invoke();
        }

        private void ClearHighlight() {
            if(mHighlightPalette) {
                if(mHighlightPalette.highlightGO)
                    mHighlightPalette.highlightGO.SetActive(false);

                mHighlightPalette = null;
            }

            if(mHighlightObjectWidget) {
                if(mHighlightObjectWidget.highlightGO)
                    mHighlightObjectWidget.highlightGO.SetActive(false);

                mHighlightObjectWidget = null;
            }
        }
    }
}