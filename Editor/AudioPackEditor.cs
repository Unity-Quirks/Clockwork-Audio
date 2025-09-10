#if UNITY_EDITOR

using Clockwork.Audio.Editor;
using UnityEditor;
using UnityEngine;

namespace Quirks.Audio.Editor
{
    [CustomEditor(typeof(AudioPack), true)]
    public class AudioPackEditor : UnityEditor.Editor
    {
        SerializedProperty audioClipsProperty;
        SerializedProperty mixerGroupProperty;
        SerializedProperty playVolumeProperty;
        SerializedProperty useRandomPitchProperty;
        SerializedProperty pitchRangeProperty;
        SerializedProperty spatialBlendProperty;
        SerializedProperty rolloffModeProperty;
        SerializedProperty minDistanceProperty;
        SerializedProperty maxDistanceProperty;
        SerializedProperty dopplerLevelProperty;

        // Music-specific properties
        SerializedProperty reverbTailProperty;

        // Preview control variables
        bool isPreviewPlaying = false;
        float currentDecibelLevel = -80f;
        float decibelUpdateTimer = 0f;
        const float DECIBEL_UPDATE_RATE = 0.05f; // Update 20 times per second

        void OnEnable()
        {
            // Find all serialized properties
            audioClipsProperty = serializedObject.FindProperty("audioClips");
            mixerGroupProperty = serializedObject.FindProperty("mixerGroup");
            playVolumeProperty = serializedObject.FindProperty("playVolume");
            useRandomPitchProperty = serializedObject.FindProperty("useRandomPitch");
            pitchRangeProperty = serializedObject.FindProperty("pitchRange");
            spatialBlendProperty = serializedObject.FindProperty("spatialBlend");
            rolloffModeProperty = serializedObject.FindProperty("rolloffMode");
            minDistanceProperty = serializedObject.FindProperty("minDistance");
            maxDistanceProperty = serializedObject.FindProperty("maxDistance");
            dopplerLevelProperty = serializedObject.FindProperty("dopplerLevel");

            // Music pack specific
            reverbTailProperty = serializedObject.FindProperty("reverbTail");

            // Subscribe to preview events
            AudioPreviewer.OnPreviewStarted += OnPreviewStarted;
            AudioPreviewer.OnPreviewStopped += OnPreviewStopped;
            AudioPreviewer.OnDecibelUpdate += OnDecibelUpdate;
        }

        void OnDisable()
        {
            // Unsubscribe from preview events
            AudioPreviewer.OnPreviewStarted -= OnPreviewStarted;
            AudioPreviewer.OnPreviewStopped -= OnPreviewStopped;
            AudioPreviewer.OnDecibelUpdate -= OnDecibelUpdate;
        }

        void OnPreviewStarted()
        {
            isPreviewPlaying = true;
            currentDecibelLevel = -80f;
            Repaint();
        }

        void OnPreviewStopped()
        {
            isPreviewPlaying = false;
            currentDecibelLevel = -80f;
            Repaint();
        }

