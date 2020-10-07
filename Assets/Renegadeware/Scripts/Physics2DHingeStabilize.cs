using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class Physics2DHingeStabilize : MonoBehaviour {
        public Rigidbody2D body;
        public HingeJoint2D hinge;
        public Rigidbody2D[] platforms;
        public float angularVelocityMax = 10f;
        public float stopDelay = 0.3f;

        private bool mIsStop;
        private float mStopBeginTime;

        private RigidbodyConstraints2D[] mPlatformDefaultConstraints;

        void Awake() {
            mPlatformDefaultConstraints = new RigidbodyConstraints2D[platforms.Length];
            for(int i = 0; i < platforms.Length; i++) {
                if(platforms[i])
                    mPlatformDefaultConstraints[i] = platforms[i].constraints;
            }
        }

        void FixedUpdate() {
            if(mIsStop) {
                if(Time.time - mStopBeginTime >= stopDelay) {
                    body.freezeRotation = false;

                    for(int i = 0; i < platforms.Length; i++) {
                        if(platforms[i])
                            platforms[i].constraints = mPlatformDefaultConstraints[i];
                    }

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

                    for(int i = 0; i < platforms.Length; i++) {
                        if(platforms[i]) {
                            platforms[i].constraints = RigidbodyConstraints2D.FreezeAll;
                            platforms[i].velocity = Vector2.zero;
                        }
                    }

                    mIsStop = true;
                    mStopBeginTime = Time.time;
                }
            }
        }
    }
}