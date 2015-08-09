using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dominator.Net;
using Dominator.Windows10.Tools;
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

			var nestedStackPanel = new StackPanel
			{
				Margin = new Thickness(16, 8, 0, 8)
			};

			foreach (var nested in group.Nested)
			{
				var nestedUI = ForDominator(nested, context);
				nestedStackPanel.Children.Add(nestedUI);
			}

			panel.Children.Add(nestedStackPanel);
			return panel;
		}

		static readonly Brush WhiteBrush = new SolidColorBrush(Colors.White);
		static readonly Brush RedBrush = new SolidColorBrush(Colors.Red);

		public static UIElement ForItem(IDominatorItem item, IUIRegistrationContext context)
		{
			var panel = new StackPanel();
			panel.Margin = new Thickness(0, 0, 0, 12);
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

			// argh, that **** calls us back when we change the state manually.
			var section = new Soft.Section();

			sw.Checked += (sender, args) =>
			{
				if (!section.IsLocked)
					context.requestAction(item, DominationAction.Dominate);
			};

			sw.Unchecked += (sender, args) =>
			{
				if (!section.IsLocked)
					context.requestAction(item, DominationAction.MakeSubmissive);
			};

			context.registerFeedback(item,
				state =>
				{
					switch (state.State)
					{
						case DominatorState.Dominated:
							using (section.Lock())
								sw.IsChecked = true;
							break;
						case DominatorState.Submissive:
							using (section.Lock())
								sw.IsChecked = false;
							break;
						case DominatorState.Indetermined:
							using (section.Lock())
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
				UncheckedContent = "YES",
				CheckedContent = "NO!",
				Margin = new Thickness(4, 4, 0, 4)
			};
		
			return sw;
		}

		public static UIElement CreateDescription(DominatorDescription description)
		{
			var panel = new StackPanel();
			var hasExplanation = description.Explanation != "";
			var title = new Label
			{
				Content = description.Title,
				// if there is no explanation, the title is the explanation
				FontSize = hasExplanation ? 20 : 16
			};
			panel.Children.Add(title);

			if (description.Explanation != "")
			{
				var expl = new Label
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
