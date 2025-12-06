//  Copyright (c) 2025-present amlovey
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OmniShader.Common;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace OmniShader.Editor
{
    public class OmniShaderPreprocesser : IPreprocessShaders, IPreprocessComputeShaders
    {
        public int callbackOrder => 0;

        public OmniShaderPreprocesser()
        {
            
        }

        public void OnProcessComputeShader(ComputeShader shader, string kernelName, IList<ShaderCompilerData> data)
        {
            var shaderPath = shader.name;
            var assetPath = AssetDatabase.GetAssetPath(shader);
            var rules = OSBuildSetting.Load();
            if (rules.Count == 0)
            {
                return;
            }

            for(int i = data.Count - 1; i >= 0; i--)
            {
                var item = data[i];
                if (rules.Any(rule => IsMatchRule(rule, kernelName, shaderPath, assetPath, data[i])))
                {
                    data.RemoveAt(i);
                }                
            }
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            var shaderPath = shader.name;
            var assetPath = AssetDatabase.GetAssetPath(shader);
            var rules = OSBuildSetting.Load();
            if (rules.Count == 0)
            {
                return;
            }

            for(int i = data.Count - 1; i >= 0; i--)
            {
                var item = data[i];
                if (rules.Any(rule => IsMatchRule(rule, string.Empty, shaderPath, assetPath, data[i])))
                {
                    data.RemoveAt(i);
                }                
            }
        }

        private bool IsMatchRule(Rule rule, string kernalName, string shaderPath, string filePath, ShaderCompilerData data)
        {
            if (rule.Platform != "All" && rule.Platform != data.buildTarget.ToString())
            {
                return false;
            }

            switch (rule.Filter)
            {
                case Filters.KernalName:
                    if (rule.Comparison == Comparisons.Contains)
                    {
                        return !string.IsNullOrEmpty(kernalName) && kernalName.Contains(rule.Value);
                    }
                    else if (rule.Comparison == Comparisons.Equals)
                    {
                        return !string.IsNullOrEmpty(kernalName) && kernalName == rule.Value;
                    }
                    break;
                case Filters.ShaderPath:
                    if (rule.Comparison == Comparisons.Contains)
                    {
                        return !string.IsNullOrEmpty(shaderPath) && shaderPath.Contains(rule.Value);
                    }
                    else if (rule.Comparison == Comparisons.Equals)
                    {
                        return !string.IsNullOrEmpty(shaderPath) && shaderPath == rule.Value;
                    }
                    break;
                case Filters.FileName: 
                    if (rule.Comparison == Comparisons.Contains)
                    {
                        return !string.IsNullOrEmpty(filePath) && filePath.Contains(rule.Value);
                    }
                    else if (rule.Comparison == Comparisons.Equals)
                    {
                        return !string.IsNullOrEmpty(filePath) && filePath == rule.Value;
                    }
                    break;
                case Filters.Keywords:
                    var keywordsSet = data.shaderKeywordSet;
                    
                    foreach (var keyword in keywordsSet.GetShaderKeywords())
                    {
                        if (rule.Comparison == Comparisons.Contains)
                        {
                            if (keyword.name.Contains(rule.Value) && keywordsSet.IsEnabled(keyword))
                            {
                                return true;
                            }
                        }
                        else if (rule.Comparison == Comparisons.Equals)
                        {
                            if (keyword.name == rule.Value && keywordsSet.IsEnabled(keyword))
                            {
                                return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }
    }


    [Serializable]
    public class OSBuildSettingConfig
    {
        public List<Rule> Rules;
    }

    public class OSBuildSetting
    {
        private static string GetDataFolderPath()
        {
            var folder = Path.Combine(Application.dataPath, "..", "ProjectSettings");
            Directory.CreateDirectory(folder);
            return folder;
        }

        private static string GetBuildSettingFilePath()
        {
            return Path.Combine(GetDataFolderPath(), "OmniShaderBuildSetting.json");
        }

        public static void Save(List<Rule> rules)
        {
            var path = GetBuildSettingFilePath();
            var config = new OSBuildSettingConfig();
            config.Rules = rules;
            var content = JsonUtility.ToJson(config);
            File.WriteAllText(path, content);
            OSUtils.Log("Build setting saved to: {0}, content = {1}", path, content);
            AssetDatabase.Refresh();
        }

        public static List<Rule> Load()
        {
            var path = GetBuildSettingFilePath();
            if (!File.Exists(path))
            {
                return new List<Rule>();
            }

            var json = File.ReadAllText(path);
            var config = JsonUtility.FromJson<OSBuildSettingConfig>(json);
            if (config == null || config.Rules == null)
            {
                return new List<Rule>();
            }

            return config.Rules;
        }
    }
}