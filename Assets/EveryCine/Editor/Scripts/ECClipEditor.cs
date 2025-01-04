using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using DMTimeArea;
using UnityEngine.Serialization;
using TimeUtility = DMTimeArea.TimeUtility;

namespace EveryCine.Editor
{
    public class ECClipEditor : SimpleTimeArea
    {
        private Rect rectTotalArea;
        private Rect rectContent;
        private Rect rectTimeRuler;

        private Keyframe _lastDraggingKeyframe;

        private struct InstantObject
        {
            public ECClipVariable variable;
            public GameObject obj;
        }
        
        private Dictionary<string, InstantObject> instantObjects = new();
        private List<ECObjectWatchState> lastWatchState = new();
        private bool recording = false;

        private Rect rectTopBar;
        private Rect rectLeft;
        private Rect rectRight;
        public Rect rectLeftTopToolBar;

        [FormerlySerializedAs("editingClip")] public ECClip clip;

        private float _lastUpdateTime = 0f;

        #region Used

        private double runningTime = 0.0f;

        protected override double RunningTime
        {
            get { return runningTime; }
            set { runningTime = value; }
        }

        private static double cutOffTime = 20.0f;

        protected override double CutOffTime
        {
            get { return cutOffTime; }
            set { cutOffTime = value; }
        }

        private float LEFTWIDTH = 250f;

        public bool IsPlaying { get; set; }

        protected override bool IsLockedMoveFrame
        {
            get { return (IsPlaying || Application.isPlaying); }
        }

        protected override bool IsLockDragHeaderArrow
        {
            get { return IsPlaying; }
        }

        public override Rect _rectTimeAreaTotal
        {
            get { return rectTotalArea; }
        }

        public override Rect _rectTimeAreaContent
        {
            get { return rectContent; }
        }

        public override Rect _rectTimeAreaRuler
        {
            get { return rectTimeRuler; }
        }

        protected override float sequencerHeaderWidth
        {
            get { return LEFTWIDTH; }
        }

        #endregion

        /*
        [MenuItem("Window/EveryCine/Clip Editor", false, 2002)]
        public static void DoWindow()
        {
            var window = GetWindow<ECClipEditor>(false, "");
            window.titleContent = new GUIContent("EveryCine Clip Editor", ECResourcesEditor.ecIcon);
            window.minSize = new Vector3(400f, 200f);
            window.Show();
            
            var inspector = GetWindow<ECInspectorWindow>(false, "");
            inspector.titleContent = new GUIContent("EveryCine Inspector", ECResourcesEditor.ecIcon);
            inspector.minSize = new Vector3(400f, 200f);
            inspector.Show();
        }
        */

        public static void OpenClipEditor(ECClip clip)
        {
            var window = GetWindow<ECClipEditor>(false, "");
            window.titleContent = new GUIContent("EveryCine Clip Editor", ECResourcesEditor.ecIcon);
            window.minSize = new Vector3(400f, 200f);
            window.CleanUpInstantObjects();
            window.recording = false;
            window.clip = clip;
            window._lastDraggingKeyframe = null;
            window.InitInstantObjects();
            window.Show();

            ECInspectorWindow.InspectSomething(clip);
        }

        private static GUIContent prefabIconContent;

        private void OnEnable()
        {
            EditorApplication.update =
                (EditorApplication.CallbackFunction)System.Delegate.Combine(EditorApplication.update,
                    new EditorApplication.CallbackFunction(OnEditorUpdate));
            _lastUpdateTime = (float)EditorApplication.timeSinceStartup;
            
            if (prefabIconContent == null)
                prefabIconContent = EditorGUIUtility.IconContent("d_Prefab Icon");
        }

        private void OnDisable()
        {
            CleanUpInstantObjects();
            EditorApplication.update =
                (EditorApplication.CallbackFunction)System.Delegate.Remove(EditorApplication.update,
                    new EditorApplication.CallbackFunction(OnEditorUpdate));
            Close();
        }

