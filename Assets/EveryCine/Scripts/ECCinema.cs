using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EveryCine
{
    public class ECCinema : MonoBehaviour
    {
        public static bool TransformPlayFromData(GameObject obj, string data)
        {
            var parsed = ECKeyframeParser.ParseTransform(data);

            if (float.IsNaN(parsed.Item1.x) || float.IsNaN(parsed.Item2.x) ||
                float.IsNaN(parsed.Item3.x) ||
                float.IsNaN(parsed.Item1.y) || float.IsNaN(parsed.Item2.y) ||
                float.IsNaN(parsed.Item3.y) ||
                float.IsNaN(parsed.Item1.z) || float.IsNaN(parsed.Item2.z) ||
                float.IsNaN(parsed.Item3.z))
            {
                return false;
            }
            else
            {
                obj.transform.localPosition = parsed.Item1;
                obj.transform.localRotation = Quaternion.Euler(parsed.Item2);
                obj.transform.localScale = parsed.Item3;
                return true;
            }
        }
        
        public class ECShow : ECCinemable, ECShowable
        {
            private float _showTime = 0.0f;
            private ECCinema _cinema;
            private ECClip _clip;

            public ECClip Clip
            {
                get => _clip;
            }

            private System.Guid _guid;

            public System.Guid ShowId
            {
                get => _guid;
            }
            
            public bool ShowPaused
            {
                get;
                private set;
            }

            public bool ShowEnded
            {
                get;
                private set;
            }
            
            internal Dictionary<string, GameObject> gameObjectVar = new();
            private Dictionary<string, int> intVar = new();
            private Dictionary<string, float> floatVar = new();

            public ECShow(ECCinema cinema, ECClip clip)
            {
                _guid = Guid.NewGuid();
                _cinema = cinema;
                _clip = clip;
            }

            public ECShow SetGameObject(string varName, GameObject obj)
            {
                gameObjectVar[varName] = obj;
                return this;
            }
            
            public ECShow SetInt(string varName, int value)
            {
                intVar[varName] = value;
                return this;
            }
            
            public ECShow SetFloat(string varName, float value)
            {
                floatVar[varName] = value;
                return this;
            }
            
            public GameObject GetGameObject(string varName)
            {
                return gameObjectVar[varName];
            }
            
            public int GetInt(string varName)
            {
                return intVar[varName];
            }
            
            public float GetFloat(string varName)
            {
                return floatVar[varName];
            }

            public ECShow Play()
            {
                _cinema.PlayShow(_clip, this);
                return this;
            }

            public void Step(float deltaTime)
            {
                int failCount = 0;

                if (ShowPaused && _showTime > 0)
                {
                    return;
                }
                
                _showTime += deltaTime;
                
                foreach (var track in _clip.tracks)
                {
                    var seek = track.Seek(this, _showTime);

                    if (seek == null)
                    {
                        failCount += 1;
                    }
                    else
                    {
                        var trackType = ECTypes.GetTrackType(track.typeStr);

                        bool result = trackType.RuntimePlay(this, track, this, seek);
                        if (!result)
                        {
                            failCount += 1;
                        }
                    }
                }

                if (failCount >= _clip.tracks.Count)
                {
                    Debug.Log($"[{ShowId}] ended");
                    ShowEnded = true;
                }
            }

            public override int GetHashCode()
            {
                return ShowId.GetHashCode();
            }

            public void Pause()
            {
                ShowPaused = true;
            }

            public void Resume()
            {
                ShowPaused = false;
            }

            public void Stop()
            {
                ShowEnded = true;
            }

            public void AddTime(double t)
            {
                _showTime += (float)t;
            }
        }

        private Dictionary<System.Guid, ECShow> _shows = new();
        
        public ECShow SetupAnimation(ECClip clip)
        {
            return new ECShow(this, clip);
        }

        public void PlayShow(ECClip clip, ECShow show)
        {
            _shows[show.ShowId] = show;
            foreach (var variable in clip.variables)
            {
                if (variable.go_instantiateIfNull)
                {
                    if (show.gameObjectVar[variable.varName] == null)
                    {
                        var go = Instantiate(variable.go_defaultPrefab);
                        show.gameObjectVar[variable.varName] = go;
                        go.AddComponent<ECClipInstantObject>();
                    }
                }
            }
            show.Step(0);
        }

        private void Update()
        {
            var shouldDisappear = new List<ECShow>();
            foreach (var show in _shows.Values)
            {
                show.Step(Time.deltaTime);
                if (show.ShowEnded)
                {
                    shouldDisappear.Add(show);
                }
            }
            
            foreach (var show in shouldDisappear)
            {
                foreach (var variable in show.gameObjectVar)
                {
                    var varT = show.Clip.GetVariableDefinitionByName(variable.Key);
                    
                    if (varT.go_destroyWhenEnd)
                        Destroy(variable.Value);
                }

                _shows.Remove(show.ShowId);
            }
        }
    }
}