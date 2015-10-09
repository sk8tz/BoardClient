using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Board.Common
{
    public static class UIElementEx
    {
        public static T ParentOfType<T>(this UIElement element) where T : DependencyObject
        {
            if (element == null) return null;
            var parent = VisualTreeHelper.GetParent(element);
            while ((parent != null) && !(parent is T))
            {
                var newVisualParent = VisualTreeHelper.GetParent(parent);
                if (newVisualParent != null)
                {
                    parent = newVisualParent;
                }
                else
                {
                    // try to get the logical parent ( e.g. if in Popup)
                    if (parent is FrameworkElement)
                    {
                        parent = (parent as FrameworkElement).Parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return parent as T;
        }

        /// <summary>
        ///     以类型获取第一子节点
        /// </summary>
        /// <typeparam name="T" />
        /// <peparam />
        /// <param name="p_element"></param>
        /// <returns></returns>
        public static T ChildOfType<T>(this UIElement p_element) where T : UIElement
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(p_element); i++)
            {
                UIElement child = VisualTreeHelper.GetChild(p_element, i) as FrameworkElement;

                if (child == null)
                {
                    continue;
                }

                if (child is T)
                {
                    return (T) child;
                }

                var grandChild = child.ChildOfType<T>();
                if (grandChild != null)
                    return grandChild;
            }

            return null;
        }

        /// <summary>
        ///     获取当前控件树下所在 T 类型节点 （深度遍历）
        /// </summary>
        /// <typeparam name="T" />
        /// <peparam />
        /// <param name="p_element"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindChildrenByType<T>(this UIElement p_element) where T : UIElement
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(p_element); i++)
            {
                UIElement child = VisualTreeHelper.GetChild(p_element, i) as FrameworkElement;
                if (child == null)
                {
                    continue;
                }

                if (child is T)
                {
                    yield return (T) child;
                }
                else
                {
                    foreach (var c in child.FindChildrenByType<T>())
                    {
                        yield return c;
                    }
                }
            }
        }

        /// <summary>
        ///     延迟获得焦点，默认延迟 100ms
        /// </summary>
        /// <param name="p_element"></param>
        /// <param name="p_time"></param>
        public static void FocusDelay(this UIElement p_element, int p_time = 100)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(p_time);

                p_element.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => p_element.Focus()));
            });
        }

        /// <summary>
        ///     获取UIElement的相对位置
        /// </summary>
        /// <param name="source"></param>
        /// <param name="p_relative">相对于UIElement</param>
        /// <returns>坐标值</returns>
        public static Point GetRelativePosition(this UIElement source, UIElement p_relative)
        {
            var pt = new Point();
            var mat = source.TransformToVisual(p_relative) as MatrixTransform;
            if (mat != null)
            {
                pt.X = mat.Matrix.OffsetX;
                pt.Y = mat.Matrix.OffsetY;
            }
            return pt;
        }
    }
}