        private void OnEditorUpdate()
        {
            // float delta = (float)(EditorApplication.timeSinceStartup - _lastUpdateTime);
            if (!Application.isPlaying && this.IsPlaying)
            {
                double fTime = (float)EditorApplication.timeSinceStartup - _lastUpdateTime;
                this.RunningTime += Math.Abs(fTime) * 1.0f;
                if (this.RunningTime >= this.CutOffTime)
                {
                    this.PausePreView();
                }
            }

            if (Application.isPlaying)
            {
                recording = false;
            }

            if (!Application.isPlaying && !recording)
            {
                if (clip != null)
                {
                    foreach (var track in clip.tracks)
                    {
                        //Debug.Log("I'm seeking!");
                        var data = track.Seek(this.RunningTime);

                        if (track.type == ECTrackType.GameObject)
                        {
                            if (instantObjects.ContainsKey(track.go_variableName))
                            {
                                var io = instantObjects[track.go_variableName];
                                ECCinema.TransformPlayFromData(io.obj, data);
                            }
                        }
                    }
                }
            }
            
            for (int i = lastWatchState.Count - 1; i >= 0; i--)
            {
                var state = lastWatchState[i];
                if (ObjCompareState(state))
                    ObjChangeDetected(i, state);
            }
            
            _lastUpdateTime = (float)EditorApplication.timeSinceStartup;
            Repaint();
        }

        private bool ObjCompareState(ECObjectWatchState state)
        {
            var target = state.target;
            var targetTrans = target.transform;
            
            if (targetTrans.localPosition != state.lastPos)
            {
                return true;
            }
            
            if (targetTrans.localRotation.eulerAngles != state.lastRot)
            {
                return true;
            }
            
            if (targetTrans.localScale != state.lastSca)
            {
                return true;
            }

            return false;
        }

        private ECObjectWatchState ObjBuildNewState(GameObject target, string varName)
        {
            return new ECObjectWatchState
            {
                lastPos = target.transform.localPosition,
                lastRot = target.transform.localRotation.eulerAngles,
                lastSca = target.transform.localScale,
                target = target,
                varName = varName
            };
        }

        private void ObjChangeDetected(int idx, ECObjectWatchState state)
        {
            var frame = Mathf.RoundToInt((float) runningTime / ECConstants.SecondPerFrame);
            Debug.Log($"[{idx}] Change Detected at {frame}");
            lastWatchState[idx] = ObjBuildNewState(state.target, state.varName);

            foreach (var track in clip.tracks)
            {
                if (track.type == ECTrackType.GameObject && track.go_variableName == state.varName)
                {
                    var kfIdx = track.FindKeyframeIdx(frame);

                    if (kfIdx >= 0)
                    {
                        track.keyframes[kfIdx].data = ECKeyframeParser.MakeTransform(
                            state.lastPos,
                            state.lastRot,
                            state.lastSca);
                    }
                    else
                    {
                        track.keyframes.Add(new Keyframe
                        {
                            start = frame,
                            end = frame,
                            data = ECKeyframeParser.MakeTransform(
                                state.lastPos,
                                state.lastRot,
                                state.lastSca),
                            type = "transform",
                            startEndTogether = !track.hasDuration
                        });
                    }
                }
            }
        }

        private void OnGUI()
        {
            Rect rectMainBodyArea =
                new Rect(0, toolbarHeight, base.position.width, this.position.height - toolbarHeight);
            rectTopBar = new Rect(0, 0, this.position.width, toolbarHeight);
            rectLeft = new Rect(rectMainBodyArea.x, rectMainBodyArea.y + timeRulerHeight, LEFTWIDTH,
                rectMainBodyArea.height);
            rectLeftTopToolBar = new Rect(rectMainBodyArea.x, rectMainBodyArea.y, LEFTWIDTH, timeRulerHeight);
            rectRight = new Rect(rectMainBodyArea.width - LEFTWIDTH, rectMainBodyArea.y + timeRulerHeight, LEFTWIDTH,
                rectMainBodyArea.height);
            
            rectTotalArea = new Rect(rectMainBodyArea.x + LEFTWIDTH, rectMainBodyArea.y,
                base.position.width - LEFTWIDTH, rectMainBodyArea.height);
            rectTimeRuler = new Rect(rectMainBodyArea.x + LEFTWIDTH, rectMainBodyArea.y,
                base.position.width - LEFTWIDTH, timeRulerHeight);
            rectContent = new Rect(rectMainBodyArea.x + LEFTWIDTH, rectMainBodyArea.y + timeRulerHeight,
                base.position.width - LEFTWIDTH, rectMainBodyArea.height - timeRulerHeight);

            InitTimeArea(false, false, true, true);
            DrawTimeAreaBackGround(recording);
            OnTimeRulerCursorAndCutOffCursorInput();
            DrawTimeRulerArea();

            // Draw your top bar
            DrawTopToolBar();
            // Draw left content
            DrawLeftContent();
            // Draw your left tool bar
            DrawLeftTopToolBar();

            GUILayout.BeginArea(rectContent);
            DrawKeyframes(rectTotalArea.x);
            //DrawCurveLine(rectTotalArea.x);

            GUILayout.EndArea();
        }


