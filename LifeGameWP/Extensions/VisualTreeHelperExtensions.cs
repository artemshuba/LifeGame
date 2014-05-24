using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LifeGameWP.Extensions
{
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Gets the visual parent of the element
        /// </summary>
        /// <param name="node">The element to check</param>
        /// <returns>The visual parent</returns>
        public static FrameworkElement GetVisualParent(this FrameworkElement node)
        {
            return VisualTreeHelper.GetParent(node) as FrameworkElement;
        }

        /// <summary>
        /// Gets the ancestors of the element, up to the root
        /// </summary>
        /// <param name="node">The element to start from</param>
        /// <returns>An enumerator of the ancestors</returns>
        public static IEnumerable<FrameworkElement> GetVisualAncestors(this FrameworkElement node)
        {
            FrameworkElement parent = node.GetVisualParent();
            while (parent != null)
            {
                yield return parent;
                parent = parent.GetVisualParent();
            }
        }

        /// <summary>
        /// Helper method that is updated binding on a focused element
        /// The supported elements is TextBox or PasswordBox
        /// </summary>
        /// <param name="ignoreEmpty">true - do not update binding if data is empty</param>
        public static void UpdateBoundText(bool ignoreEmpty = true)
        {
            var focusObj = FocusManager.GetFocusedElement();
            BindingExpression binding = null;
            if (focusObj is TextBox)
            {
                var tb = (TextBox)focusObj;
                if (!ignoreEmpty && string.IsNullOrWhiteSpace(tb.Text))
                    return;

                binding = ((TextBox)focusObj).GetBindingExpression(TextBox.TextProperty);
            }
            else if (focusObj is PasswordBox)
            {
                var ptb = (PasswordBox)focusObj;
                if (!ignoreEmpty && string.IsNullOrWhiteSpace(ptb.Password))
                    return;

                binding = ptb.GetBindingExpression(PasswordBox.PasswordProperty);
            }
            if (binding != null)
            {
                binding.UpdateSource();
            }
        }

        #region GetVisualChildren(...)

        public static IEnumerable<FrameworkElement> GetVisualChildren(this FrameworkElement root)
        {
            if (root == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                yield return VisualTreeHelper.GetChild(root, i) as FrameworkElement;
        }
        #endregion

        /// <summary>
        /// Performs a breadth-first enumeration of all the descendents in the tree
        /// </summary>
        /// <param name="root">The root node</param>
        /// <returns>An enumerator of all the children</returns>
        public static IEnumerable<FrameworkElement> GetVisualDescendents(this FrameworkElement root)
        {

            var toDo = new Queue<IEnumerable<FrameworkElement>>();

            toDo.Enqueue(root.GetVisualChildren());
            while (toDo.Count > 0)
            {
                IEnumerable<FrameworkElement> children = toDo.Dequeue();
                foreach (FrameworkElement child in children)
                {
                    yield return child;
                    toDo.Enqueue(child.GetVisualChildren());
                }
            }
        }

        #region GetFirstLogicalChildByType<T>(...)
        /// <summary>
        /// Retrieves the first logical child of a specified type using a 
        /// breadth-first search.  A visual element is assumed to be a logical 
        /// child of another visual element if they are in the same namescope.
        /// For performance reasons this method manually manages the queue 
        /// instead of using recursion.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <param name="applyTemplates">Specifies whether to apply templates on the traversed framework elements</param>
        /// <returns>The first logical child of the framework element of the specified type.</returns>
        public static T GetFirstLogicalChildByType<T>(this FrameworkElement parent, bool applyTemplates)
            where T : FrameworkElement
        {
            var queue = new Queue<FrameworkElement>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                var elementAsControl = element as Control;

                if (applyTemplates && elementAsControl != null)
                {
                    elementAsControl.ApplyTemplate();
                }

                if (element is T && element != parent)
                {
                    return (T)element;
                }

                foreach (var visualChild in element.GetVisualChildren())
                {
                    queue.Enqueue(visualChild);
                }
            }

            return null;
        }
        #endregion
    }
}
