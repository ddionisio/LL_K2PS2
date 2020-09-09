using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [CreateAssetMenu(fileName = "materialTag", menuName = "Game/Material Tag")]
    public class MaterialTagData : ScriptableObject {
        [M8.Localize]
        public string labelRef;
        public Sprite icon;
        public Color color = Color.white;

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