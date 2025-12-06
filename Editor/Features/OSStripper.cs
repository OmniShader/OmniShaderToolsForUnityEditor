//  Copyright (c) 2025-present amlovey
//
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OmniShader.Common;
using UnityEditor;
using UnityEngine;

namespace OmniShader.Editor
{
    [Serializable]
    public class AnalysisResult
    {
        public string[] keywords;
        public string[] passes;
        public string shader_path;
    }

    public class OmniShaderStripper
    {
        public static AnalysisResult Analysis(string targetShaderPath)
        {
            if (string.IsNullOrEmpty(targetShaderPath))
            {
                return null;
            }

            var checkPah = targetShaderPath;

            if (targetShaderPath.EndsWith(".shadergraph"))
            {
                var genCode = GetShaderGraphGeneratedCode(targetShaderPath);
                checkPah = GetTempShaderPath(targetShaderPath);
                File.WriteAllText(checkPah, genCode, Encoding.UTF8);
            }

            var result_json = Communicator.Execute("analysis", "\"" + Path.GetFullPath(checkPah) + "\"");
            OSUtils.Log("Commnicator Results: {0}", result_json);

            if (string.IsNullOrEmpty(result_json))
            {
                return null;
            }

            try
            {
                var result = JsonUtility.FromJson<AnalysisResult>(result_json);
                if (result.keywords != null)
                {
                    result.keywords = result.keywords.ToList().OrderBy(x => x).ToArray();
                }

                return result;
            }
            catch (Exception e)
            {
                OSUtils.LogError(e);
                return null;
            }
        }

        public static void Strip(string targetShaderPath, string[] excludeKeywords, string[] includedPass, string saveToFileNameFormat, string shader_path_name)
        {
            if (string.IsNullOrEmpty(targetShaderPath) || string.IsNullOrEmpty(saveToFileNameFormat))
            {
                return;
            }

            string src = targetShaderPath;
            if (targetShaderPath.EndsWith(".shadergraph"))
            {
                var code = GetShaderGraphGeneratedCode(targetShaderPath);
                src = GetTempShaderPath(targetShaderPath);
                File.WriteAllText(src, code, Encoding.UTF8);
            }

            EditorUtility.DisplayProgressBar("Shader Stripper", "Stripping...", 0.5f);

            var saveFileName = FormatStringWithPlaceholder(saveToFileNameFormat, targetShaderPath);
            var saveFolder = Path.GetDirectoryName(targetShaderPath);
            var saveFile = Path.Combine(saveFolder, saveFileName);

            var generatedCode = Communicator.Execute(
                "strip",
                WrapString(Path.GetFullPath(src)),
                WrapString(shader_path_name),
                WrapString(string.Join(",", includedPass)),
                WrapString(string.Join(",", excludeKeywords))
            );

            EditorUtility.DisplayCancelableProgressBar("Shader Stripper", "Stripping", 0.8f);

            File.WriteAllText(saveFile, generatedCode, Encoding.UTF8);
            EditorUtility.DisplayCancelableProgressBar("Shader Stripper", "Stripping", 1f);
        }

        private static string GetTempShaderPath(string targetShaderPath)
        {
            var tempFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp"));
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            var fileName = Path.GetFileNameWithoutExtension(targetShaderPath) + "_os_gen.shader";
            var tempFile = Path.Combine(tempFolder, fileName);
            return tempFile;
        }

        private static string WrapString(string s)
        {
            return "\"" + s + "\"";
        }

        private static string FormatStringWithPlaceholder(string format, string srcFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(srcFilePath);
            var s = format.Trim().Replace("[FileName]", fileName);
            s = s.Replace("[%d]", DateTime.Now.ToString("yyyyMMddHHmmss"));
            return s;
        }

        public static string GetShaderGraphGeneratedCode(string graphPath)
        {
            if (!graphPath.ToLower().EndsWith(".shadergraph"))
            {
                return null;
            }

            var graphyAssembly = GetGraphEditorAssembly();
            if (graphyAssembly == null)
            {
                return null;
            }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            var types = graphyAssembly.GetTypes();
            Type importEditorType = null;
            Type generatorType = null;
            foreach (var type in types)
            {
                if (type.Name == "ShaderGraphImporterEditor")
                {
                    importEditorType = type;
                }

                if (type.Name.Equals("Generator"))
                {
                    generatorType = type;
                }
            }

            if (importEditorType != null && generatorType != null)
            {
                MethodInfo getDataMethod = null;
                var methods = importEditorType.GetMethods(flags);
                foreach (var method in methods)
                {
                    if (method.Name.Contains("GetGraphData"))
                    {
                        getDataMethod = method;
                    }
                }

                if (getDataMethod != null)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(graphPath);
                    var data = getDataMethod.Invoke(null, new object[] { importer });

                    var constructors = generatorType.GetConstructors();
                    if (constructors.Length == 0)
                    {
                        return string.Empty;
                    }

                    var assetName = Path.GetFileNameWithoutExtension(graphPath);
                    var generator = constructors[0]?.Invoke(new object[] { data, null, 1, assetName, null, null, true });

                    var properties = generatorType.GetProperties(flags);
                    PropertyInfo generateShaderProperty = null;
                    foreach (var item in properties)
                    {
                        if (item.Name.Equals("generatedShader"))
                        {
                            generateShaderProperty = item;
                        }
                    }

                    return generateShaderProperty?.GetValue(generator)?.ToString();
                }
            }

            return string.Empty;
        }

        private static Assembly GetGraphEditorAssembly()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var name = assembly.GetName().Name;
                if (name.Equals("Unity.ShaderGraph.Editor"))
                {
                    return assembly;
                }
            }

            return null;
        }
    }
}