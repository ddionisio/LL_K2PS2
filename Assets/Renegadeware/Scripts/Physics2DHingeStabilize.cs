using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class Physics2DHingeStabilize : MonoBehaviour {
        public Rigidbody2D body;
        public HingeJoint2D hinge;
        public float angularVelocityMax = 10f;
        public float stopDelay = 0.3f;

        private bool mIsStop;
        private float mStopBeginTime;

        void FixedUpdate() {
            if(mIsStop) {
                if(Time.time - mStopBeginTime >= stopDelay) {
                    body.freezeRotation = false;
                    mIsStop = false;
                }   
            }
            else {
                var rotZ = transform.localEulerAngles.z;
                if(rotZ > 180f)
                    rotZ -= 360f;

                if(body.angularVelocity >= angularVelocityMax && (rotZ <= hinge.limits.min || rotZ >= hinge.limits.max)) {
                    body.angularVelocity = 0f;
                    body.freezeRotation = true;
                    mIsStop = true;
                    mStopBeginTime = Time.time;
                }
            }
        }
    }
}