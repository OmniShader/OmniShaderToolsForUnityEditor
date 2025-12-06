//  Copyright (c) 2025-present amlovey
//  
using System.Collections.Generic;
using OmniShader.Common.UI;
using UnityEngine.UIElements;

namespace OmniShader.Editor
{
    internal class ContentPanel : Container
    {
        private OmniShaderEditorState state;
        private List<ContentControl> contents = new List<ContentControl>();

        public ContentPanel(OmniShaderEditorState state)
        : base(state.isMenuExpanded ? 180 : 40, 0, 0, 24)
        {
            this.state = state;
        }

        public void UpdateUI()
        {
            if (state != null)
            {
                this.style.left = state.isMenuExpanded ? 180 : 40;

                foreach (var content in contents)
                {
                    content.style.visibility = content.key == state.selectedMenuId ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        public void AddContent(ContentControl content)
        {
            contents.Add(content);
            this.Add(content);
        }
    }
}