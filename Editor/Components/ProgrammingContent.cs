//  Copyright (c) 2025-present amlovey
//  
using System;
using OmniShader.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace OmniShader.Editor
{
    internal class ProgrammingContent : ContentControl
    {
        public ProgrammingContent(string descrption) : base(NaviMenus.PROGRAMMING, descrption)
        {
            this.style.flexDirection = FlexDirection.Column;

            var label = new Label("Omni Shader Tools For Unity has extensions of popular IDEs that support shaderlab \nprogramming. It provides syntax highlighting, code completion, go to definition, find \nsymbol references and other features to make shader development easier.");
            label.style.width = Length.Percent(100);
            label.style.marginBottom = 8;
            label.style.flexWrap = Wrap.Wrap;
            contentRoot.Add(label);     

            AddButton("Downlaod Visual Studio Extension", () =>
            {
                Application.OpenURL(Constants.MARKETPLACE_VS);
            });

            AddButton("Downlaod Visual Studio Code Extension", () =>
            {
                Application.OpenURL(Constants.MARKETPLACE_VSCODE);
            });

            AddButton("Preview Core Language Features Online", () =>
            {
                Application.OpenURL(Constants.CORE_FEATURES);
            });
        }

        private void AddButton(string text, Action onClick)
        {
            var button = new TextButton(text);
            button.style.width = 280;
            button.style.marginTop = 12;
            button.OnClick = onClick;
            contentRoot.Add(button);
        }
    }
}