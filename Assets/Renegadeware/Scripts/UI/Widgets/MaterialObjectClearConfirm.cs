﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectClearConfirm : MonoBehaviour {
        public string modal = "confirm";

        [M8.Localize]
        public string descRef;

        private M8.GenericParams mParms = new M8.GenericParams();

        private bool mIsConfirmed;

        public void Invoke() {
            if(mIsConfirmed)
                GameData.instance.signalReset.Invoke();
            else {
                mParms[ModalConfirm.parmDescTextRef] = descRef;
                mParms[ModalConfirm.parmCallback] = (System.Action<bool>)OnAccept;

                M8.ModalManager.main.Open(modal, mParms);
            }
        }

        void OnAccept(bool confirm) {
            if(confirm) {
                GameData.instance.signalReset.Invoke();

                mIsConfirmed = true; //only ask once
            }
        }
    }
}