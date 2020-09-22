using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public struct GameUtils {
        public static bool CheckTags(Component comp, string[] tags) {
            for(int i = 0; i < tags.Length; i++) {
                if(comp.CompareTag(tags[i]))
                    return true;
            }

            return false;
        }
    }
}