using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Reflection;
public class MoeGameConfigTables
{
    public static async Task InitTables()
    {
        Type configType = typeof(IMoeConfigTableBase);
        Assembly assem = Assembly.GetAssembly(configType);
        foreach (Type child in assem.GetTypes())
        {
            if (child.BaseType != null && child.BaseType.BaseType != null && child.BaseType.BaseType == configType)
            {
                IMoeConfigTableBase haha = System.Activator.CreateInstance(child) as IMoeConfigTableBase;
                if (haha != null)
                {
                    await haha.LoadConfigTable();
                }
                else
                {
                    Debug.LogErrorFormat("请确保类型 {0} 继承 MoeConfigTable", child.Name);
                }
            }
        }

        // await LanguageConfigTable.Inst.LoadConfigTable();
        // await MusicConfigTable.Inst.LoadConfigTable();
    }
}