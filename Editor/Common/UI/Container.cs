//  Copyright (c) 2025-present amlovey
//  
using UnityEngine.UIElements;

namespace OmniShader.Common.UI
{
    public class Container : VisualElement
    {
        public Container()
        {
            this.style.position = Position.Absolute;
            this.SetEdgeDistance(0, 0, 0, 0);
        }

        public Container(float left, float top, float right, float bottom)
        {
            this.style.position = Position.Absolute;
            this.SetEdgeDistance(left, top, right, bottom);
        }
    }

    public class Hoverable : VisualElement
    {
        public Hoverable(string normalColor, string hoverColor)
        {
            this.style.backgroundColor = ColorHelper.Parse(normalColor);

            this.RegisterCallback<PointerOverEvent>(evt =>
            {
                this.style.backgroundColor = ColorHelper.Parse(hoverColor);
            });

            this.RegisterCallback<PointerOutEvent>(evt =>
            {
                this.style.backgroundColor = ColorHelper.Parse(normalColor);
            });
        }
    }
}
