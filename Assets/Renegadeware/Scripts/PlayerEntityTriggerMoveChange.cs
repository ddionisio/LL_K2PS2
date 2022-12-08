using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class PlayerEntityTriggerMoveChange : MonoBehaviour {

        public PlayerEntity.MoveState toState;

        private PlayerEntity mPlayerEnt;

        void OnTriggerEnter2D(Collider2D collision) {
            var tagPlayer = GameData.instance.playerTag;
            if(collision.CompareTag(tagPlayer)) {
                if(!mPlayerEnt)
                    mPlayerEnt = collision.GetComponent<PlayerEntity>();

                if(mPlayerEnt && mPlayerEnt.state == PlayerEntity.State.Move)
                    mPlayerEnt.moveState = toState;
            }
        }
    }
}