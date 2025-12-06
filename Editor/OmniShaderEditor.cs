//  Copyright (c) 2025-present amlovey
//  
using System.Collections.Generic;
using System.Linq;
using OmniShader.Common;
using OmniShader.Common.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OmniShader.Editor
{
    public class OmniShaderEditor : Container
    {
        public OmniShaderEditorState State;

        private NaviMenuPanel menuPanel;
        private ContentPanel contentPanel;
        private StripperContent stripper;

        public OmniShaderEditor(OmniShaderEditorState state) : base(0, 2, 0, 0)
        {
            this.State = state;
            RenderUI();
        }

        private void RenderUI()
        {
            RenderMenuPanel();
            RenderContents();
            RenderStatusBar();
        }

        private void OnMenuClick(string menuId)
        {
            State.selectedMenuId = menuId;
            menuPanel?.UpdateUI(menuId);
            contentPanel?.UpdateUI();
        }

        private void RenderContents()
        {
            contentPanel = new ContentPanel(State);
            contentPanel.AddContent(new HomeConent("Omni Shader Tools For Unity"));
            CreateBuildContent();
            CreateStripperContent();
            contentPanel.AddContent(new ProgrammingContent("Shader Programming"));
            contentPanel.UpdateUI();

            this.Add(contentPanel);
        }

        private void CreateBuildContent()
        {
            var buildContent = new BuildContent("Shader Build Settings");
            buildContent.OnSubmit = (ele) =>
            {
                var rule = ele.GetAddRuleContolData();
                ele.AddRule(rule);
                OSUtils.Log("Added Rule: {0}", rule);
                OSBuildSetting.Save(State.rulesStoreInPorject);

                GameObject go = new GameObject();
                EditorUtility.SetDirty(go);
                GameObject.DestroyImmediate(go);
                AssetDatabase.SaveAssets();
            };
            buildContent.SetRules(State.rulesStoreInPorject);
            buildContent.OnRemove = ele =>
            {
                State.rulesStoreInPorject = ele.GetTableRules();
                OSBuildSetting.Save(State.rulesStoreInPorject);
            };

            buildContent.SetAddRuleControlData(State.addRuleControlData);
            buildContent.OnAddRuleDataChanged = ele =>
            {
                State.addRuleControlData = ele.GetAddRuleContolData();  
            };
            buildContent.SetFilterPlatform(State.tablePlatformFilter);
            buildContent.OnPlatformFliterChanged = platform =>
            {
               State.tablePlatformFilter = platform;  
            };

            contentPanel.AddContent(buildContent);
        }

        private void CreateStripperContent()
        {
            stripper = new StripperContent("Shader Stripper");
            stripper.OnTargetChanged = (ele, target) =>
            {
                if (target == null)
                {
                    State.targetShaderPath = string.Empty;
                }
                else
                {
                    State.targetShaderPath = AssetDatabase.GetAssetPath(target);
                }

                AnalysisShaderInStripper();               
            };

            stripper.SetSaveFileFormat(State.saveToFileNameFormat);
            stripper.OnSaveFileInputChanged = (format) =>
            {
                State.saveToFileNameFormat = format;
            };

            stripper.SetSaveShaderPath(State.saveShaderPath);
            stripper.OnSaveShaderPathInputChanged = (path) =>
            {
                State.saveShaderPath = path;
            };

            if (OSUtils.IsSupportedShader(State.targetShaderPath))
            {
                var shader = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(State.targetShaderPath);
                stripper.SetTarget(shader);
            }

            stripper.OnGenerateClick = () =>
            {
                OSUtils.Log($"Generate {State.targetShaderPath}");

                EditorUtility.DisplayProgressBar("Shader Stripper", "Stripping...", 0);
                try
                {
                    OmniShaderStripper.Strip(
                        State.targetShaderPath, 
                        State.exludedKeywordsInStripper,
                        State.includedPassesInStripper,
                        State.saveToFileNameFormat,
                        State.saveShaderPath
                    );

                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    OSUtils.LogError(e);
                }

                EditorUtility.ClearProgressBar();
            };

            stripper.OnAnalysisClick = AnalysisShaderInStripper;
            stripper.SetKeywords(State.includedKeywordsInStripper.ToList(), State.exludedKeywordsInStripper.ToList());
            stripper.SetPasses(State.includedPassesInStripper.ToList(), State.exludedPassesInStripper.ToList());
            stripper.OnKeywordsChanged = control =>
            {
                State.exludedKeywordsInStripper = control.RightBoxData.Select(x => x.name).ToArray();
                State.includedKeywordsInStripper = control.LeftBoxData.Select(x => x.name).ToArray();
            };
            stripper.OnPassesChanged = control =>
            {
                State.exludedPassesInStripper = control.RightBoxData.Select(x => x.name).ToArray();
                State.includedPassesInStripper = control.LeftBoxData.Select(x => x.name).ToArray();
            };

            contentPanel.AddContent(stripper);
        }

        private void AnalysisShaderInStripper()
        {
            var result = OmniShaderStripper.Analysis(State.targetShaderPath);
            if (result == null)
            {
                OSUtils.Log("Found No Results");
                stripper.SetKeywords(new List<string>(), new List<string>());
                stripper.SetPasses(new List<string>(), new List<string>());
                State.includedKeywordsInStripper = new string[0];
                State.includedPassesInStripper = new string[0];
                State.saveShaderPath = "OmniShader";
                stripper.SetSaveShaderPath(State.saveShaderPath);
                return;
            }

            OSUtils.Log("Found Keyword = {0}, Passes = {1}", result.keywords.Length, result.passes.Length);
            stripper.SetKeywords(result.keywords?.ToList(), new List<string>());
            stripper.SetPasses(result.passes?.ToList(), new List<string>());
            State.includedKeywordsInStripper = result.keywords;
            State.includedPassesInStripper = result.passes;
            State.exludedKeywordsInStripper = new string[0];
            State.exludedPassesInStripper = new string[0];
            State.saveShaderPath = string.Format("OmniShader/{0}", result.shader_path);
            stripper.SetSaveShaderPath(State.saveShaderPath);
        }

        private void RenderMenuPanel()
        {
            menuPanel = new NaviMenuPanel();
            menuPanel.UpdateExpandState(State.isMenuExpanded);
            menuPanel.OnPanelExpandClick = () =>
            {
                State.isMenuExpanded = !State.isMenuExpanded;
                menuPanel?.UpdateExpandState(State.isMenuExpanded);
                contentPanel?.UpdateUI();
            };
            this.Add(menuPanel);

            menuPanel.AddMenu(CreateNaviMenu(NaviMenus.HOME, Icons.Home));
            menuPanel.AddMenu(CreateNaviMenu(NaviMenus.BUILD, Icons.Build));
            menuPanel.AddMenu(CreateNaviMenu(NaviMenus.STRIPPER, Icons.ContentCut));
            menuPanel.AddMenu(CreateNaviMenu(NaviMenus.PROGRAMMING, Icons.Code));
        }

        private NaviMenu CreateNaviMenu(string displayName, Icon icon)
        {
            return new NaviMenu
            {
                id = displayName,
                displayName = displayName,
                icon = icon,
                OnClick = () => { OnMenuClick(displayName); },
                IsSelected = displayName == State.selectedMenuId,
            };
        }

        private void RenderStatusBar()
        {
            var statusBar = new Container();
            statusBar.style.backgroundColor = ColorHelper.Parse(Theme.Current.StatusBarBackround);
            statusBar.SetEdgeDistance(0, float.NaN, 0, 0);
            statusBar.style.height = 24;

            this.Add(statusBar);
        }
    }
}
