using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DMTimeArea
{
    public class ResManager
    {
        public static GUIContent playContent = EditorGUIUtility.IconContent("Animation.Play", "|Play the Current active Shot.");
        public static GUIContent recordContent = EditorGUIUtility.IconContent("Animation.Record", "|Enable/disable keyframe recording mode.");
        public static GUIContent prevKeyContent = EditorGUIUtility.IconContent("Animation.PrevKey", "|Go to previous keyframe.");
        public static GUIContent nextKeyContent = EditorGUIUtility.IconContent("Animation.NextKey", "|Go to next keyframe.");
        public static GUIContent firstKeyContent = EditorGUIUtility.IconContent("Animation.FirstKey", "|Go to the beginning of the active Shot.");
        public static GUIContent lastKeyContent = EditorGUIUtility.IconContent("Animation.LastKey", "|Go to the end of the active Shot.");

        private static Texture _stopIcon;
        public static Texture StopIcon
        {
            get
            {
                if (_stopIcon == null)
                {
                    _stopIcon = (EditorGUIUtility.Load("Assets/EveryCine/SimpleTimeLineWindow/EditorResources/StopIcon.png") as Texture);
                }
                return _stopIcon;
            }
        }

        private static Texture _settingIcon;
        public static Texture SettingIcon
        {
            get
            {
                if (_settingIcon == null)
                {
                    _settingIcon = (EditorGUIUtility.Load("Assets/EveryCine/SimpleTimeLineWindow/EditorResources/SettingsIcon.png") as Texture);
                }
                return _settingIcon;
            }
        }

        private static Texture _cutOffGuideLine;
        public static Texture CutOffGuideLineTexture
        {
            get
            {
                if (_cutOffGuideLine == null)
                    _cutOffGuideLine = (EditorGUIUtility.Load("Assets/EveryCine/SimpleTimeLineWindow/EditorResources/CutOffTimeCursor.png") as Texture);
                return _cutOffGuideLine;
            }
        }

        private static Texture _timeHead;
        public static Texture TimeHeadTexture
        {
            get
            {
                if (_timeHead == null)
                    _timeHead = (EditorGUIUtility.Load("Assets/EveryCine/SimpleTimeLineWindow/EditorResources/Timecursor.png") as Texture);
                return _timeHead;
            }
        }
        
        private static Texture _keyDiamondTexture;
        public static Texture KeyDiamondTexture
        {
            get
            {
                if (_keyDiamondTexture == null)
                    _keyDiamondTexture = (EditorGUIUtility.Load("Assets/EveryCine/SimpleTimeLineWindow/EditorResources/KeyDiamond.png") as Texture);
                return _keyDiamondTexture;
            }
        }
        
        private static Texture _keyDiamondSelectedTexture;
        public static Texture KeyDiamondSelectedTexture
        {
            get
            {
                if (_keyDiamondSelectedTexture == null)
                    _keyDiamondSelectedTexture = (EditorGUIUtility.Load("Assets/EveryCine/SimpleTimeLineWindow/EditorResources/KeyDiamond_Selected.png") as Texture);
                return _keyDiamondSelectedTexture;
            }
        }
        
        private static Texture _addIcon;
        public static Texture AddIcon
        {
            get
            {
                if (_addIcon == null)
                {
                    _addIcon = (EditorGUIUtility.FindTexture("d_Toolbar Plus") as Texture);
                }
                return _addIcon;
            }
        }
    }
}
