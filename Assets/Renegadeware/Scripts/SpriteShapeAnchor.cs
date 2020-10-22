using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Renegadeware.K2PS2 {
    // Demo Script Usage:
    //   When you want multiple SpriteShapes to share a common Spline,
    //   attach this script to the secondary objects you would like to 
    //   copy the Spline and set the ParentObject to the original object
    //   you are copying from.

    [ExecuteInEditMode]
    public class SpriteShapeAnchor : MonoBehaviour {

        public GameObject m_ParentObject;

        public bool disableDisplay;

        // Use this for initialization
        void Awake() {
            if(Application.isPlaying) {
                if(disableDisplay) {
                    var ctrl = GetComponent<SpriteShapeController>();
                    ctrl.enabled = false;

                    var renderCtrl = GetComponent<SpriteShapeRenderer>();
                    renderCtrl.enabled = false;
                }
            }
        }

        // Update is called once per frame
        void Update() {
            if(m_ParentObject != null) {
                CopySpline(m_ParentObject, gameObject);
            }
        }

        private static void CopySpline(GameObject src, GameObject dst) {
#if UNITY_EDITOR
            var parentSpriteShapeController = src.GetComponent<SpriteShapeController>();
            var mirrorSpriteShapeController = dst.GetComponent<SpriteShapeController>();

            if(parentSpriteShapeController != null && mirrorSpriteShapeController != null) {
                SerializedObject srcController = new SerializedObject(parentSpriteShapeController);
                SerializedObject dstController = new SerializedObject(mirrorSpriteShapeController);
                SerializedProperty srcSpline = srcController.FindProperty("m_Spline");
                dstController.CopyFromSerializedProperty(srcSpline);

                if(dstController.ApplyModifiedProperties())
                    EditorUtility.SetDirty(mirrorSpriteShapeController);
            }
#endif
        }

    }
}