        protected override void DrawVerticalTickLine()
        {
            Color preColor = Handles.color;
            Color color = Color.white;
            color.a = 0.3f;
            Handles.color = color;
            // draw vertical ticks
            float step = 10;
            float preStep = GetTimeArea.drawRect.height / 20f;
            // step = GetTimeArea.drawRect.y;
            step = 0f;
            while (step <= GetTimeArea.drawRect.height + GetTimeArea.drawRect.y)
            {
                Vector2 pos = new Vector2(rectContent.x, step + GetTimeArea.drawRect.y);
                Vector2 endPos = new Vector2(position.width, step + GetTimeArea.drawRect.y);
                step += preStep;
                float height = PixelToY(step);
                Rect rect = new Rect(rectContent.x + 5f, step - 10f + GetTimeArea.drawRect.y, 100f, 20f);
                GUI.Label(rect, height.ToString("0"));
                Handles.DrawLine(pos, endPos);
            }

            Handles.color = preColor;
        }

        protected virtual void DrawLeftContent()
        {
            GUILayout.BeginArea(rectLeft);
            
            foreach (var track in clip.tracks)
            {
                var rect = EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.box));

                if (GUI.Button(rect, GUIContent.none))
                {
                    ECInspectorWindow.InspectSomething(track);
                }
                
                GUILayout.BeginHorizontal();
                
                Texture icon;

                switch (track.type)
                {
                    case ECTrackType.GameObject:
                        icon = ECResourcesEditor.prefabIcon;
                        break;
                    case ECTrackType.MainCamera:
                    default:
                        icon = ECResourcesEditor.cameraIcon;
                        break;
                }
                
                EditorGUILayout.LabelField(new GUIContent(icon), GUILayout.Width(20));
                GUILayout.Label(track.trackName);
                
                GUILayout.EndHorizontal();
            
                GUILayout.EndVertical();
            }
            
