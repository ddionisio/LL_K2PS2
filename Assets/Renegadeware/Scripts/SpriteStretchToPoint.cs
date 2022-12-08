using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    /// <summary>
    /// This will stretch at the up direction
    /// </summary>
    public class SpriteStretchToPoint : MonoBehaviour {
        public SpriteRenderer spriteRender;
        public Transform destination;

        void Update() {
            if(destination) {
                Vector2 startPos = transform.position;
                Vector2 endPos = destination.position;
                Vector2 dpos = endPos - startPos;
                float dist = dpos.magnitude;

                if(dist > 0f) {
                    spriteRender.enabled = true;

                    var dir = dpos / dist;

                    transform.up = dir;

                    switch(spriteRender.drawMode) {
                        case SpriteDrawMode.Sliced:
                        case SpriteDrawMode.Tiled:
                            var size = spriteRender.size;
                            size.y = dist;
                            spriteRender.size = size;
                            break;

                        default:
                            //TODO
                            //stretch scale
                            break;
                    }
                }
                else {
                    spriteRender.enabled = false;
                }
            }
            else {
                spriteRender.enabled = false;
            }
        }
    }
}