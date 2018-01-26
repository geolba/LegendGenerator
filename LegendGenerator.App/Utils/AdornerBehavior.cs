using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LegendGenerator.App.Utils
{
    public static class AdornerBehavior
    {
        public static bool GetWrapWithAdornerDecorator(TabItem tabItem)
        {
            return (bool)tabItem.GetValue(WrapWithAdornerDecoratorProperty);
        }
        public static void SetWrapWithAdornerDecorator(TabItem tabItem, bool value)
        {
            tabItem.SetValue(WrapWithAdornerDecoratorProperty, value);
        }

        // Using a DependencyProperty as the backing store for WrapWithAdornerDecorator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WrapWithAdornerDecoratorProperty =
            DependencyProperty.RegisterAttached("WrapWithAdornerDecorator", typeof(bool), typeof(AdornerBehavior), new UIPropertyMetadata(false, OnWrapWithAdornerDecoratorChanged));

        public static void OnWrapWithAdornerDecoratorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var tabItem = o as TabItem;
            if (tabItem == null) return;

            if (e.NewValue as bool? == true)
            {
                if (tabItem.Content is AdornerDecorator) return;
                var content = tabItem.Content as UIElement;
                tabItem.Content = null;
                tabItem.Content = new AdornerDecorator { Child = content };
            }
            if (e.NewValue as bool? == false)
            {
                if (tabItem.Content is AdornerDecorator)
                {
                    var decorator = tabItem.Content as AdornerDecorator;
                    var content = decorator.Child;
                    decorator.Child = null;
                    tabItem.Content = content;
                }
            }
        }
    }
}
