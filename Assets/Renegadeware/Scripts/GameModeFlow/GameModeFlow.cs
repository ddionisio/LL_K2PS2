using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public abstract class GameModeFlow : MonoBehaviour {
        //General Flow
        public abstract IEnumerator Intro();
        public abstract IEnumerator Outro();
        public abstract IEnumerator SectionBegin(int index);
        public abstract IEnumerator SectionEnd(int index);
    }
}