        void OnDecibelUpdate(float decibelLevel)
        {
            currentDecibelLevel = decibelLevel;

            // Throttle repaints to avoid performance issues
            decibelUpdateTimer += Time.realtimeSinceStartup;
            if (decibelUpdateTimer >= DECIBEL_UPDATE_RATE)
            {
                decibelUpdateTimer = 0f;
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space();
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            EditorGUILayout.LabelField("Audio Pack Settings", headerStyle);

            EditorGUILayout.Space();

            // Audio Base Section
            DrawSection("Audio Base", () =>
            {
                EditorGUILayout.PropertyField(audioClipsProperty, new GUIContent("Audio Clips", "Collection of audio clips in this pack"));
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(mixerGroupProperty, new GUIContent("Mixer Group", "Audio mixer group for this pack"));
                EditorGUILayout.PropertyField(playVolumeProperty, new GUIContent("Play Volume", "Volume level for this audio pack"));
            });

            // Pitch Settings Section
            DrawSection("Pitch Settings", () =>
            {
                EditorGUILayout.PropertyField(useRandomPitchProperty, new GUIContent("Use Random Pitch", "Enable random pitch variation"));

                if (useRandomPitchProperty.boolValue)
                {
                    EditorGUILayout.Space(5);
                    DrawMinMaxSlider("Pitch Range", pitchRangeProperty, 0f, 5f);

                    // Show current values
                    Vector2 pitchRange = pitchRangeProperty.vector2Value;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Min: {pitchRange.x:F2}", GUILayout.Width(70));
                    EditorGUILayout.LabelField($"Max: {pitchRange.y:F2}", GUILayout.Width(70));
                    EditorGUILayout.EndHorizontal();
                }
            });

            // 3D Audio Settings Section
            DrawSection("3D Audio Settings", () =>
            {
                EditorGUILayout.PropertyField(spatialBlendProperty, new GUIContent("Spatial Blend", "How 3D the audio is (0 = 2D, 1 = fully 3D)"));
                EditorGUILayout.PropertyField(rolloffModeProperty, new GUIContent("Rolloff Mode", "How audio volume decreases with distance"));

                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(minDistanceProperty, new GUIContent("Min Distance", "Distance at which audio starts to fade"));
                EditorGUILayout.PropertyField(maxDistanceProperty, new GUIContent("Max Distance", "Distance at which audio is completely silent"));
                EditorGUILayout.PropertyField(dopplerLevelProperty, new GUIContent("Doppler Level", "Doppler effect strength"));
            });

            // Music-specific settings
            if (target is MusicPack && reverbTailProperty != null)
            {
                DrawSection("Music Settings", () =>
                {
                    EditorGUILayout.PropertyField(reverbTailProperty, new GUIContent("Reverb Tail", "Reverb tail duration for smooth transitions"));
                });
            }

            // Info Section
            if (audioClipsProperty.arraySize > 0)
            {
                DrawSection("Pack Information", () =>
                {
                    EditorGUILayout.LabelField($"Total Clips: {audioClipsProperty.arraySize}");

                    float shortestClip = float.MaxValue;
                    float longestClip = 0f;
                    float totalDuration = 0f;
                    AudioPack audioPack = (AudioPack)target;
                    for (int i = 0; i < audioPack.ClipCount; i++)
                    {
                        AudioClip clip = audioPack.AudioClips[i];

                        if (clip != null)
                        {
                            if (clip.length < shortestClip)
                                shortestClip = clip.length;

                            if (clip.length > longestClip)
                                longestClip = clip.length;

                            totalDuration += clip.length;
                        }
                    }

                    if (audioPack.ClipCount > 1)
                    {
                        EditorGUILayout.LabelField($"Shortest Clip: {shortestClip:F2} seconds");
                        EditorGUILayout.LabelField($"Longest Clip: {longestClip:F2} seconds");
                    }

                    EditorGUILayout.LabelField($"Total Duration: {totalDuration:F2} seconds");
                });
            }

            // Preview Section
            if (audioClipsProperty.arraySize > 0 && !Application.isPlaying)
            {
                DrawSection("Preview", () =>
                {
                    EditorGUILayout.BeginHorizontal();

                    // Play/Stop buttons
                    if (!isPreviewPlaying)
                    {
                        if (GUILayout.Button("Play Random Clip"))
                        {
                            AudioPack audioPack = (AudioPack)target;
                            AudioClip randomClip = audioPack.GetRandomClip();
                            if (randomClip != null)
                            {
                                Vector3 position = Camera.main ? Camera.main.transform.position : Vector3.zero;
                                float pitch = useRandomPitchProperty.boolValue ? Random.Range(pitchRangeProperty.vector2Value.x, pitchRangeProperty.vector2Value.y) : 1f;

                                AudioPreviewer.PlayClipAtPoint(randomClip, position, audioPack.playVolume, pitch);
                            }
                        }
                    }
                    else
                    {
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("Stop Preview"))
                        {
                            AudioPreviewer.StopCurrentPreview();
                        }
                        GUI.backgroundColor = Color.white;
                    }

                    EditorGUILayout.EndHorizontal();

                    // Decibel Meter
                    if (isPreviewPlaying)
                    {
                        EditorGUILayout.Space(10);
                        DrawDecibelMeter();
                    }
                });
            }

            serializedObject.ApplyModifiedProperties();
        }

        // TODO make work with pitch
        // Pitch changes time
        void DrawDecibelMeter()
        {
            EditorGUILayout.LabelField("Audio Level", EditorStyles.boldLabel);

            // Convert decibel to normalized value for display (assuming -80dB to 0dB range)
            float normalizedLevel = Mathf.InverseLerp(-80f, 0f, currentDecibelLevel);

            // Draw the meter background
            Rect meterRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(meterRect, Color.black);

            // Draw the level bar
            Rect levelRect = new Rect(meterRect.x, meterRect.y, meterRect.width * normalizedLevel, meterRect.height);

            // Color based on level (green -> yellow -> red)
            Color levelColor;
            if (normalizedLevel < 0.7f)
                levelColor = Color.Lerp(Color.green, Color.yellow, normalizedLevel / 0.7f);
            else
                levelColor = Color.Lerp(Color.yellow, Color.red, (normalizedLevel - 0.7f) / 0.3f);

            EditorGUI.DrawRect(levelRect, levelColor);

            // Draw level text
            string levelText = currentDecibelLevel > -80f ? $"{currentDecibelLevel:F1} dB" : "Silent";
            GUI.Label(meterRect, levelText, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        void DrawSection(string title, System.Action DrawContent)
        {
            EditorGUILayout.Space();

            // Section header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            EditorGUILayout.LabelField(title, headerStyle);

            // Draw a line cause pretty
            Rect rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.3f, 0.3f, 0.3f));

            EditorGUILayout.Space(5);

            // Content
            EditorGUI.indentLevel++;
            DrawContent();
            EditorGUI.indentLevel--;
        }

        void DrawMinMaxSlider(string label, SerializedProperty property, float minLimit, float maxLimit)
        {
            Vector2 range = property.vector2Value;
            float minValue = range.x;
            float maxValue = range.y;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

            minValue = EditorGUILayout.FloatField(minValue, GUILayout.Width(50));

            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);

            maxValue = EditorGUILayout.FloatField(maxValue, GUILayout.Width(50));

            EditorGUILayout.EndHorizontal();

            // Clamp values
            if (minValue > maxValue)
                minValue = maxValue;

            minValue = Mathf.Clamp(minValue, minLimit, maxLimit);
            maxValue = Mathf.Clamp(maxValue, minLimit, maxLimit);

            property.vector2Value = new Vector2(minValue, maxValue);
        }
    }
}

#endif