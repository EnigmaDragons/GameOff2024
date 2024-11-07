using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FIcons
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FIcon_Loader))]
    public class FIcon_LoaderEditor : Editor
    {
        public static bool drawCompInfo = true;

        public SerializedProperty s_NewHeight;
        public SerializedProperty s_TargetFilter;
        public SerializedProperty s_LoadingAnimation;
        public SerializedProperty s_ScaleDownSupport;
        public SerializedProperty s_LoadOnStart;
        public SerializedProperty s_LossyScaleSync;

        private void OnEnable()
        {
            s_NewHeight = serializedObject.FindProperty("NewHeight");
            s_TargetFilter = serializedObject.FindProperty("TargetFilter");
            s_LoadingAnimation = serializedObject.FindProperty("LoadingAnimation");
            s_ScaleDownSupport = serializedObject.FindProperty("ScaleDownSupport");
            s_LoadOnStart = serializedObject.FindProperty("LoadOnStart");
            s_LossyScaleSync = serializedObject.FindProperty("LossyScaleSync");

            drawCompInfo = CheckToShowInfo();
        }

        public override void OnInspectorGUI()
        {
            FIcon_Loader targetScript = (FIcon_Loader)target;

            #region Component info message

            if (drawCompInfo)
            {
                UnityEditor.EditorGUILayout.HelpBox("It's recommended to use IconsManager from code with 'FIcons_Manager.Get' methods, but nothing stops you from using just this component.", UnityEditor.MessageType.Info);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent("OK")))
                    ClickInfo();

                if (GUILayout.Button(new GUIContent("OK and I will look into User Manual")))
                    ClickInfo();

                EditorGUILayout.EndHorizontal();
            }

            #endregion

            List<string> exclude = new List<string>();
            if (targetScript.LoadingPath == FE_LoadingPath.OtherTexture)
            {
                exclude.Add("PathTo");
                exclude.Add("LoadOnStart");
            }
            else
            {
                exclude.Add("OtherTexture");
                exclude.Add("LoadOnStart");
            }

            EditorGUIUtility.labelWidth = 145;

            GUILayout.Space(7f);
            EditorGUILayout.BeginVertical(FEditor.FGUI_Resources.BGInBoxStyle);
            DrawPropertiesExcluding(serializedObject, exclude.ToArray());
            EditorGUILayout.EndVertical();

            GUILayout.Space(4f);

            EditorGUILayout.BeginVertical(FEditor.FGUI_Resources.BGInBoxStyle);
            EditorGUI.indentLevel++;
            targetScript.Advanced = EditorGUILayout.Foldout(targetScript.Advanced, new GUIContent("Advanced", "Showing advanced icon loading options"), true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
            EditorGUI.indentLevel--;

            EditorGUIUtility.labelWidth = 140;

            if (targetScript.Advanced)
            {
                FEditor.FGUI_Inspector.DrawUILine(Color.white * 0.35f, 2, 4);

                if (targetScript.NewHeight == 0)
                {
                    bool pre = targetScript.NewHeight == 0;
                    pre = EditorGUILayout.Toggle("Preserve Aspect", pre);
                    if (pre == false) targetScript.NewHeight = targetScript.NewWidth;
                }
                else
                    EditorGUILayout.PropertyField(s_NewHeight);

                Color preCol = GUI.color;
                if (targetScript.ScaleDownSupport && targetScript.TargetFilter == FilterMode.Point) GUI.color = new Color(1f, 0.8f, 0.5f, 0.9f);
                EditorGUILayout.PropertyField(s_TargetFilter);
                GUI.color = preCol;

                EditorGUILayout.PropertyField(s_LoadingAnimation);
                EditorGUILayout.PropertyField(s_ScaleDownSupport);
                EditorGUILayout.PropertyField(s_LossyScaleSync);
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndVertical();

            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(s_LoadOnStart);
            GUILayout.Space(2f);

            if (Application.isPlaying)
            {
                GUILayout.Space(4f);
                if (GUILayout.Button("Reload Image")) targetScript.Refresh();
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region Miscellaneous

        const string INFO_ID = "FIcon_LoaderInfo";
        private bool CheckToShowInfo()
        {
            int showIt = PlayerPrefs.GetInt(INFO_ID);

            int session = System.Diagnostics.Process.GetCurrentProcess().Id;
            int lastSessionId = PlayerPrefs.GetInt(INFO_ID + "_S");

            if (session != lastSessionId)
            {
                PlayerPrefs.SetInt(INFO_ID + "_S", session);
                PlayerPrefs.SetInt(INFO_ID, 1);
                showIt = 1;
            }

            return showIt == 1;
        }

        private void ClickInfo()
        {
            PlayerPrefs.SetInt(INFO_ID, 0);
            drawCompInfo = false;
        }

        #endregion
    }
}