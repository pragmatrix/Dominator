using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dominator.Net;
using ToggleSwitch;

namespace Dominator.Windows10
{
	static class UI
	{
		public static UIElement ForDominator(IDominator dominator, IUIRegistrationContext registrationContext)
		{
			return dominator.DispatchTo(group => ForGroup(group, registrationContext), item => ForItem(item, registrationContext));
		}

		public static UIElement ForGroup(IDominatorGroup group, IUIRegistrationContext context)
		{
			var panel = new StackPanel();
			var description = CreateDescription(group.Description);
			panel.Children.Add(description);

			var nestedGrid = new Grid
			{
				Margin = new Thickness(16, 8, 0, 8)
			};

			foreach (var nested in group.Nested)
			{
				var nestedUI = ForDominator(nested, context);
				nestedGrid.Children.Add(nestedUI);
			}

			panel.Children.Add(nestedGrid);
			return panel;
		}

		static readonly Brush WhiteBrush = new SolidColorBrush(Colors.White);
		static readonly Brush RedBrush = new SolidColorBrush(Colors.Red);

		public static UIElement ForItem(IDominatorItem item, IUIRegistrationContext context)
		{
			var panel = new StackPanel();
			var description = CreateDescription(item.Description);
			panel.Children.Add(description);

			var switchPanel = new DockPanel();
			var sw = createSwitch(item, context);
			switchPanel.Children.Add(sw);
			var errorLabel = new Label()
			{
				Content = "ErrorText",
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = RedBrush
			};
			switchPanel.Children.Add(errorLabel);

			context.registerFeedback(item,
				state =>
				{
					switch (state.State)
					{
						case DominatorState.Dominated:
							sw.IsChecked = true;
							break;
						case DominatorState.Submissive:
							sw.IsChecked = false;
							break;
						case DominatorState.Indetermined:
							sw.IsChecked = false;
							break;
					}

					errorLabel.Content = state.Error_ != null ? state.Error_.Message : "";
				});

			panel.Children.Add(switchPanel);
			return panel;
		}

		static HorizontalToggleSwitch createSwitch(IDominatorItem item, IUIRegistrationContext context)
		{
			var sw = new HorizontalToggleSwitch
			{
				UncheckedContent = new Label()
				{
					Content = "NOT OK",
					Foreground = WhiteBrush
				},
				CheckedContent = "OK",
//				HorizontalAlignment = HorizontalAlignment.Left,
				Margin = new Thickness(4, 4, 0, 4)
			};

			sw.Checked += (sender, args) => context.requestAction(item, DominationAction.Dominate);
			sw.Unchecked += (sender, args) => context.requestAction(item, DominationAction.MakeSubmissive);

		
			return sw;
		}

		public static UIElement CreateDescription(DominatorDescription description)
		{
			var panel = new StackPanel();
			var title = new Label()
			{
				Content = description.Title,
				FontSize = 20
			};
			panel.Children.Add(title);
			if (description.Explanation != "")
			{
				var expl = new Label()
				{
					Content = description.Explanation
				};
				panel.Children.Add(expl);
			}
			return panel;
		}
	}

	interface IUIRegistrationContext
	{
		void requestAction(IDominatorItem dominator, DominationAction action);
		void registerFeedback(IDominatorItem dominator, Action<DominationState> feedbackFunction);
	}
}
