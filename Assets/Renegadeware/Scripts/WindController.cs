using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class WindController : MonoBehaviour {
        public float length = 1f;
        public float width = 1f;
        public LayerMask collisionMask;
        public int resolution = 1;

        public Color gizmoColor = Color.cyan;

        public float force;
        public float forceApplyDelay = 0f;

        private float[] mDistances;
        private float mLastFixedTime;

        private BoxCollider2D mBoxColl;
        private bool mHasBoxColl;

        private int mTriggerCount;

        void OnEnable() {
            mTriggerCount = 0;
            mLastFixedTime = 0f;
        }

        void Awake() {
            mDistances = new float[resolution];
            mBoxColl = GetComponent<BoxCollider2D>();
            mHasBoxColl = mBoxColl != null;
        }

        void OnTriggerEnter2D(Collider2D collision) {
            if(IsCollisionMatch(collision))
                mTriggerCount++;
        }

        void OnTriggerExit2D(Collider2D collision) {
            if(IsCollisionMatch(collision))
                mTriggerCount--;

            if(mTriggerCount < 0) {
                Debug.LogWarning("Trigger Count < 0, one of the collider did not give 'exit' message!");
                mTriggerCount = 0;
            }
        }

        void FixedUpdate() {
            //if we have collider, wait for trigger count > 0
            if(mHasBoxColl && mTriggerCount <= 0)
                return;

            bool apply;
            if(forceApplyDelay > 0f) {
                float curTime = Time.fixedTime;
                if(curTime - mLastFixedTime >= forceApplyDelay) {
                    mLastFixedTime = curTime;
                    apply = true;
                }
                else
                    apply = false;
            }
            else
                apply = true;

            //apply force
            if(apply) {
                float halfWidth = width * 0.5f;

                var mtx = transform.localToWorldMatrix;

                Vector2 start = mtx.MultiplyPoint3x4(new Vector2(-halfWidth, 0f));
                Vector2 end = mtx.MultiplyPoint3x4(new Vector2(halfWidth, 0f));
                Vector2 dir = mtx.MultiplyVector(Vector2.up);

                float step = (1.0f / resolution) * 0.5f;

                float t = 0f;

                for(int i = 0; i < resolution; i++) {
                    t += step;

                    Vector2 pt = Vector2.Lerp(start, end, t);

                    var hit = Physics2D.CircleCast(pt, step, dir, length, collisionMask);
                    if(hit.rigidbody && IsCollisionMatch(hit.collider)) {
                        hit.rigidbody.AddForceAtPosition(dir * force, hit.point);

                        mDistances[i] = hit.distance;
                    }
                    else
                        mDistances[i] = length;

                    t += step;
                }
            }
        }

        void OnDrawGizmos() {
            Gizmos.color = gizmoColor;

            if(width > 0f && length > 0f) {
                float halfWidth = width * 0.5f;

                Vector2[] corners = new Vector2[] {
                new Vector2(-halfWidth, 0f),
                new Vector2(-halfWidth, length),
                new Vector2(halfWidth, length),
                new Vector2(halfWidth, 0f),
            };

                var mtx = transform.localToWorldMatrix;

                for(int i = 0; i < corners.Length; i++) {
                    int nextInd = i == corners.Length - 1 ? 0 : i + 1;

                    Vector3 pos1 = mtx.MultiplyPoint3x4(corners[i]), pos2 = mtx.MultiplyPoint3x4(corners[nextInd]);

                    Gizmos.DrawLine(pos1, pos2);
                }
            }

            if(resolution > 0) {
                Gizmos.color = gizmoColor * 0.6f;

                float halfWidth = width * 0.5f;

                var mtx = transform.localToWorldMatrix;

                Vector3 start = mtx.MultiplyPoint3x4(new Vector2(-halfWidth, 0f));
                Vector3 end = mtx.MultiplyPoint3x4(new Vector2(halfWidth, 0f));
                Vector3 dir = mtx.MultiplyVector(Vector2.up);

                float step = (1.0f / resolution) * 0.5f;

                float t = 0f;

                for(int i = 0; i < resolution; i++) {
                    t += step;

                    Vector3 pt = Vector3.Lerp(start, end, t);

                    float len;
                    if(Application.isPlaying && mDistances != null && i < mDistances.Length) {
                        len = mDistances[i];
                    }
                    else
                        len = length;

                    if(len > 0f)
                        M8.Gizmo.Arrow(pt, dir * len);

                    t += step;
                }
            }
        }

        private bool IsCollisionMatch(Collider2D coll) {
            var gameDat = GameData.instance;

            return coll.CompareTag(gameDat.playerTag) || coll.CompareTag(gameDat.materialObjectTag);
        }
    }
}