            GUILayout.EndArea();
        }
        
        protected virtual void DrawTopToolBar()
        {
            GUILayout.BeginArea(rectTopBar);
            Rect rect = new Rect(rectTopBar.width - 32, rectTopBar.y, 30, 30);
            if (GUI.Button(new Rect(0, rectTopBar.y, 150, 20), "Inspect itself"))
            {
                ECInspectorWindow.InspectSomething(clip);
            }
            if (!Application.isPlaying && GUI.Button(rect, ResManager.SettingIcon, EditorStyles.toolbarDropDown))
            {
                OnClickSettingButton();
            }

            GUILayout.EndArea();
        }

        private void DrawLeftTopToolBar()
        {
            // left top tool bar
            GUILayout.BeginArea(rectLeftTopToolBar, string.Empty, EditorStyles.toolbarButton);
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(ResManager.AddIcon, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                var newTrack = GetWindow<ECNewTrackWindowEditor>();
                newTrack.clip = clip;
                newTrack.titleContent = new GUIContent("New Track", ECResourcesEditor.ecIcon);
                newTrack.Show();
            }

            if (GUILayout.Button(ResManager.prevKeyContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                PreviousTimeFrame();
            }

            bool playing = IsPlaying;
            playing = GUILayout.Toggle(playing, ResManager.playContent, EditorStyles.toolbarButton,
                new GUILayoutOption[0]);
            if (!Application.isPlaying)
            {
                if (IsPlaying != playing)
                {
                    IsPlaying = playing;
                    if (IsPlaying)
                        PlayPreview();
                    else
                        PausePreView();
                }
            }

            if (GUILayout.Button(ResManager.nextKeyContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                NextTimeFrame();
            }

            if (GUILayout.Button(ResManager.StopIcon, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))
                && !Application.isPlaying)
            {
                PausePreView();
                this.RunningTime = 0.0f;
            }
            
            if (GUILayout.Button(ECResourcesEditor.recordIcon, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                recording = !recording;

                if (recording)
                {
                    foreach (var obj in instantObjects)
                    {
                        lastWatchState.Add(ObjBuildNewState(obj.Value.obj, obj.Key));
                    }
                }
                else
                {
                    foreach (var track in clip.tracks)
                    {
                        track.keyframes =
                            track.keyframes.OrderBy(kf => kf.start)
                                .ToList();
                    }
                    lastWatchState.Clear();
                }
            }

            GUILayout.FlexibleSpace();
            string timeStr = TimeAsString((double)this.RunningTime, "F2");
            GUILayout.Label(timeStr);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void PlayPreview()
        {
            IsPlaying = true;
        }

        private void PausePreView()
        {
            IsPlaying = false;
        }

        private void CleanUpInstantObjects()
        {
            foreach (var instantObject in instantObjects)
            {
                DestroyImmediate(instantObject.Value.obj);
            }
            lastWatchState.Clear();
            instantObjects.Clear();
        }

        private void InitInstantObjects()
        {
            Debug.Log("Init Instant Objects...");
            foreach (var variable in clip.variables)
            {
                if (variable.varType == ECClipVariable.ECClipVariableType.GameObject)
                {
                    var obj = Instantiate(variable.go_defaultPrefab);
                    obj.hideFlags = HideFlags.DontSave;
                    obj.AddComponent<ECClipInstantObject>();
                    instantObjects[variable.varName] = new InstantObject
                    {
                        obj = obj,
                        variable = variable
                    };
                }
            }
        }

        private void OnDestroy()
        {
            Debug.Log("Destroying!");
            CleanUpInstantObjects();
        }

        private void DrawKeyframes(float offsetX)
        {
            var e = Event.current;
            var eventType = e.type;
            var eventPos = e.mousePosition;
            var i = 0;
            
            foreach (var track in clip.tracks)
            {
                foreach (var keyframe in track.keyframes)
                {
                    if (track.hasDuration)
                    {
                        // todo:
                    }
                    else
                    {
                        var px = TimeToPixel(keyframe.start * ECConstants.SecondPerFrame);

                        var btn = new GUIStyle(GUI.skin.button);
                        btn.normal.background = ResManager.KeyDiamondTexture as Texture2D;
                        
                        if (ECInspectorWindow.currentInspecting == keyframe)
                        {
                            btn.normal.background = ResManager.KeyDiamondSelectedTexture as Texture2D;
                        }

                        var rect = new Rect(offsetX + px - LEFTWIDTH * 2 - 6, 6 + i * 26, 12, 12);

                        if (GUI.Button(rect,
                                ResManager.KeyDiamondTexture,
                                btn))
                        {
                            ECInspectorWindow.InspectSomething(keyframe);
                        }
                        
                        if (eventType == EventType.MouseDown && rect.Contains(eventPos)) {
                            _lastDraggingKeyframe = keyframe;
                        }
                    }
                }

                i++;
            }
            
            if (_lastDraggingKeyframe != null && eventType == EventType.MouseDrag)
            {
                double t = PixelToTime(-offsetX + eventPos.x + LEFTWIDTH * 2 - 0);
                int f = Mathf.RoundToInt((float)t / ECConstants.SecondPerFrame - ECConstants.SecondPerFrame / 2);

                if (f >= 0)
                {
                    _lastDraggingKeyframe.start = f;
                    _lastDraggingKeyframe.end = f;
                }
            }
            
            if (_lastDraggingKeyframe != null && eventType == EventType.MouseUp) {
                ECInspectorWindow.InspectSomething(_lastDraggingKeyframe);
                _lastDraggingKeyframe = null;
            }
        }
    }
}