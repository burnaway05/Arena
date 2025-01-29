using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS
{
    //script for storing globally hashes of names for all frequently used animations, for performance benefits
    public static class AnimationNames
    {
        //item animating
        public static int ITEM_FIRE = Animator.StringToHash("recoil");
        public static int ITEM_TAKE = Animator.StringToHash("take");
        public static int ITEM_IDLE = Animator.StringToHash("idle");
        public static int ITEM_MELEE = Animator.StringToHash("melee");
        public static int ITEM_RELOAD = Animator.StringToHash("reload");
        public static int ITEM_ENDRELOAD = Animator.StringToHash("endReload");

        //universal
        public static int ITEM_SPEED = Animator.StringToHash("speed");

        //character animating
        public static int CHARACTER_LOOK = Animator.StringToHash("look");
        public static int CHARACTER_MOVEMENT_HORIZONTAL = Animator.StringToHash("X");
        public static int CHARACTER_MOVEMENT_VERTICAL = Animator.StringToHash("Y");
        public static int CHARACTER_ISGROUNDED = Animator.StringToHash("isGrounded");
        public static int CHARACTER_ISCROUCHING = Animator.StringToHash("isCrouching");
    }
}