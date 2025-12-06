//  Copyright (c) 2025-present amlovey
//  
using System;
using System.Collections.Generic;
using System.Linq;
using OmniShader.Common.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OmniShader.Editor
{
    public class Filters
    {
        public const string Keywords = "Keywords";
        public const string ShaderPath = "Shader Path";
        public const string FileName = "File Name";
        public const string KernalName = "Kernal Name";

        public static string[] GetArray()
        {
            return new string[] { Keywords, ShaderPath, FileName, KernalName };
        }
    }

    [Serializable]
    public class Rule
    {
        public string Platform = string.Empty;
        public string Filter = string.Empty;
        public string Comparison = string.Empty;
        public string Value = string.Empty;

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", Platform, Filter, Comparison, Value);
        }

        public static Rule GetDeault()
        {
            return new Rule()
            {
                Platform = "All",
                Filter = Filters.Keywords,
                Comparison = Comparisons.Equals,
                Value = string.Empty
            };
        }
    }

    public class Comparisons
    {
        public new const string Equals = "Equals";
        public const string Contains = "Contains";

        public static string[] GetArray()
        {
            return new string[] { Equals, Contains };
        }
    }

    public class OSTableRow : VisualElement
    {
        public OSTableRow()
        {
            this.style.flexDirection = FlexDirection.Row;
        }

        public void ClearData()
        {
            this.Clear();
        }

        public void AddCell(string text, float width = 120)
        {
            var label = new Label(text);
            label.style.width = width;
            label.style.height = 32;
            label.SetPadding(0, 6);
            label.style.overflow = Overflow.Hidden;
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.tooltip = text;
            this.Add(label);
        }
    }

    public class OSTable : Container
    {
        private VisualElement headerCotainer;
        public List<Rule> Rules = new List<Rule>();

        private List<Rule> bindRules = new List<Rule>();
        private ListView listView;

        public Func<Rule, bool> Filter;
        public Action<OSTable> OnSelectedRowRemoved;

        public OSTable()
        {
            RenderHeader();
            RenderTableBody();
        }

        private void RenderTableBody()
        {
            listView = new ListView();
            listView.style.backgroundColor = ColorHelper.Parse(Theme.Current.Background);
            listView.style.position = Position.Absolute;
            listView.SetEdgeDistance(0, 32, 0, 0);
            listView.itemsSource = bindRules;
            listView.fixedItemHeight = 32;
            listView.makeItem = this.CreateRow;
            listView.bindItem = this.UpdateItem;
            listView.selectionType = SelectionType.Multiple;

            this.Add(listView);
        }

        public void InitData(List<Rule> rules)
        {
            if (rules == null)
            {
                return;
            }

            this.Rules = rules;
            UpdateBaseOnFilter();
        }

        protected virtual void UpdateItem(VisualElement item, int index)
        {
            var row = item as OSTableRow;
            row?.ClearData();

            if (index >= bindRules.Count)
            {
                return;
            }

            var rule = bindRules[index];
            row.AddCell(rule.Platform, 140);
            row.AddCell(rule.Filter, 100);
            row.AddCell(rule.Comparison, 120);
            row.AddCell(rule.Value, 640 - 140 - 100 - 120);
        }

        protected virtual VisualElement CreateRow()
        {
            return new OSTableRow();
        }

        private void RenderHeader()
        {
            headerCotainer = new Container();
            headerCotainer.style.flexDirection = FlexDirection.Row;
            headerCotainer.style.justifyContent = Justify.FlexStart;
            headerCotainer.style.height = 32;

            this.Add(headerCotainer);
        }

        public void AddRow(Rule rule)
        {
            this.Rules.Add(rule);

            if (Filter != null)
            {
                bindRules.Clear();
                bindRules.AddRange(this.Rules.Where(Filter));
            }
            else
            {
                bindRules.Add(rule);
            }

            this.listView.Rebuild();
        }

        public void UpdateBaseOnFilter()
        {
            bindRules.Clear();
            if (Filter != null)
            {
                bindRules.AddRange(this.Rules.Where(Filter));
            }
            else
            {
                bindRules.AddRange(this.Rules);
            }

            this.listView.Rebuild();
        }

        public void RemoveSelectedRows()
        {
            var selectedIndices = listView.selectedIndices;
            foreach (var index in selectedIndices)
            {
                var rule = bindRules[index];
                Rules.Remove(rule);
                bindRules.RemoveAt(index);
            }

            listView.selectedIndex = -1;
            OnSelectedRowRemoved?.Invoke(this);
            listView.Rebuild();
        }

        public void AddHeader(string Name, float width = 120, Action clickAction = null)
        {
            var label = new Label(Name);
            label.style.width = width;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.backgroundColor = ColorHelper.Parse(Theme.Current.ListBoxHeaderBackground);
            label.style.borderRightWidth = 1;
            label.style.borderRightColor = ColorHelper.Parse(Theme.Current.LineColor);
            label.SetPadding(0, 6);
            label.RegisterCallback<ClickEvent>(evt =>
            {
                clickAction?.Invoke();
            });

            headerCotainer.Add(label);
        }
    }

    public class BuildContent : ContentControl
    {
        private VisualElement addRulesContainer;
        private DropdownTextButton platformDropdown;
        private DropdownTextButton filterDropdown;
        private DropdownTextButton comparisonDropdown;
        private TextField ruleValueInput;
        private OSTable table;
        private DropdownTextButton tablePlatformFilter;

        public Action<BuildContent> OnAddRuleDataChanged;
        public Action<string> OnPlatformFliterChanged;
        public Action<BuildContent> OnSubmit;
        public Action<BuildContent> OnRemove;

        public BuildContent(string descrption) : base(NaviMenus.BUILD, descrption)
        {
            var label = new Label("Configure the rules that define what shaders or shader variants should be skipped when \nbuilding to improve the build time and performance.");
            contentRoot.Add(label);

            RenderAddRuleControl();

            var line = new VisualElement();
            line.style.marginTop = 12;
            line.style.height = 1;
            line.style.width = 640;
            line.style.backgroundColor = ColorHelper.Parse(Theme.Current.LineColor);
            contentRoot.Add(line);

            RenderRulesTable();
        }

        public void SetFilterPlatform(string platform)
        {
            tablePlatformFilter.SetText(platform);
        }

        public void SetRules(List<Rule> rules)
        {
            table?.InitData(rules);
        }

        public void SetAddRuleControlData(Rule rule)
        {
            platformDropdown.SetText(rule.Platform);
            filterDropdown.SetText(rule.Filter);
            comparisonDropdown.SetText(rule.Comparison);
            ruleValueInput.SetValueWithoutNotify(rule.Value);
        }

        public Rule GetAddRuleContolData()
        {
            var rule = new Rule();
            rule.Platform = platformDropdown.GetButtonText();
            rule.Filter = filterDropdown.GetButtonText();
            rule.Comparison = comparisonDropdown.GetButtonText();
            rule.Value = ruleValueInput.value;
            return rule;
        }

        public List<Rule> GetTableRules()
        {
            var rules = table?.Rules;
            return rules ?? new List<Rule>();
        }

        public void RenderRulesTable()
        {
            var tableContainer = new Container(0, 98, 0, 4);
            tableContainer.style.flexDirection = FlexDirection.Column;

            var tableTopFixArea = new VisualElement();
            tableTopFixArea.style.width = 640;
            tableTopFixArea.style.borderBottomColor = ColorHelper.Parse(Theme.Current.LineColor);
            tableTopFixArea.style.borderBottomWidth = 1;
            tableTopFixArea.SetPadding(8, 0);
            tableContainer.Add(tableTopFixArea);

            var label = new Label("SHADER SKIP RULES");
            label.style.fontSize = 21;
            tableTopFixArea.Add(label);

            var labelFilter = new Label("Platform: ");
            labelFilter.style.position = Position.Absolute;
            labelFilter.style.right = 250;
            labelFilter.style.bottom = 10;
            tableTopFixArea.Add(labelFilter);

            tablePlatformFilter = new DropdownTextButton("All");
            tablePlatformFilter.style.width = 170;
            tablePlatformFilter.style.position = Position.Absolute;
            tablePlatformFilter.style.bottom = 4;
            tablePlatformFilter.style.right = 70;

            tablePlatformFilter.menu.AppendAction(
                "All",
                act =>
                {
                    tablePlatformFilter.SetText("All");
                    table?.UpdateBaseOnFilter();
                    OnPlatformFliterChanged?.Invoke(this.tablePlatformFilter?.GetButtonText());
                },
                DropdownMenuAction.Status.Normal
            );
            BuildTarget[] targets = GetBuildTargets();
            foreach (var target in targets)
            {
                tablePlatformFilter.menu.AppendAction(
                    target.ToString(),
                    act =>
                    {
                        tablePlatformFilter.SetText(target.ToString());
                        table?.UpdateBaseOnFilter();
                        OnPlatformFliterChanged?.Invoke(this.tablePlatformFilter?.GetButtonText());
                    },
                    DropdownMenuAction.Status.Normal);
            }

            tableTopFixArea.Add(tablePlatformFilter);

            var removeButton = new TextButton("Remove");
            removeButton.style.width = 60;
            removeButton.style.position = Position.Absolute;
            removeButton.style.bottom = 4;
            removeButton.style.right = 0;
            removeButton.OnClick = () =>
            {
                table?.RemoveSelectedRows();
                OnRemove?.Invoke(this);
            };

            tableTopFixArea.Add(removeButton);

            // table header
            table = new OSTable();
            table.SetEdgeDistance(0, 42, 0, 0);
            table.style.width = 640;
            table.style.overflow = Overflow.Hidden;
            table.style.backgroundColor = ColorHelper.Parse(Theme.Current.LineColor);
            table.Filter = rule =>
            {
                var targetPlatform = tablePlatformFilter.GetButtonText();
                if (targetPlatform == "All")
                {
                    return true;
                }

                if (rule.Platform == "All")
                {
                    return true;
                }

                return rule.Platform == targetPlatform;
            };

            table.AddHeader("Platform", 140);
            table.AddHeader("Filter", 100);
            table.AddHeader("Comparison", 120);
            table.AddHeader("Value", 642 - 140f - 120f - 100f);

            tableContainer.Add(table);
            contentRoot.Add(tableContainer);
        }

        private void RenderFilterControl()
        {
            filterDropdown = new DropdownTextButton(Filters.Keywords);
            filterDropdown.style.width = 100;

            foreach (var filter in Filters.GetArray())
            {
                filterDropdown.menu.AppendAction(
                    filter,
                    act =>
                    {
                        filterDropdown.SetText(filter);
                        OnAddRuleDataChanged?.Invoke(this);
                    },
                    DropdownMenuAction.Status.Normal);
            }

            addRulesContainer.Add(filterDropdown);
        }

        private void RenderComparisionControl()
        {
            comparisonDropdown = new DropdownTextButton(Comparisons.Equals);
            comparisonDropdown.style.width = 80;
            foreach (var comparison in Comparisons.GetArray())
            {
                comparisonDropdown.menu.AppendAction(
                    comparison,
                    act =>
                    {
                        comparisonDropdown.SetText(comparison);
                        OnAddRuleDataChanged?.Invoke(this);
                    },
                     DropdownMenuAction.Status.Normal);
            }
            addRulesContainer.Add(comparisonDropdown);
        }

        private void RenderAddRuleControl()
        {
            addRulesContainer = new VisualElement();
            addRulesContainer.style.flexDirection = FlexDirection.Row;
            addRulesContainer.style.marginTop = 16;
            addRulesContainer.style.marginLeft = -2;

            RenderPlatformDropdown();
            RenderFilterControl();
            RenderComparisionControl();

            ruleValueInput = new TextField();
            ruleValueInput.style.height = 28;
            ruleValueInput.style.width = 200;
            ruleValueInput.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                OnAddRuleDataChanged?.Invoke(this);
            });

            addRulesContainer.Add(ruleValueInput);

            var addButton = new TextButton("Add");
            addButton.style.width = 60;
            addButton.OnClick = () =>
            {
                if (string.IsNullOrEmpty(this.ruleValueInput.value?.Trim()))
                {
                    EditorUtility.DisplayDialog("Omni Shader", "Please input a value", "Ok");
                    return;
                }

                OnSubmit?.Invoke(this);
            };
            addRulesContainer.Add(addButton);

            contentRoot.Add(addRulesContainer);
        }

        public void AddRule(Rule rule)
        {
            table?.AddRow(rule);
        }

        private void RenderPlatformDropdown()
        {
            platformDropdown = new DropdownTextButton("All");
            platformDropdown.style.width = 170;

            platformDropdown.menu.AppendAction(
                "All",
                act =>
                {
                    platformDropdown.SetText("All");
                    OnAddRuleDataChanged?.Invoke(this);
                },
                DropdownMenuAction.Status.Normal);
            BuildTarget[] targets = GetBuildTargets();
            foreach (var target in targets)
            {
                platformDropdown.menu.AppendAction(
                    target.ToString(),
                    act =>
                    {
                        platformDropdown.SetText(target.ToString());
                        OnAddRuleDataChanged?.Invoke(this);
                    },
                    DropdownMenuAction.Status.Normal);
            }

            addRulesContainer.Add(platformDropdown);
        }

        private BuildTarget[] GetBuildTargets()
        {
            BuildTarget[] targets = (BuildTarget[])Enum.GetValues(typeof(BuildTarget));

            string[] ingoreGroups = new string[]
            {
                "NoTarget",
                "ReservedCFE",
            };
            targets = targets.Where(target => !ingoreGroups.Contains(target.ToString())).ToArray();

            var obsoleteAttribute = typeof(BuildTarget).GetField("ObsoleteAttribute", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return targets.Where(target =>
            {
                var type = typeof(BuildTarget);
                var field = type.GetField(target.ToString());
                var attribute = field.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                return attribute.Length == 0;
            }).ToArray();
        }
    }
}