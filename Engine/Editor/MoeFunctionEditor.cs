using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MoeFunctionEditor
{
    [MenuItem("MoeTools/GenExcel")]
    static void Excel2Flatbuffers() {
        string script = System.IO.Path.Combine(Application.dataPath, "../_OutAssets/ExcelConfigTables/_gen.sh");
        System.Diagnostics.Process.Start("/bin/bash", script);
    }
}
