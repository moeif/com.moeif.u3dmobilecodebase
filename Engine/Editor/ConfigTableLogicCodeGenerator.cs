using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System;
using FlatBuffers;
using System.Text;
public class ConfigTableLogicCodeGenerator : EditorWindow
{
    [MenuItem("MoeTools/表代码生成器")]
    static void DoIt()
    {
        ConfigTableLogicCodeGenerator window = (ConfigTableLogicCodeGenerator)EditorWindow.GetWindow(typeof(ConfigTableLogicCodeGenerator));
        window.Show();
    }

    // 就是写在表里的那个表名
    private string tableName;

    private void OnGUI()
    {
        GUILayout.Space(30);
        GUILayout.Label("表名: ");
        tableName = GUILayout.TextField(tableName);
        GUILayout.Space(10);

        if (GUILayout.Button("生成"))
        {
            if (string.IsNullOrEmpty(tableName))
            {
                Debug.LogErrorFormat("表名不能为空");
                return;
            }

            GenerateCode(tableName);
        }

    }

    private void GenerateCode(string tableName)
    {
        string singleDataName = string.Format("Single{0}Data", tableName);
        Type type = typeof(IFlatbufferObject);
        Assembly assem = Assembly.GetAssembly(type);
        PropertyInfo[] pInfoArray = null;
        foreach (Type child in assem.GetTypes())
        {
            if (child.Name == singleDataName)
            {
                pInfoArray = child.GetProperties();
                break;
            }
        }

        if (pInfoArray != null)
        {
            GenerateFromProperties(tableName, pInfoArray);
        }
        else
        {
            Debug.LogErrorFormat("从类型 {0} 中找不到属性，请确保已执行 Flatbuffers 生成操作");
        }
    }

    internal class _NameWithType
    {
        public string name;
        public string typeStr;

        public _NameWithType(string name, string typeStr)
        {
            this.name = name;
            this.typeStr = typeStr;
        }
    }

    private void GenerateFromProperties(string tableName, PropertyInfo[] pInfoArray)
    {
        List<_NameWithType> nameWithTypeList = new List<_NameWithType>();
        foreach (PropertyInfo pInfo in pInfoArray)
        {
            if (pInfo.PropertyType == typeof(FlatBuffers.ByteBuffer))
            {
                continue;
            }
            Debug.LogFormat("{0} - {1}", pInfo.Name, pInfo.PropertyType);
            string typeStr = string.Empty;
            if (pInfo.PropertyType == typeof(System.String))
            {
                typeStr = "string";
            }
            else if (pInfo.PropertyType == typeof(System.Int32))
            {
                typeStr = "int";
            }
            else if (pInfo.PropertyType == typeof(System.Single))
            {
                typeStr = "float";
            }
            else
            {
                Debug.LogErrorFormat("发现未识别类型: {0}", pInfo.PropertyType);
                return;
            }

            nameWithTypeList.Add(new _NameWithType(pInfo.Name, typeStr));
        }


        // Generate Code
        string className = tableName + "Table";
        string codeFilePath = Application.dataPath + "/Scripts/ConfigTables/" + className + ".cs";

        string usingCodeStr = @"
using System.Collections;
using System.Collections.Generic;
using FlatBuffers;
using UnityEngine;

        ";

        // 生成属代码
        string propertyCodeStr = "";
        foreach (_NameWithType item in nameWithTypeList)
        {
            propertyCodeStr += string.Format("    public {0} {1};\n", item.typeStr, item.name);
        }

        string configDataClassName = tableName + "Data";
        string configDataCodeStr = @"
public class {0}
{
{1}
}

".Replace("{0}", configDataClassName).Replace("{1}", propertyCodeStr);


        // 生成加载代码
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(string.Format("public class {0} : MoeConfigTable<{1}, {2}>", className, className, configDataClassName));
        builder.AppendLine("{");
        builder.AppendLine(string.Format("    protected override string tableName => \"{0}\";\n", tableName));
        builder.AppendLine("    protected override void OnDataLoaded(ByteBuffer buffer)");
        builder.AppendLine("    {");
        builder.AppendLine(string.Format("        {0} config = {1}.GetRootAs{2}(buffer);", tableName, tableName, tableName));
        builder.AppendLine("        for (int i = 0; i < config.DataLength; ++i)");
        builder.AppendLine("        {");
        builder.AppendLine(string.Format("            Single{0}Data? data = config.Data(i);", tableName));
        builder.AppendLine(string.Format("            {0}Data cData = new {1}Data();", tableName, tableName));
        foreach (_NameWithType item in nameWithTypeList)
        {
            builder.AppendLine(string.Format("            cData.{0} = data.Value.{1};", item.name, item.name));
        }
        builder.AppendLine("            base.Add(cData.ID, cData);");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        File.WriteAllText(codeFilePath, usingCodeStr + configDataCodeStr + builder.ToString());
        AssetDatabase.Refresh();

    }
}
