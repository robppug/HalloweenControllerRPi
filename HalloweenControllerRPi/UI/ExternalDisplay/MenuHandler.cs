using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi.UI.ExternalDisplay
{
   public class MenuNode<T> where T : UIElement
   {
      public T Control { get; set; }

      public MenuNode<T> Parent;
      public List<MenuNode<T>> Children;

      public MenuNode(MenuNode<T> parent, T ctrl)
      {
         Parent = parent;
         Control = ctrl;
         Children = new List<MenuNode<T>>();
      }

      public void AddNode(MenuNode<T> node)
      {
         Children.Add(node);
      }
   }

   public class MenuHandler
   {
      private GraphicsProvider _displayDevice;
      private MenuNode<UserControl> _previousMenuNode;
      private MenuNode<UserControl> _popUpMenuNode;
      private List<MenuNode<UserControl>> _menuTree;

      public MenuNode<UserControl> CurrentMenuNode;
      public event EventHandler MenuChanged;

      public enum MenuButtons
      {
         Left,
         Right,
         Up,
         Down,
         Enter,
         Menu_1,
         Menu_2
      }

      public MenuHandler(GraphicsProvider g, MenuNode<UserControl> rootNode)
      {
         _displayDevice = g;
         _previousMenuNode = null;

         _menuTree = new List<MenuNode<UserControl>>();

         AddNode(rootNode);

         ChangeCurrentMenu(rootNode);
      }

      private void ChangeCurrentMenu(MenuNode<UserControl> newNode)
      {
         CurrentMenuNode = newNode;

         _displayDevice.ActiveCanvas.Children.Add(newNode.Control);
      }

      public async Task Update()
      {
         await _displayDevice.Draw();
      }

      public void HandleButtonInput(MenuButtons button)
      {

      }

      public void DisplayPopup(UserControl popup)
      {
         _popUpMenuNode = new MenuNode<UserControl>(CurrentMenuNode, popup);

         _displayDevice.ActiveCanvas.Children.Remove(_popUpMenuNode.Parent.Control);

         ChangeCurrentMenu(_popUpMenuNode);
      }

      public void EndPopup(UserControl popup)
      {
         _displayDevice.ActiveCanvas.Children.Remove(_popUpMenuNode.Control);

         ChangeCurrentMenu(_popUpMenuNode.Parent);

         _popUpMenuNode = null;
      }

      private void AddNode(MenuNode<UserControl> node)
      {
         _menuTree.Add(node);
      }
   }
}
