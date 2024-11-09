using FIMSpace.FEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FIcons
{
    [CustomEditor(typeof(FIcons_Manager))]
    public class FIcons_ManagerEditor : Editor
    {
        public static bool DrawRefs = false;
        public static bool DrawDebug = false;
        public static bool DrawDetails = false;

        public static bool containersDetails = true;
        public static int page = 1;

        private GUISkin skin;

        #region Properties

        private SerializedProperty sp_LoadAnimatorPrefab;
        private SerializedProperty sp_DefaultLoadSprite;
        private SerializedProperty sp_ErrorSprite;

        //private SerializedProperty sp_Spinner32;
        //private SerializedProperty sp_Spinner64;
        //private SerializedProperty sp_Spinner128;
        //private SerializedProperty sp_Spinner256;

        #endregion

        private void OnEnable()
        {
            skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

            sp_LoadAnimatorPrefab = serializedObject.FindProperty("LoadAnimatorPrefab");
            sp_DefaultLoadSprite = serializedObject.FindProperty("DefaultLoadSprite");
            sp_ErrorSprite = serializedObject.FindProperty("ErrorSprite");

            //sp_Spinner32 = serializedObject.FindProperty("Spinner32");
            //sp_Spinner64 = serializedObject.FindProperty("Spinner64");
            //sp_Spinner128 = serializedObject.FindProperty("Spinner128");
            //sp_Spinner256 = serializedObject.FindProperty("Spinner256");
        }


        public override void OnInspectorGUI()
        {
            FIcons_Manager targetScript = (FIcons_Manager)target;

            List<string> excluded = new List<string>
            {
                "LoadAnimatorPrefab",
                "DefaultLoadSprite",
                "ErrorSprite"
                //"Spinner32",
                //"Spinner64",
                //"Spinner128",
                //"Spinner256"
            };

            if (targetScript.ShowSimpleSettings)
            {
                excluded.Add("SkipDuration");
                excluded.Add("MaxLoadTimeDelay");
                excluded.Add("AllowUnloadSources");
                excluded.Add("FadesSpeed");
                excluded.Add("SpinnerAfter");
                excluded.Add("ScaleAsync");
                excluded.Add("ScalingQuality");
                excluded.Add("SpinningSpeed");
                excluded.Add("CallUnloadUnused");
            }

            GUILayout.Space(7f);
            EditorGUILayout.BeginVertical(FEditor.FGUI_Resources.BGInBoxStyle);
            DrawPropertiesExcluding(serializedObject, excluded.ToArray());
            EditorGUILayout.EndVertical();

            GUILayout.Space(4f);
            EditorGUILayout.LabelField("Used Memory: " + string.Format("{0:0.00}", System.Math.Round(targetScript.GetMemoryInUse(), 2)) + "MB / " + string.Format("{0:0.00}", System.Math.Round(targetScript.MaxMemoryUsage, 2)) + "MB");
            GUILayout.Space(2f);

            DrawManagerReferences();

            DrawDebugStuff(targetScript);

            EditorUtility.SetDirty(target);

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawManagerReferences()
        {
            GUIStyle bold = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

            GUILayout.Space(4f);
            EditorGUILayout.BeginVertical(FEditor.FGUI_Resources.BGInBoxStyle);

            #region References Tab

            EditorGUI.indentLevel++;
            DrawRefs = EditorGUILayout.Foldout(DrawRefs, new GUIContent(" Draw Helper References", "References of helper sprites for manager"), true, bold);
            EditorGUI.indentLevel--;

            if (DrawRefs)
            {
                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 1, 3);

                EditorGUILayout.PropertyField(sp_LoadAnimatorPrefab);
                EditorGUILayout.PropertyField(sp_DefaultLoadSprite);
                EditorGUILayout.PropertyField(sp_ErrorSprite);

                //EditorGUILayout.PropertyField(sp_Spinner32);
                //EditorGUILayout.PropertyField(sp_Spinner64);
                //EditorGUILayout.PropertyField(sp_Spinner128);
                //EditorGUILayout.PropertyField(sp_Spinner256);
            }

            EditorGUILayout.EndVertical();

            #endregion

            GUILayout.Space(2f);
        }


        private void DrawDebugStuff(FIcons_Manager targetScript)
        {
            GUIStyle bold = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

            GUILayout.Space(4f);
            EditorGUILayout.BeginVertical(FEditor.FGUI_Resources.BGInBoxStyle);


            #region Debug Info Tab

            EditorGUI.indentLevel++;
            DrawDebug = EditorGUILayout.Foldout(DrawDebug, new GUIContent(" Draw Debug Info", "Info about loaded textures and sprites"), true, bold);
            EditorGUI.indentLevel--;

            if (DrawDebug)
            {
                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 1, 3);

                EditorGUILayout.LabelField(" -  Sprites Used Memory: " + string.Format("{0:0.00}", System.Math.Round(targetScript.GetMemoryInUse(false), 2)) + "MB");
                float memoryUsed = targetScript.GetMemoryInUse();
                bool exceed = false;
                string but = "";
                if (memoryUsed > targetScript.MaxMemoryUsage) { but = " !!!"; exceed = true; }

                EditorGUILayout.LabelField(" -  Used Memory (+Textures): " + string.Format("{0:0.00}", System.Math.Round(memoryUsed, 2)) + "MB / " + string.Format("{0:0.00}", System.Math.Round(targetScript.MaxMemoryUsage, 2)) + "MB" + but);
                EditorGUILayout.LabelField(" -  Loaded Textures: " + targetScript.GetTexturesCount());
                EditorGUILayout.LabelField(" -  Generated Sprites: " + targetScript.GetSpritesCount());

                GUILayout.Space(4f);
                EditorGUILayout.LabelField(" -  UIImages In Use: " + targetScript.GetImagesUseCount());
                EditorGUILayout.LabelField(" -  Loading Tasks: " + targetScript.GetLoadTasksCount());
                EditorGUILayout.LabelField(" -  Sprite Requests: " + targetScript.GetContainersRequestsCount());
                EditorGUILayout.LabelField(" -  Shared Sprites Use: " + targetScript.GetSharedSpritesUseCount());
                but = "";
                if (!exceed) but = " (limit not exceeded)";
                EditorGUILayout.LabelField(" -  Can Be Unloaded: " + targetScript.GetReadyToUnloadCount() + but);
                //EditorGUILayout.LabelField(" -  Unloaded: " + targetScript.GetUnloadedCount());

                GUILayout.Space(4f);
                EditorGUILayout.LabelField(" -  Textures To Load: " + targetScript.GetTexturesToLoadCount());
                EditorGUILayout.LabelField(" -  Sprites To Assign: " + targetScript.GetSpritesToAssignCount());
                GUILayout.Space(4f);


                string info = "Unloading all source textures from texture containers (generated sprites will remain). You can do it if you don't want to keep texture files in memory, textures are needed to generate new scaled sprites. If you are sure you will not generate new scaled sprites feel free to call this method.";

                if (Application.isPlaying)
                {
                    if (targetScript.GetTexturesCount() > 0)
                    {
                        if (GUILayout.Button(new GUIContent("Unload Textures (Advanced)", info)))
                            targetScript.UnloadSourceTextures();
                    }
                }
                else
                {
                    Color preCol = GUI.color;
                    GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.6f);
                    if (GUILayout.Button(new GUIContent("Unload Textures (Risky - check tooltip)", info))) { Debug.Log("[ICONS MANAGER] Unloading Textures available only in playmode!"); }
                    GUI.color = preCol;
                }
            }

            EditorGUILayout.EndVertical();

            #endregion


            GUILayout.Space(4f);


            #region Containers & Sprites Info Tab


            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(FEditor.FGUI_Resources.BGInBoxStyle);


            DrawDetails = EditorGUILayout.Foldout(DrawDetails, new GUIContent(" Draw Details", "Details about each loaded texture or generated sprite"), true, bold);

            if (DrawDetails)
            {
                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 1, 3);

                Color preCol = GUI.color;

                EditorGUILayout.BeginHorizontal();

                if (containersDetails) GUI.color = new Color(0.3f, 1f, 0.3f, 0.95f);
                if (GUILayout.Button("Containers (Full Textures)")) { page = 1; RefreshLists(); containersDetails = !containersDetails; }
                GUI.color = preCol;

                if (!containersDetails) GUI.color = new Color(0.3f, 1f, 0.3f, 0.95f);
                if (GUILayout.Button("Sprites")) { page = 1; RefreshLists(); containersDetails = !containersDetails; }
                GUI.color = preCol;

                EditorGUILayout.EndHorizontal();

                bool dataToShow = true;
                Dictionary<int, FIcon_TextureContainer> containers = null;

                if (!Application.isPlaying)
                {
                    dataToShow = false;
                }
                else
                {
                    containers = FIcons_Manager.Get.GetContainers();
                    if (containers == null) dataToShow = false; else if (containers.Count == 0) dataToShow = false;
                }

                if (dataToShow)
                {
                    if (containersDetails)
                    {
                        DrawContainers(containers);
                    }
                    else
                    {
                        DrawSprites(containers);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("There is no data to show right now", MessageType.Info);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            #endregion

        }

        private void RefreshLists()
        {
            tContainers.Clear();
            sprites.Clear();
        }

        private List<FIcon_TextureContainer> tContainers = new List<FIcon_TextureContainer>();
        private void DrawContainers(Dictionary<int, FIcon_TextureContainer> containersDictionary)
        {
            int itemsPerPage = 15;

            if (tContainers.Count != containersDictionary.Values.Count)
            {
                tContainers.Clear();

                foreach (var container in containersDictionary.Values)
                    tContainers.Add(container);
            }

            int pages = (tContainers.Count - 1) / itemsPerPage + 1;
            if (page > pages) page = 1;

            for (int i = 0; i < itemsPerPage; i++)
            {
                int index = (page - 1) * itemsPerPage + i;
                if (index >= tContainers.Count) break;

                FIcon_TextureContainer container = tContainers[index];

                Rect r = EditorGUILayout.GetControlRect(false, 24);

                GUI.BeginGroup(r, skin.GetStyle("TE NodeBox"));

                int lab = 0;
                if (container.SourceTextureLoadState == FE_TextureLoadState.Loaded) lab = 3;
                else if (container.SourceTextureLoadState == FE_TextureLoadState.Error) lab = 5;
                else if (container.SourceTextureLoadState == FE_TextureLoadState.Loading) lab = 1;

                GUI.Label(new Rect(5, 6, 62, 22), container.SourceTextureLoadState.ToString(), skin.GetStyle("sv_label_" + lab));

                string info = "";
                info = Path.GetFileNameWithoutExtension(container.PathToSourceTexture);
                if (container.SourcePixels != null && container.SourcePixels.Length > 0)
                {
                    info += " | " + container.SourceWidth + "x" + container.SourceHeight;
                    info += " | " + System.Math.Round(container.TotalSizeInMB, 2) + "mb";
                    info += " | " + container.GetSpritesCount() + " Scaled Sprites";
                    info += " | " + (container.GetSpritesCount() + container.GetSharedCount()) + " References";
                    info += " | Last Use: " + container.LastUse;
                }
                else
                {
                    if (container.SourceTextureLoadState == FE_TextureLoadState.Unloaded)
                    {
                        info += " | " + container.SourceWidth + "x" + container.SourceHeight;
                        info += " | " + container.GetSpritesCount() + " Scaled Sprites";
                        info += " | " + (container.GetSpritesCount() + container.GetSharedCount()) + " References";
                        info += " | Last Use: " + container.LastUse;
                    }
                    else
                        info += " Waiting to load...";
                }

                GUI.Label(new Rect(72, 5, r.width - 64, 18), info, skin.GetStyle("label"));
                GUI.EndGroup();

                GUILayout.Space(1f);
            }

            if (pages > 1)
            {
                GUILayout.Space(3f);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<")) page--; if (page < 1) page = pages;

                EditorGUILayout.LabelField(" " + page + " / " + pages + " ", new GUILayoutOption[1] { GUILayout.MaxWidth(70) });

                if (GUILayout.Button(">")) page++; if (page > pages) page = 1;
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }
        }

        private List<FIcon_GeneratedSprite> sprites = new List<FIcon_GeneratedSprite>();
        private void DrawSprites(Dictionary<int, FIcon_TextureContainer> containers)
        {
            int itemsPerPage = 15;

            if (sprites.Count != FIcons_Manager.Get.GetSpritesCount())
            {
                sprites.Clear();

                foreach (var container in containers.Values)
                    foreach (var spr in container.GetGeneratedSprites().Values)
                        sprites.Add(spr);
            }

            int pages = (sprites.Count - 1) / itemsPerPage + 1;
            if (page > pages) page = 1;


            for (int i = 0; i < itemsPerPage; i++)
            {
                int index = (page - 1) * itemsPerPage + i;
                if (index >= sprites.Count) break;

                FIcon_GeneratedSprite sprite = sprites[index];

                Rect r = EditorGUILayout.GetControlRect(false, 24);

                GUI.BeginGroup(r, skin.GetStyle("TE NodeBox"));

                int lab = 0;
                if (sprite.LoadState == FE_SpriteLoadState.Loaded) lab = 3;
                else if (sprite.LoadState == FE_SpriteLoadState.Loading) lab = 1;

                GUI.Label(new Rect(5, 6, 62, 22), sprite.LoadState.ToString(), skin.GetStyle("sv_label_" + lab));

                string info = "";
                info = "";

                if (sprite.GeneratedSprite != null)
                {
                    info = sprite.GeneratedSprite.name;
                    info += " | ID: " + sprite.Id;
                    info += " | " + System.Math.Round(sprite.SizeInMB, 2) + "mb";
                    info += " | " + (sprite.References.Count) + " References";
                    if (sprite.WaitingFor) info += " | Waiting For";
                    info += " | Active: " + sprite.ActiveUsers;
                    info += " | Last Use: " + sprite.LastUse;
                }
                else
                {
                    if (sprite.LoadState != FE_SpriteLoadState.Unloaded) info += " Waiting to load...";
                    if (sprite.TexturePath.Length > 0) info += " | " + sprite.TexturePath;
                    info += " | ID: " + sprite.Id + " " + sprite.Width + "x" + sprite.Height;
                    if (sprite.References != null) info += " | References: " + sprite.References.Count;
                    if (sprite.References != null) info += " | Filter: " + sprite.Filter;
                    info += " | Active: " + sprite.ActiveUsers;
                }

                GUI.Label(new Rect(72, 5, r.width - 64, 18), info, skin.GetStyle("label"));
                GUI.EndGroup();

                GUILayout.Space(1f);
            }

            if (pages > 1)
            {
                GUILayout.Space(3f);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<")) page--; if (page < 1) page = pages;

                EditorGUILayout.LabelField(" " + page + " / " + pages + " ", new GUILayoutOption[1] { GUILayout.MaxWidth(70) });

                if (GUILayout.Button(">")) page++; if (page > pages) page = 1;
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }

        }
    }
}
