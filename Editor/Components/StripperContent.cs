//  Copyright (c) 2025-present amlovey
//  
using System;
using System.Collections.Generic;
using System.Linq;
using OmniShader.Common.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OmniShader.Editor
{
    public class ListBoxData
    {
        public string key;
        public string name;
    }

    public class ListBox : VisualElement
    {
        private ListView list;
        private List<ListBoxData> items;

        public Action<List<ListBoxData>> OnItemsClick;

        public ListBox(string title)
        {
            this.style.maxWidth = 280;
            this.style.height = 180;
            this.SetBorderColor(ColorHelper.Parse(Theme.Current.LineColor));
            this.SetBorderWidth(1);

            var titleLabel = new Label(title);
            titleLabel.text = title;
            titleLabel.style.position = Position.Absolute;
            titleLabel.style.top = 0;
            titleLabel.style.left = 0;
            titleLabel.style.right = 0;
            titleLabel.style.height = 32;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.backgroundColor = ColorHelper.Parse(Theme.Current.ListBoxHeaderBackground);

            this.Add(titleLabel);

            items = new List<ListBoxData>();

            list = new ListView();
            list.fixedItemHeight = 24;

            list.style.position = Position.Absolute;
            list.style.top = 32;
            list.style.left = 0;
            list.style.right = 0;
            list.style.bottom = 0;
            list.style.backgroundColor = ColorHelper.Parse(Theme.Current.Background);
            list.makeItem = CreateListItem;
            list.bindItem = UpdateItem;
            list.itemsSource = items;
            list.selectionType = SelectionType.Multiple;
#if UNITY_6000_0_OR_NEWER
            list.itemsChosen += (selectedItems) =>
            {
                var dataList = selectedItems as List<ListBoxData>;
                OnItemsClick?.Invoke(dataList);
            };
#else
            list.onItemsChosen += (selectedItems) =>
            {
                var dataList = selectedItems as List<ListBoxData>;
                OnItemsClick?.Invoke(dataList);
            };
#endif
            list.Rebuild();

            this.Add(list);
        }

        public List<ListBoxData> GetSelectedItems()
        {
            return list.selectedItems.Cast<ListBoxData>().ToList();
        }

        public void SetData(List<ListBoxData> data)
        {
            this.items.Clear();
            this.items.AddRange(data.ToList().OrderBy(x => x.name));
            list.Rebuild();
        }

        public void ClearSelection()
        {
            list.SetSelectionWithoutNotify(new List<int>());
        }

        private void UpdateItem(VisualElement item, int index)
        {
            var label = item as Label;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.SetPadding(0, 8);

            label.tooltip = items[index].name;
            label.text = items[index].name;
        }

        private VisualElement CreateListItem()
        {
            var label = new Label();
            return label;
        }
    }

    public class ListBoxGroup : VisualElement
    {
        public List<ListBoxData> LeftBoxData;
        public List<ListBoxData> RightBoxData;

        public Action<ListBoxGroup> OnValueChanged;

        private ListBox leftBox;
        private ListBox rightBox;

        public ListBoxGroup(string leftGroupTitle, string rightGroupTitle)
        {
            this.style.width = 630;
            this.style.flexShrink = 0;
            this.style.flexGrow = 0;
            this.style.height = 180;

            LeftBoxData = new List<ListBoxData>();
            RightBoxData = new List<ListBoxData>();

            leftBox = new ListBox(leftGroupTitle);
            leftBox.style.position = Position.Absolute;
            leftBox.style.top = 0;
            leftBox.style.bottom = 0;
            leftBox.style.width = 280;
            leftBox.style.left = 0;
            leftBox.SetData(LeftBoxData);

            this.Add(leftBox);

            rightBox = new ListBox(rightGroupTitle);
            rightBox.style.right = 0;
            rightBox.style.position = Position.Absolute;
            rightBox.style.top = 0;
            rightBox.style.bottom = 0;
            rightBox.style.width = 280;
            this.Add(rightBox);

            var toRightButton = new IconButton(Icons.FastForward);
            toRightButton.style.position = Position.Absolute;
            toRightButton.style.left = Length.Percent(50);
            toRightButton.style.marginLeft = -24;
            toRightButton.style.top = 52;
            toRightButton.style.width = 48;
            toRightButton.OnClick = ToRightClick;

            this.Add(toRightButton);

            var toLeftButton = new IconButton(Icons.FastRewind);
            toLeftButton.style.position = Position.Absolute;
            toLeftButton.style.left = Length.Percent(50);
            toLeftButton.style.marginLeft = -24;
            toLeftButton.style.top = 92;
            toLeftButton.style.width = 48;
            toLeftButton.OnClick = ToLeftClick;
            this.Add(toLeftButton);
        }

        private void ToLeftClick()
        {
            var selectedItems = rightBox.GetSelectedItems();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            // Move to left
            LeftBoxData.AddRange(selectedItems);
            leftBox.SetData(LeftBoxData);

            // Remove from right
            RightBoxData.RemoveAll(item => selectedItems.Contains(item));
            rightBox.SetData(RightBoxData);

            ClearSelection();

            OnValueChanged?.Invoke(this);
        }

        private void ToRightClick()
        {
            var selectedItems = leftBox.GetSelectedItems();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            // Move to right
            RightBoxData.AddRange(selectedItems);
            rightBox.SetData(RightBoxData);

            // Remove from left
            LeftBoxData.RemoveAll(item => selectedItems.Contains(item));
            leftBox.SetData(LeftBoxData);

            ClearSelection();
            OnValueChanged?.Invoke(this);
        }

        public void ClearSelection()
        {
            leftBox.ClearSelection();
            rightBox.ClearSelection();
        }

        public void InitData(List<ListBoxData> leftData, List<ListBoxData> rightData)
        {
            LeftBoxData.Clear();
            LeftBoxData.AddRange(leftData);
            leftBox.SetData(LeftBoxData);

            RightBoxData.Clear();
            RightBoxData.AddRange(rightData);
            rightBox.SetData(RightBoxData);
        }
    }

    public class StripperContent : ContentControl
    {
        public Action<StripperContent, UnityEngine.Object> OnTargetChanged;
        public Action<string> OnSaveFileInputChanged;
        public Action<string> OnSaveShaderPathInputChanged;
        public Action OnGenerateClick;
        public Action OnAnalysisClick;
        public Action<ListBoxGroup> OnKeywordsChanged;
        public Action<ListBoxGroup> OnPassesChanged;

        private ObjectField objectControl;
        private ScrollView optionsContainer;
        private TextField saveToFileInput;
        private ListBoxGroup keywordsGroup;
        private ListBoxGroup passGroup;
        private TextField saveShaderPathInput;

        public StripperContent(string descrption) : base(NaviMenus.STRIPPER, descrption)
        {
            var label = new Label("Shader Stripper will strip code from shaders or shader graphs by removing keywords, \npasses and comments, following the below settings. And save the result to a .shader file.\nVery useful for reducing shader variants and improving performance from code level.");
            contentRoot.Add(label);
            RenderShaderSelectionUI();
            RenderOptions();
        }

        private void RenderOptions()
        {
            optionsContainer = new ScrollView();
            optionsContainer.style.position = Position.Absolute;
            optionsContainer.SetEdgeDistance(0, 108, 0, 0);
            optionsContainer.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            optionsContainer.style.flexDirection = FlexDirection.Column;
            optionsContainer.style.unityOverflowClipBox = OverflowClipBox.ContentBox;
            optionsContainer.style.overflow = Overflow.Visible;

            keywordsGroup = new ListBoxGroup("INCLUDED KEYWORDS", "EXCLUDED KEYWORDS");
            keywordsGroup.OnValueChanged += (group) =>
            {
                OnKeywordsChanged?.Invoke(group);
            };
            optionsContainer.Add(keywordsGroup);

            passGroup = new ListBoxGroup("INCLUDED PASSES", "EXCLUDED PASSES");
            passGroup.style.marginTop = 16;
            passGroup.OnValueChanged += (group) =>
            {
                OnPassesChanged?.Invoke(group);
            };
            optionsContainer.Add(passGroup);

            var saveShaderPathContainer = new VisualElement();
            saveShaderPathContainer.style.flexDirection = FlexDirection.Row;
            saveShaderPathContainer.style.alignItems = Align.Center;
            saveShaderPathContainer.style.marginTop = 20;
            saveShaderPathContainer.style.marginLeft = -2;

            var label1 = new Label("Saved Shader Path:");
            saveShaderPathContainer.Add(label1);

            saveShaderPathInput = new TextField();
            saveShaderPathInput.style.width = 405;
            saveShaderPathInput.style.marginLeft = 8;
            saveShaderPathInput.RegisterValueChangedCallback(evt =>
            {
                OnSaveShaderPathInputChanged?.Invoke(evt.newValue);   
            });

            saveShaderPathContainer.Add(saveShaderPathInput);

            var saveToFileContainer = new VisualElement();
            saveToFileContainer.style.flexDirection = FlexDirection.Row;
            saveToFileContainer.style.alignItems = Align.Center;
            saveToFileContainer.style.marginTop = 16;
            saveToFileContainer.style.marginLeft = -2;

            var label = new Label("Saved File Name:");
            saveToFileContainer.Add(label);

            saveToFileInput = new TextField();
            saveToFileInput.style.width = 420;
            saveToFileInput.style.marginLeft = 8;
            saveToFileInput.RegisterValueChangedCallback(evt =>
            {
                OnSaveFileInputChanged?.Invoke(evt.newValue);
            });
            saveToFileContainer.Add(saveToFileInput);

            optionsContainer.Add(saveShaderPathContainer);
            optionsContainer.Add(saveToFileContainer);

            contentRoot.Add(optionsContainer);
        }

        private void RenderShaderSelectionUI()
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.alignItems = Align.Center;
            root.style.marginTop = 16;
            root.style.marginLeft = -2;
            contentRoot.Add(root);

            var label = new Label("Target Shader:");
            root.Add(label);

            objectControl = new ObjectField();
            objectControl.objectType = typeof(Shader);
            objectControl.allowSceneObjects = false;
            objectControl.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                OnTargetChanged?.Invoke(this, evt.newValue);
            });

            objectControl.style.width = 280;
            objectControl.style.height = 21;
            objectControl.style.marginRight = 8;
            objectControl.style.marginLeft = 8;
            root.Add(objectControl);

            var analysisButton = new IconButton(Icons.Refresh);
            analysisButton.style.marginRight = 16;
            analysisButton.style.height = 18;
            analysisButton.style.width = 18;
            analysisButton.OnClick = () =>
            {
                OnAnalysisClick?.Invoke();
            };
            root.Add(analysisButton);

            var generateButton = new TextButton("Generate");
            generateButton.style.width = 120;
            generateButton.OnClick = () =>
            {
                OnGenerateClick?.Invoke();
            };
            root.Add(generateButton);
        }

        public void SetTarget(UnityEngine.Object target)
        {
            if (objectControl == null)
            {
                return;
            }

            objectControl.value = target;
        }

        public void SetKeywords(List<string> keywords, List<string> exludedKeywords)
        {
            if (keywords != null)
            {
                var data = keywords.Select(it => new ListBoxData() { key = it, name = it }).ToList();
                var excluedData = exludedKeywords.Select(it => new ListBoxData() { key = it, name = it }).ToList();
                keywordsGroup?.InitData(data, excluedData);
            }
        }

        public void SetPasses(List<string> passes, List<string> exludedPasses)
        {
            if (passes != null)
            {
                var data = passes.Select(it => new ListBoxData() { key = it, name = it }).ToList();
                var excluedData = exludedPasses.Select(it => new ListBoxData() { key = it, name = it }).ToList();
                passGroup?.InitData(data, excluedData);
            }
        }

        public void SetSaveFileFormat(string format)
        {
            if (saveToFileInput == null)
            {
                return;
            }

            saveToFileInput.SetValueWithoutNotify(format);
        }

        public void SetSaveShaderPath(string path)
        {
            if (saveShaderPathInput == null)
            {
                return;
            }

            saveShaderPathInput.SetValueWithoutNotify(path);
        }
    }
}