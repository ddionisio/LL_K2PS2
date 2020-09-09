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

        private string mLabelText;
    }
}