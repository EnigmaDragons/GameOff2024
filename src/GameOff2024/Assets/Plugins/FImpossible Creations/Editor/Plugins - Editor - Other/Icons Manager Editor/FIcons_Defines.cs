// To detect Addressables we must be outside Editor directory
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    /// <summary>
    /// FM: Class to help define if adressables are imported
    /// </summary>
    [InitializeOnLoad]
    sealed class FIcons_Defines
    {
        const string define = "FICONS_ADDRESSABLES_IMPORTED";
        const string define2 = "FICONS_ENCODERSIMPORTED";

        static FIcons_Defines()
        {
            if (FDefinesCompilation.GetTypesInNamespace("UnityEngine.AddressableAssets", "").Count > 0)
                FDefinesCompilation.SetDefine(define);
            else
                FDefinesCompilation.RemoveDefine(define);

            try
            {
                if (System.Reflection.Assembly.LoadFrom(Application.dataPath + "/Plugins/x64/PresentationCore.dll") != null)
                {
                    FDefinesCompilation.SetDefine(define2);
                }
                else
                {
                    FDefinesCompilation.RemoveDefine(define2);
                }
            }
            catch (Exception)
            {
                FDefinesCompilation.RemoveDefine(define2);
            }
        }
    }
}