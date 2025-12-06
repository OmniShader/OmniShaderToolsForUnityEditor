//  Copyright (c) 2025-present amlovey
//  

using UnityEngine.UIElements;

namespace OmniShader.Editor
{
    internal class HomeConent : ContentControl
    {
        public HomeConent(string descrption) : base(NaviMenus.HOME, descrption)
        {
            var label = new Label("Welcome to Omni Shader!\n\nOmni Shader Tools For Unity is a powerful toolset for Unity developers, designed to \nstreamline shader development and enhance your workflow. Explore the features \nthrough the navigation menu on the left and take your Unity projects to the next level!");
            contentRoot.Add(label);
        }
    }
}