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

        [Header("Signals")]
        public M8.Signal signalInvokeDragBegin;
        public M8.Signal signalInvokeDragEnd;

        public MaterialObjectData data { get; private set; }
        public int maxCount { get; private set; }

        public bool isDragging { get; private set; }

        private MaterialObjectDragWidget mDragWidget;

        private MaterialObjectEntity mEntGhost;

        public void Init(MaterialObjectData aData, int aMaxCount, MaterialObjectDragWidget dragWidget) {
            data = aData;
            maxCount = aMaxCount;

            mDragWidget = dragWidget;

            icon.sprite = data.icon;
            titleText.text = data.label;
            RefreshCountDisplay();
        }

        public void RefreshCountDisplay() {
            if(!countText)
                return;

            int curCount = maxCount - data.spawnedCount;
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

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            isDragging = true;

            //setup drag widget display
            mDragWidget.Setup(data);

            //spawn entity ghost

            signalInvokeDragBegin?.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            //update drag widget position

            //update entity ghost
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            //check placement

            EndDrag();
        }

        private void EndDrag() {
            if(!isDragging)
                return;

            isDragging = false;

            //release entity ghost if it wasn't placed
            if(mEntGhost) {
                if(mEntGhost.state == MaterialObjectEntity.State.Ghost || mEntGhost.state == MaterialObjectEntity.State.None)
                    mEntGhost.Release();

                mEntGhost = null;
            }

            signalInvokeDragEnd?.Invoke();
        }
    }
}