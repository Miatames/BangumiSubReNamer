using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BangumiSubReNamer.Resources;

public static class TextBlockWrappingBehavior
{
    public static readonly DependencyProperty MaxLinesProperty =
        DependencyProperty.RegisterAttached(
            "MaxLines",
            typeof(int),
            typeof(TextBlockWrappingBehavior),
            new PropertyMetadata(default(int), OnMaxLinesPropertyChangedCallback));

    public static void SetMaxLines(TextBlock element, int value) => element.SetValue(MaxLinesProperty, value);

    public static int GetMaxLines(TextBlock element) =>(int)element.GetValue(MaxLinesProperty);

    private static void OnMaxLinesPropertyChangedCallback(
        DependencyObject d, 
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock) return;
        if (textBlock.IsLoaded)
        {
            SetLineHeight();
        }
        else
        {
            textBlock.Loaded += OnLoaded;

            void OnLoaded(object _, RoutedEventArgs __)
            {
                textBlock.Loaded -= OnLoaded;
                SetLineHeight();
            }
        }

        void SetLineHeight()
        {
            double lineHeight =
                double.IsNaN(textBlock.LineHeight)
                    ? textBlock.FontFamily.LineSpacing * textBlock.FontSize
                    : textBlock.LineHeight;
            textBlock.MaxHeight = Math.Ceiling(lineHeight * GetMaxLines(textBlock));
        }
    }
}