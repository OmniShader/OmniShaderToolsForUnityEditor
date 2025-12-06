using System;
using System.Collections.Generic;
using OmniShader.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace OmniShader.Editor
{
    public class NaviMenu
    {
        public string id;
        public string displayName;
        public Icon icon;
        public Action OnClick;
        public bool IsSelected;
    }

    public class NaviMenuControl : VisualElement
    {
        public NaviMenu menu;

        public NaviMenuControl(NaviMenu menu)
        {
            this.menu = menu;

            this.RegisterCallback<PointerOverEvent>(evt =>
            {
                evt.StopPropagation();

                if (!menu.IsSelected)
                {
                    this.style.backgroundColor = ColorHelper.Parse(Theme.Current.HoverBackground);
                }
            });

            this.RegisterCallback<PointerOutEvent>(evt =>
            {
                evt.StopPropagation();

                if (!menu.IsSelected)
                {
                    this.style.backgroundColor = ColorHelper.Parse(Theme.Current.MenuBackground);
                }
            });

            this.style.flexDirection = FlexDirection.Row;
            this.style.alignItems = Align.Center;
            this.style.justifyContent = Justify.FlexStart;
            this.style.flexShrink = 0;
            this.style.flexGrow = 0;
            this.style.width = 180;
            this.style.height = 36;
            this.style.fontSize = 14;
            this.SetPadding(12, 6, 10, 6);
            this.style.marginBottom = 2;
            this.RegisterCallback<ClickEvent>(OnClick);
            this.SetSelected(menu.IsSelected);

            var iconElement = new TextElement();
            iconElement.AddToClassList("icon");
            iconElement.SetMaterialIconFont();
            iconElement.text = menu.icon.value;

            this.Add(iconElement);
            var textElement = new TextElement();
            textElement.text = menu.displayName;
            textElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            textElement.style.marginLeft = 12;

            this.Add(textElement);
        }

        private void OnClick(ClickEvent evt)
        {
            evt.StopPropagation();

            menu.IsSelected = true;
            this.menu.OnClick?.Invoke();
        }

        public void SetSelected(bool isSelected)
        {
            if (isSelected)
            {
                this.style.backgroundColor = ColorHelper.Parse(Theme.Current.MenuSelectedBackgroud);
            }
            else
            {
                this.style.backgroundColor = ColorHelper.Parse(Theme.Current.MenuBackground);
            }
        }
    }

    internal class NaviMenuPanel : Container
    {
        private List<NaviMenuControl> menus;

        public Action OnPanelExpandClick;
        private IconButton iconElement;

        public NaviMenuPanel()
        {
            this.style.flexDirection = FlexDirection.Column;
            this.style.flexShrink = 0;
            this.style.flexGrow = 0;
            this.style.backgroundColor = ColorHelper.Parse(Theme.Current.MenuPanelBackground);
            this.style.width = 180;
            this.style.overflow = Overflow.Hidden;

            RenderBanner();

            menus = new List<NaviMenuControl>();
        }

        private void RenderBanner()
        {
            var banner = new VisualElement();
            banner.style.height = 32;
            banner.style.position = Position.Absolute;
            banner.style.left = 0;
            banner.style.right = 0;
            banner.style.bottom = 24;
            banner.style.width = 180;

            var textElement = new TextElement();
            textElement.text = "Omni Shader v0.0.1b";
            textElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            textElement.style.fontSize = 12;
            textElement.style.height = 32;
            textElement.style.marginLeft = 28;
            textElement.style.marginTop = 2;

            iconElement = new IconButton(Icons.ArrowBack);
            iconElement.style.position = Position.Absolute;
            iconElement.style.left = 8;
            iconElement.style.top = 4;
            iconElement.style.unityTextAlign = TextAnchor.MiddleRight;
            iconElement.OnClick = () =>
            {
                OnPanelExpandClick?.Invoke();
            };
            
            banner.Add(iconElement);
            banner.Add(textElement);
            banner.SetPadding(12, 0, 0, 0);

            this.Add(banner);
        }

        public void AddMenu(NaviMenu menu)
        {
            var menuControl = new NaviMenuControl(menu);
            this.Add(menuControl);
            menus.Add(menuControl);
        }

        public void UpdateExpandState(bool isExpand)
        {
            this.style.width = isExpand ? 180 : 40;
            iconElement.text = isExpand ? Icons.ArrowBack.value : Icons.ArrowForward.value;
        }

        public void UpdateUI(string selectedMenuId)
        {
            foreach (var item in menus)
            {
                item.menu.IsSelected = item.menu.id == selectedMenuId;
                item.SetSelected(item.menu.IsSelected);
            }
        }
    }
}