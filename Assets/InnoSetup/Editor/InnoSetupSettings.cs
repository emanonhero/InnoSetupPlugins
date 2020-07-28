using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Eamon.InnoSetup
{
    [CreateAssetMenu(menuName = "Eamon/InnoSetupSettings")]
    public class InnoSetupSettings : ScriptableObject
    {
        private const string packageGuid = "889d80cb91521644cb7c24a13c7ad4ae";
        private string settingsDir
        {
            get
            {
                if (string.IsNullOrEmpty(_setDir))
                {
                    _setDir = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(packageGuid));
                }
                return _setDir;
            }
        }
        //本包相对路径 Assets/Tools/InnoSetup
        private string _setDir;
        [Tooltip("AppId值为Empty时不做任何设置")]
        public Guid AppId;
        [Tooltip("值为空时使用ProductName")]
        public string ChineseAppName;
        [Tooltip("值为空时使用ChineseAppName")]
        [HideInInspector]
        public string OutputBaseFilename;

        [Tooltip("程序运行包相对Iss脚本的路径")]
        private string BuildDir = @"..\Build";

        internal void GenerateNewAppId()
        {
            AppId = Guid.NewGuid();
        }

        //public Guid AppId = Guid.Empty;
        [Header("配置路径")]
        [Tooltip("脚本模板")]
        public string IssTemplate = "template.txt";
        [Tooltip("许可文件")]
        public string LicenseFile = "License.txt";
        [Tooltip("安装指南文件")]
        public string InfoBeforeFile = "Instruction.txt";
        [Tooltip("说明文件")]
        public string InfoAfterFile = "Readme.txt";
        [Tooltip("InnoSetup编译文件")]
        public string InnoSetupFile = @"‪C:\Program Files (x86)\Inno Setup 6\Compil32.exe";
        [Tooltip("安装包输出文件夹")]
        public string OutputDir = @"InstallerOutput";

        public bool ExportIssScript()
        {
            var abt = EditorUserBuildSettings.activeBuildTarget;
            if (abt != BuildTarget.StandaloneWindows && abt != BuildTarget.StandaloneWindows64)
            {
                EditorUtility.DisplayDialog("不支持的发布平台", "目前仅支持StandaloneWindows||StandaloneWindows64。", "确定");
                return false;
            }
            BuildDir = Path.GetDirectoryName(EditorUserBuildSettings.GetBuildLocation(abt));
            if (string.IsNullOrEmpty(BuildDir)||!Directory.Exists(BuildDir))
            {
                EditorUtility.DisplayDialog("未知打包路径", BuildDir+Environment.NewLine+"请打包成功后再导出脚本。", "确定");
                return false;
            }
#if !UNITY_STANDALONE_WIN
            Debug.LogError("只支持Win平台编辑器");
#endif
            var IssFile = Path.Combine(OutputDir, PlayerSettings.productName + ".iss");
            Directory.CreateDirectory(Path.GetDirectoryName(IssFile));
            Dictionary<string, string> defineVariables = new Dictionary<string, string>();
            defineVariables.Add("MyAppName", PlayerSettings.productName);
            defineVariables.Add("MyAppVersion", PlayerSettings.bundleVersion);
            defineVariables.Add("MyAppPublisher", "云南视广科技有限公司");
            defineVariables.Add("MyAppURL", "http://www.sg-bridge.com/");
            defineVariables.Add("MyAppExeName", PlayerSettings.productName + ".exe");

            if (string.IsNullOrEmpty(ChineseAppName))
            {
                ChineseAppName = PlayerSettings.productName;
            }
            if (string.IsNullOrEmpty(OutputBaseFilename))
            {
                OutputBaseFilename = ChineseAppName;
            }
            defineVariables.Add("ChineseAppName", ChineseAppName);
            defineVariables.Add("OutputBaseFilename", OutputBaseFilename);
            defineVariables.Add("BuildFile", BuildDir + @"\*");
            if (!string.IsNullOrEmpty(LicenseFile) && File.Exists(Path.Combine(settingsDir, LicenseFile)))
            {
                defineVariables.Add("LicenseFilePlaceholder", "LicenseFile = " + LicenseFile);
                File.Copy(Path.Combine(settingsDir, LicenseFile), Path.Combine(OutputDir, LicenseFile), true);//三个参数分别是源文件路径，存储路径，若存储路径有相同文件是否替换
            }
            else
            {
                defineVariables.Add("LicenseFilePlaceholder", "");
            }
            if (!string.IsNullOrEmpty(InfoBeforeFile) && File.Exists(Path.Combine(settingsDir, InfoBeforeFile)))
            {
                defineVariables.Add("InfoBeforeFilePlaceholder", "InfoBeforeFile = " + InfoBeforeFile);
                File.Copy(Path.Combine(settingsDir, InfoBeforeFile), Path.Combine(OutputDir, InfoBeforeFile), true);
            }
            else
            {
                defineVariables.Add("InfoBeforeFilePlaceholder", "");
            }
            if (!string.IsNullOrEmpty(InfoAfterFile) && File.Exists(Path.Combine(settingsDir, InfoAfterFile)))
            {
                defineVariables.Add("InfoAfterFilePlaceholder", "InfoAfterFile = " + InfoAfterFile);
                File.Copy(Path.Combine(settingsDir, InfoAfterFile), Path.Combine(OutputDir, InfoAfterFile), true);
            }
            else
            {
                defineVariables.Add("InfoAfterFilePlaceholder", "");
            }
            if (AppId != Guid.Empty)
            {
                defineVariables.Add("AppIdPlaceholder", "AppId={{" + AppId.ToString().ToUpper() + "}");
            }
            else
            {
                defineVariables.Add("AppIdPlaceholder", "");
            }

            string defines = string.Empty;
            foreach (var item in defineVariables)
            {
                defines += newDefineLine(item.Key, item.Value);
            }

            try
            {
                defines += File.ReadAllText(Path.Combine(settingsDir, IssTemplate));
                File.WriteAllText(IssFile, defines, System.Text.Encoding.UTF8);
                Process.Start("explorer.exe", Path.GetDirectoryName(IssFile));
            }
            catch (Exception e)
            {

                Debug.LogException(e);
                return false;
            }
            return true;
        }
        private string newDefineLine(string key, string value)
        {
            return string.Format("{0}#define {1} \"{2}\"", Environment.NewLine, key, value);
        }
        public void CompileIssScript()
        {
            var IssFile = Path.Combine(OutputDir, PlayerSettings.productName + ".iss");
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = InnoSetupFile;
                psi.UseShellExecute = false;
                psi.WorkingDirectory = Path.GetDirectoryName(InnoSetupFile);
                psi.Arguments = "/cc \"" + Application.dataPath.Replace("Assets", "") + IssFile + "\"";
                psi.CreateNoWindow = true;
                Process.Start(psi);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [ContextMenu("ExportAndCompile")]
        public void ExportAndCompile()
        {
            //todo:EditorBuildSettings.scenes可以自动添加激活加密场景，参考：https://github.com/hananoki/BuildAssist
            if (ExportIssScript()) CompileIssScript();
        }
    }
}