using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(UIWidget), true)]
public class UIWidgetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UIWidget panel = target as UIWidget;
        SerializedObject so = new SerializedObject(target);


        SerializedProperty sp_links = so.FindProperty("compItemList");

        if (GUILayout.Button("AttachAllComp ( R_* )"))
        {
            sp_links.ClearArray();

            UIWidget[] weidgets = panel.gameObject.GetComponentsInChildren<UIWidget>();
            Dictionary<int, Transform> parentsDict = new Dictionary<int, Transform>();
            for (int i = 0; i < weidgets.Length; ++i)
            {
                parentsDict.Add(weidgets[i].transform.GetHashCode(), weidgets[i].transform);
            }

            Transform[] childs = panel.gameObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < childs.Length; ++i)
            {
                GameObject obj = childs[i].gameObject;
                if (obj.name.StartsWith("R_"))
                {
                    Debug.Log("Attach: " + obj.name);

                    Transform checkTrans = obj.transform.parent;
                    while (checkTrans != null)
                    {
                        int hashCode = checkTrans.GetHashCode();
                        if (parentsDict.ContainsKey(hashCode))
                        {
                            if (parentsDict[hashCode] == panel.transform)
                            {
                                AttachWidget(obj, panel.transform, sp_links);
                            }
                            break;
                        }
                        checkTrans = checkTrans.parent;
                    }

                    //AttachWidget(obj, panel.transform, sp_links);
                }
            }
        }

        if (GUILayout.Button("Copy All Comps Name"))
        {
            CopyCode(sp_links);
        }

        if (GUILayout.Button("Write Code To Base"))
        {
            CopyCode(sp_links);
            string className = panel.GetType().Name + "Base";
            string scriptPath = Application.dataPath + "/Scripts/Logic/UI/PanelsBase/" + className + ".cs";
            WriteCodeToBasePanel(scriptPath, className, GUIUtility.systemCopyBuffer);
        }

        GameObject go = EditorGUILayout.ObjectField(new GUIContent("Add Widget"), null, typeof(GameObject), true) as GameObject;

        if (go != null)
        {
            Transform t = go.transform.parent;
            bool inherited = false;
            do
            {
                if (t == panel.transform)
                {
                    inherited = true;
                    break;
                }
                t = t.parent;
            }
            while (t != null);

            if (inherited)
            {
                sp_links.InsertArrayElementAtIndex(sp_links.arraySize);
                SerializedProperty sp_link_item = sp_links.GetArrayElementAtIndex(sp_links.arraySize - 1);

                SerializedProperty sp_id = sp_link_item.FindPropertyRelative("Name");
                SerializedProperty sp_widget = sp_link_item.FindPropertyRelative("Obj");

                sp_id.stringValue = go.name;
                sp_widget.objectReferenceValue = (Object)go;
            }
        }

        EditorGUILayout.Space();

        for (int i = 0; i < sp_links.arraySize; ++i)
        {
            SerializedProperty sp_link_item = sp_links.GetArrayElementAtIndex(i);

            if (DrawItem(sp_link_item))
            {
                sp_links.DeleteArrayElementAtIndex(i);
                break;
            }
        }

        so.ApplyModifiedProperties();
        DrawDefaultInspector();
    }

    private void CopyCode(SerializedProperty sp_links) {
        // decl
        StringBuilder content = new StringBuilder();

        for (int i = 0; i < sp_links.arraySize; i++)
        {
            var item = sp_links.GetArrayElementAtIndex(i);
            var name = item.FindPropertyRelative("Name");
            string[] nameSplits = name.stringValue.Split('_');
            string compType = nameSplits[nameSplits.Length - 1];

            if (nameSplits.Length == 2)
            {
                content.AppendLine("[HideInInspector]");
                content.AppendLine(string.Format("public GameObject {0};", name.stringValue));
            }
            else
            {
                content.AppendLine("[HideInInspector]");
                content.AppendLine(string.Format("public {1} {0};", name.stringValue, compType));
            }
        }

        content.AppendLine("");

        content.AppendLine("protected override void OnInitCompos()");
        content.AppendLine("{");
        for (int i = 0; i < sp_links.arraySize; i++)
        {
            var item = sp_links.GetArrayElementAtIndex(i);
            var name = item.FindPropertyRelative("Name");
            string[] nameSplits = name.stringValue.Split('_');
            string compType = nameSplits[nameSplits.Length - 1];

            if (nameSplits.Length != 2)
            {
                content.AppendLine(string.Format("    {0} = base.GetComp<{1}>(\"{0}\");", name.stringValue, compType));
            }
            else
            {
                content.AppendLine(string.Format("    {0} = base.GetComp(\"{0}\");", name.stringValue));
            }
        }
        content.AppendLine("}");
        //content.AppendLine("end");
        GUIUtility.systemCopyBuffer = content.ToString();
    }

    private void WriteCodeToBasePanel(string scriptPath, string className, string codeStr) {
        Debug.Log(scriptPath);
        Debug.Log(className);

        StringBuilder content = new StringBuilder();
        content.AppendLine("using UnityEngine;");
        content.AppendLine("using UnityEngine.UI;");
        content.AppendLine("");
        content.AppendLine(string.Format("public class {0} : UIPanel", className));
        content.AppendLine("{");
        content.Append(codeStr);
        content.AppendLine("}");

        System.IO.File.WriteAllText(scriptPath, content.ToString());
        AssetDatabase.Refresh();
    }

    private void AttachWidget(GameObject go, Transform _trans, SerializedProperty sp_links)
    {
        if (go != null)
        {
            Transform t = go.transform.parent;
            bool inherited = false;
            do
            {
                if (t == _trans)
                {
                    inherited = true;
                    break;
                }
                t = t.parent;
            }
            while (t != null);

            if (inherited)
            {
                sp_links.InsertArrayElementAtIndex(sp_links.arraySize);
                SerializedProperty sp_link_item = sp_links.GetArrayElementAtIndex(sp_links.arraySize - 1);

                SerializedProperty sp_id = sp_link_item.FindPropertyRelative("Name");
                SerializedProperty sp_widget = sp_link_item.FindPropertyRelative("Obj");

                sp_id.stringValue = go.name;
                sp_widget.objectReferenceValue = (Object)go;
            }
        }
    }

    bool DrawItem(SerializedProperty sp_link_item)
    {
        bool to_delete = false;

        EditorGUILayout.BeginHorizontal(GUI.skin.textArea);

        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            to_delete = true;
        }

        SerializedProperty sp_id = sp_link_item.FindPropertyRelative("Name");
        SerializedProperty sp_widget = sp_link_item.FindPropertyRelative("Obj");

        EditorGUILayout.LabelField(sp_id.stringValue, GUILayout.MinWidth(20));
        EditorGUILayout.ObjectField(sp_widget.objectReferenceValue, typeof(GameObject), true);

        EditorGUILayout.EndHorizontal();

        return to_delete;
    }
}
