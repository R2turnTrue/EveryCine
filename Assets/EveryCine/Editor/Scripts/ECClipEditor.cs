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
    public class ECClipEditor : SimpleTimeArea, ECCinemable, ECShowable
    {
        private Rect rectTotalArea;
        private Rect rectContent;
        private Rect rectTimeRuler;

        public static ECClipEditor Instance;

        private int _lastDraggingPoint;
        private Keyframe _lastDraggingKeyframe;

        private struct InstantObject
        {
            public ECClipVariable variable;
            public GameObject obj;
        }
        
        private Dictionary<string, InstantObject> instantObjects = new();
        private List<ECTrackWatchState> lastWatchState = new();
        private bool recording = false;

        private Rect rectTopBar;
        private Rect rectLeft;
        private Rect rectRight;
        public Rect rectLeftTopToolBar;

        [FormerlySerializedAs("editingClip")] public ECClip clip;

        private float _lastUpdateTime = 0f;

        #region Used

        private double runningTime = 0.0f;

        public override double RunningTime
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

            Instance = window;

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
                        var data = track.Seek(this, this.RunningTime);

                        if (data != null)
                        {
                            var trackType = ECTypes.GetTrackType(track.typeStr);
                            trackType.RuntimePlay(this, track, this, data);
                        }

                        /*
                        if (track.type == ECTrackType_Old.Transform)
                        {
                            if (instantObjects.ContainsKey(track.go_variableName))
                            {
                                var io = instantObjects[track.go_variableName];
                                ECCinema.TransformPlayFromData(io.obj, data);
                            }
                        }
                        */
                    }
                }
            }
            
            for (int i = lastWatchState.Count - 1; i >= 0; i--)
            {
                var state = lastWatchState[i];
                var trackType = ECTypes.GetTrackType(state.track.typeStr);
                var newState = trackType.CompareWatchTrack(state, state.track);
                if (newState)
                {
                    state = trackType.CreateNewWatchState(state.track, this);
                    lastWatchState[i] = state;
                    
                    var frame = Mathf.RoundToInt((float) runningTime / ECConstants.SecondPerFrame);
                    //Debug.Log($"[{i}] Change Detected at {frame}");

                    var track = state.track;
                    
                    var kfIdx = track.FindKeyframeIdx(frame);

                    if (kfIdx >= 0)
                    {
                        track.keyframes[kfIdx].data = trackType.BuildWatchKeyframeData(track, state);
                    }
                    else
                    {
                        var kf = trackType.CreateEmptyKeyframe(track, frame, frame);
                        kf.data = trackType.BuildWatchKeyframeData(track, state);
                        track.keyframes.Add(kf);
                    }
                }
            }
            
            _lastUpdateTime = (float)EditorApplication.timeSinceStartup;
            Repaint();
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

                ECTrackType type = ECTypes.GetTrackType(track.typeStr);
                ECTrackTypeInspector inspector = ECEditorTypes.GetTrackInspector(type.GetType());

                Texture icon = null;
                
                if (inspector != null)
                {
                    icon = inspector.Icon();
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
                        foreach (var track in clip.tracks)
                        {
                            var typ = ECTypes.GetTrackType(track.typeStr);
                            lastWatchState.Add(typ.CreateNewWatchState(track, this));
                        }
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
                        var px = TimeToPixel(keyframe.start * ECConstants.SecondPerFrame);
                        var width = TimeToPixel(keyframe.end * ECConstants.SecondPerFrame) - px;
                        

                        var btn = new GUIStyle(GUI.skin.button);
                        btn.normal.background = Texture2D.grayTexture;
                        
                        if (ECInspectorWindow.currentInspecting == keyframe)
                        {
                            btn.normal.background = Texture2D.linearGrayTexture;
                        }

                        int dragPoint = 1;

                        var rect = new Rect(offsetX + px - LEFTWIDTH * 2, 6 + i * 26, width, 12);

                        var st = offsetX + TimeToPixel(keyframe.start * ECConstants.SecondPerFrame) - LEFTWIDTH * 2 - 6;
                        var ed = offsetX + TimeToPixel(keyframe.end * ECConstants.SecondPerFrame) - LEFTWIDTH * 2 - 6;
                        var distFromStart = Mathf.Abs(st - eventPos.x);
                        var distFromEnd = Mathf.Abs(ed - eventPos.x);

                        if (distFromStart <= 15)
                        {
                            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
                            dragPoint = 0;
                        } else if (distFromEnd <= 15)
                        {
                            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
                            dragPoint = 2;
                        }
                        else
                        {
                        }

                        if (GUI.Button(rect,
                                "",
                                btn))
                        {
                            ECInspectorWindow.InspectSomething(keyframe);
                        }
                        
                        if (eventType == EventType.MouseDown && rect.Contains(eventPos)) {
                            _lastDraggingKeyframe = keyframe;
                            _lastDraggingPoint = dragPoint;
                        }
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

                if (_lastDraggingKeyframe.track.hasDuration)
                {
                    if (_lastDraggingPoint == 0)
                    {
                        if (_lastDraggingKeyframe.end - f >= 2)
                        {
                            _lastDraggingKeyframe.start = f;
                        }
                    } else if (_lastDraggingPoint == 2)
                    {
                        if (f - _lastDraggingKeyframe.start >= 2)
                        {
                            _lastDraggingKeyframe.end = f;
                        }
                    }
                    else
                    {
                        var beforeDistance = _lastDraggingKeyframe.end - _lastDraggingKeyframe.start;
                        
                        _lastDraggingKeyframe.start = f - beforeDistance / 2;
                        _lastDraggingKeyframe.end = f + beforeDistance / 2;
                    }
                }
                else
                {
                    if (f >= 0)
                    {
                        _lastDraggingKeyframe.start = f;
                        _lastDraggingKeyframe.end = f;
                    }
                }
            }
            
            if (_lastDraggingKeyframe != null && eventType == EventType.MouseUp) {
                ECInspectorWindow.InspectSomething(_lastDraggingKeyframe);
                _lastDraggingKeyframe = null;
            }
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void Stop()
        {
        }

        public void AddTime(double t)
        {
            RunningTime += t;
        }

        public GameObject GetGameObject(string varName)
        {
            //Debug.Log($"Accessing: {varName} - {instantObjects.ContainsKey(varName)}");
            
            if (!instantObjects.ContainsKey(varName)) return null;
            
            var obj = instantObjects[varName];
            
            return obj.obj;
        }

        public int GetInt(string varName)
        {
            // todo
            return 0;
        }

        public float GetFloat(string varName)
        {
            // todo
            return 0.0f;
        }
    }
}