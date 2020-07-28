using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Eamon.InnoSetup
{
    [CustomEditor(typeof(InnoSetupSettings), true)]
    public class InnoSetupSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {            
            InnoSetupSettings _target = (InnoSetupSettings)target;
            if (_target.AppId != System.Guid.Empty)
            {
                GUILayout.Label("AppId是安装包在操作系统中注册的唯一标识符");
                var tmpGuid = System.Guid.Empty;
                if (System.Guid.TryParse(EditorGUILayout.TextField("AppId", _target.AppId.ToString()), out tmpGuid))
                {
                    _target.AppId = tmpGuid;
                }
            }
            if (GUILayout.Button("Generate New AppId"))
            {
                _target.GenerateNewAppId();
            }
            DrawDefaultInspector();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("导出脚本"))
            {
                _target.ExportIssScript();
            }
            if (GUILayout.Button("导出并编译"))
            {
                _target.ExportAndCompile();
            }
            GUILayout.EndHorizontal();
        }
    }
}
