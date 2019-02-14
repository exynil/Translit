using System.Windows;
using System.Windows.Input;

namespace Translit.Models.Other
{
	public static class DropBehavior
	{
		private static readonly DependencyProperty PreviewDropCommandProperty =
					DependencyProperty.RegisterAttached
					(
						"PreviewDropCommand",
						typeof(ICommand),
						typeof(DropBehavior),
						new PropertyMetadata(PreviewDropCommandPropertyChangedCallBack)
					);

		public static void SetPreviewDropCommand(this UIElement inUiElement, ICommand inCommand)
		{
			inUiElement.SetValue(PreviewDropCommandProperty, inCommand);
		}

		private static ICommand GetPreviewDropCommand(UIElement inUiElement)
		{
			return (ICommand)inUiElement.GetValue(PreviewDropCommandProperty);
		}
		
		private static void PreviewDropCommandPropertyChangedCallBack(
			DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
		{
			if (!(inDependencyObject is UIElement uiElement)) return;

			uiElement.Drop += (sender, args) =>
			{
				GetPreviewDropCommand(uiElement).Execute(args.Data);
				args.Handled = true;
			};
		}
	}
}
