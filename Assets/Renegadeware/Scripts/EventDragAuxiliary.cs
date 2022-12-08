using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Renegadeware.K2PS2 {
    public class EventDragAuxiliary : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public GameObject target;

        private IBeginDragHandler mDragBeginHandler;
        private IDragHandler mDragHandler;
        private IEndDragHandler mDragEndHandler;

        void Awake() {
            if(target) {
                var behaviours = target.GetComponents<MonoBehaviour>();
                for(int i = 0; i < behaviours.Length; i++) {
                    var behaviour = behaviours[i];

                    if(mDragBeginHandler == null)
                        mDragBeginHandler = behaviour as IBeginDragHandler;

                    if(mDragHandler == null)
                        mDragHandler = behaviour as IDragHandler;

                    if(mDragEndHandler == null)
                        mDragEndHandler = behaviour as IEndDragHandler;

                    if(mDragBeginHandler != null && mDragHandler != null && mDragEndHandler != null)
                        break;
                }
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if(mDragBeginHandler != null)
                mDragBeginHandler.OnBeginDrag(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(mDragHandler != null)
                mDragHandler.OnDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(mDragEndHandler != null)
                mDragEndHandler.OnEndDrag(eventData);
        }
    }
}