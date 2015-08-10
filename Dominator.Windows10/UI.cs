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
			var description = CreateDescription(group.Description, forGroup: true);
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

		static readonly Brush RedBrush = new SolidColorBrush(Colors.Red);
		const int DefaultMargin = 6;

		public static UIElement ForItem(IDominatorItem item, IUIRegistrationContext context)
		{
			var panel = new StackPanel
			{
				Margin = new Thickness(0, 0, 0, DefaultMargin * 2)
			};
			var description = CreateDescription(item.Description, forGroup: false);
			panel.Children.Add(description);

			var switchPanel = new DockPanel();
			var sw = createSwitch();
			switchPanel.Children.Add(sw);
			// we don't want to use actual Label controls, because they parse '_' underscores as
			// Alt Key shortcuts.
			var errorLabel = CreateTextBlock("");
			errorLabel.VerticalAlignment = VerticalAlignment.Center;
			errorLabel.Foreground = RedBrush;
			errorLabel.Margin = new Thickness(DefaultMargin*2, DefaultMargin, DefaultMargin, DefaultMargin);

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

					errorLabel.Text = state.Error_ != null ? state.Error_.Message : "";
				});

			panel.Children.Add(switchPanel);
			return panel;
		}

		static HorizontalToggleSwitch createSwitch()
		{
			var sw = new HorizontalToggleSwitch
			{
				UncheckedContent = "YES",
				CheckedContent = "NO!",
				Margin = new Thickness(DefaultMargin, DefaultMargin, 0, DefaultMargin)
			};
		
			return sw;
		}

		public static UIElement CreateDescription(DominatorDescription description, bool forGroup)
		{
			var panel = new StackPanel();
			var hasExplanation = description.Explanation != "";
			var title = CreateTextBlock(description.Title);
			// if there is no explanation, the title is the explanation
			title.FontSize = hasExplanation || forGroup ? 22 : 16;
			panel.Children.Add(title);

			if (description.Explanation != "")
			{
				var explanationLabel = CreateTextBlock(description.Explanation);
				panel.Children.Add(explanationLabel);
			}
			return panel;
		}

		public static TextBlock CreateTextBlock(string text)
		{
			return new TextBlock
			{
				Margin = new Thickness(DefaultMargin),
				Text = text,
				TextWrapping = TextWrapping.WrapWithOverflow
			};
		}
	}

	interface IUIRegistrationContext
	{
		void requestAction(IDominatorItem dominator, DominationAction action);
		void registerFeedback(IDominatorItem dominator, Action<DominationState> feedbackFunction);
	}
}
