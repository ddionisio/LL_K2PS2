using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Renegadeware.K2PS2 {
    [CustomEditor(typeof(WindController))]
    public class WindControllerInspector : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            //check for box collider, resize it if needed
            var dat = this.target as WindController;

            var boxColl = dat.GetComponent<BoxCollider2D>();
            if(boxColl) {
                Vector2 newBoxSize = new Vector2(dat.width, dat.length);
                Vector2 newBoxCenter = new Vector2(0f, dat.length * 0.5f);

                if(boxColl.offset != newBoxCenter || boxColl.size != newBoxSize) {
                    Undo.RecordObject(boxColl, "Wind Controller Resize");

                    boxColl.offset = newBoxCenter;
                    boxColl.size = newBoxSize;
                }
            }
        }
    }
}