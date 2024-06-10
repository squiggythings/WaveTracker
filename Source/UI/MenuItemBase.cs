using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    /// <summary>
    /// Base class for menu items
    /// </summary>
    public abstract class MenuItemBase : Clickable {
        public Menu parentMenu;

        public const int MAX_WIDTH = 400;
        public const int PADDING_LEFT = 2;
        public const int MARGIN_LEFT = 12;

        public const int PADDING_RIGHT = 6;
        public const int MARGIN_RIGHT = 0;
        public const int OPTION_HEIGHT = 10;
        public bool IsHoveredExclusive => IsHovered && !parentMenu.IsChildMenuHovered();
        public new bool Clicked => base.Clicked && !parentMenu.IsChildMenuHovered();


        public abstract void Update();
        public abstract void Draw();

        public abstract void SetWidth(int width);

        /// <summary>
        /// Gets the child menu item at <c>index</c>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public MenuItemBase this[int index] {
            get {
                if (this is SubMenu sub) {
                    return sub.menu[index];
                }
                else {
                    throw new IndexOutOfRangeException("This menu item does not have any children!");
                }
            }
            set {
                if (this is SubMenu sub) {
                    sub[index] = value;
                }
                else {
                    throw new IndexOutOfRangeException("This menu item does not have any children!");
                }
            }
        }
    }
}
