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
        public TMP_Text countText;

        public MaterialObjectData data { get; private set; }

        public bool isDragging { get; private set; }

        private MaterialObjectDragWidget mDragWidget;

        private MaterialObjectEntity mEntGhost;

        public void Setup(MaterialObjectData aData, MaterialObjectDragWidget dragWidget) {
            data = aData;

            mDragWidget = dragWidget;

            icon.sprite = data.icon;
            titleText.text = data.label;
        }

        public void RefreshCountDisplay() {
            if(!countText)
                return;

            int curCount = data.maxCount - data.spawnedCount;
            if(curCount > 0) {
                countText.gameObject.SetActive(true);
                countText.text = curCount.ToString();
            }
            else
                countText.gameObject.SetActive(false);
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

            GameData.instance.signalDragBegin.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            var pos = eventData.position;

            //update drag widget position
            mDragWidget.transform.position = pos;

            //update entity ghost position
            var cam = Camera.main;
            var entPos = cam.ScreenToWorldPoint(pos);

            mEntGhost.UpdateGhostPosition(entPos);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            //check placement
            if(mEntGhost.isPlaceable) {
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

            GameData.instance.signalDragEnd.Invoke();
        }
    }
}