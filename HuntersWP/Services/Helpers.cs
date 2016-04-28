using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Phone.Net.NetworkInformation;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace HuntersWP.Services
{
    public static class Helpers
    {
        public static bool IsNetworkAvailable
        {
            get
            {
               // return false;

                return NetworkInterface.GetIsNetworkAvailable();
            }
        }

        public static void GetItemsInViewPort(this ItemsControl list, IList<WeakReference> items)
        {
            int index;
            FrameworkElement container;
            GeneralTransform itemTransform;
            Rect boundingBox;

            if (VisualTreeHelper.GetChildrenCount(list) == 0)
            {
                // no child yet
                return;
            }

            ScrollViewer scrollHost = VisualTreeHelper.GetChild(list, 0) as ScrollViewer;

            list.UpdateLayout();

            if (scrollHost == null)
            {
                return;
            }

            for (index = 0; index < list.Items.Count; index++)
            {
                container = (FrameworkElement)list.ItemContainerGenerator.ContainerFromIndex(index);
                if (container != null)
                {
                    itemTransform = null;
                    try
                    {
                        itemTransform = container.TransformToVisual(scrollHost);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore failures when not in the visual tree
                        return;
                    }

                    boundingBox = new Rect(itemTransform.Transform(new Point()), itemTransform.Transform(new Point(container.ActualWidth, container.ActualHeight)));

                    if (boundingBox.Bottom > 0)
                    {
                        items.Add(new WeakReference(container));
                        index++;
                        break;
                    }
                }
            }

            for (; index < list.Items.Count; index++)
            {
                container = (FrameworkElement)list.ItemContainerGenerator.ContainerFromIndex(index);
                itemTransform = null;
                try
                {
                    itemTransform = container.TransformToVisual(scrollHost);
                }
                catch (ArgumentException)
                {
                    // Ignore failures when not in the visual tree
                    return;
                }

                boundingBox = new Rect(itemTransform.Transform(new Point()), itemTransform.Transform(new Point(container.ActualWidth, container.ActualHeight)));

                if (boundingBox.Top < scrollHost.ActualHeight)
                {
                    items.Add(new WeakReference(container));
                }
                else
                {
                    break;
                }
            }

            return;
        }

        public static IList<WeakReference> GetItemsInViewPort(this ItemsControl list)
        {
            IList<WeakReference> viewPortItems = new List<WeakReference>();

            GetItemsInViewPort(list, viewPortItems);

            return viewPortItems;
        }


        internal static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int counter = 0; counter < childCount; counter++)
            {
                yield return VisualTreeHelper.GetChild(parent, counter);
            }
        }

        private const string ExternalAddress = "app://external/";
        public static bool IsExternalNavigation(this Uri uri)
        {
            return (ExternalAddress == uri.ToString());
        }

        public static void RegisterNotification(this FrameworkElement element, string propertyName, PropertyChangedCallback callback)
        {
            DependencyProperty prop = DependencyProperty.RegisterAttached("Notification" + propertyName,
                                                                          typeof(object),
                                                                          typeof(FrameworkElement),
                                                                          new PropertyMetadata(callback));

            element.SetBinding(prop, new Binding(propertyName) { Source = element });
        }  

        public static MemberInfo GetMemberInfo(this System.Linq.Expressions.Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            return memberExpression.Member;
        }

        public static string Sha1(string value)
        {
            var sha = new SHA1Managed();
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            byte[] resultHash = sha.ComputeHash(bytes);

            return resultHash.Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class BooleanToOppositeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Collapsed;
        }
    }

    public sealed class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var completed = (bool)value;
            var color = completed ? new SolidColorBrush(Color.FromArgb(128,0,128,0)) : new SolidColorBrush(Color.FromArgb(255, 100, 160, 200));
            return color;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value is Visibility && (Visibility)value == Visibility.Visible;
            var completed = (bool)value;
            var color = completed ? new SolidColorBrush(Color.FromArgb(128, 0, 128, 0)) : new SolidColorBrush(Color.FromArgb(255, 100, 160, 200));
            return color;

        }
    }

}
