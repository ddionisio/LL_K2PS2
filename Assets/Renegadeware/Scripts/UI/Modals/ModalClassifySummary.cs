﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class ModalClassifySummary : M8.ModalController, M8.IModalPush, M8.IModalPop {

        public void End() {
            Close();

            GameData.instance.Progress();
        }

        void M8.IModalPop.Pop() {

        }

        void M8.IModalPush.Push(M8.GenericParams parms) {

        }
    }
}