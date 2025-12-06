//  Copyright (c) 2025-present amlovey
//  
using OmniShader.Common.UI;
using UnityEngine.UIElements;

namespace OmniShader.Editor
{
    public class ContentControl : Container
    {
        public string title;
        public string key;

        protected Container contentRoot;

        public ContentControl(string key, string title): base(16, 8, 16, 8)
        {
            this.title = title;
            this.key = key;
            RenderTitle(title);
            RenderTitleLine();
            RenderContentRoot();
        }

        private void RenderContentRoot()
        {
            contentRoot = new Container(0, 6, 0, 0);
            contentRoot.style.marginTop = 56;

            this.Add(contentRoot);
        }

        private void RenderTitleLine()
        {
            var line = new VisualElement();
            line.style.height = 1;
            line.style.backgroundColor = ColorHelper.Parse(Theme.Current.LineColor);
            line.style.position = Position.Absolute;
            line.style.left = 0;
            line.style.right = 0;
            line.style.top = 48;

            this.Add(line);
        }

        protected virtual void RenderTitle(string title)
        {
            var ele = new TextElement();
            ele.text = title;
            ele.style.position = Position.Absolute;
            ele.style.top = 6;
            ele.style.fontSize = 28;

            this.Add(ele);
        }
    }
}