using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Dominator.Net;
using Dominator.Windows10.Tools;
using ToggleSwitch;
using static Dominator.Windows10.Localization.Application;

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
			errorLabel.Margin = new Thickness(DefaultMargin*2, 0, DefaultMargin, 0);

			var messageLabel = CreateTextBlock("");
			messageLabel.VerticalAlignment = VerticalAlignment.Center;
			messageLabel.Margin = new Thickness(DefaultMargin * 2, 0, DefaultMargin, 0);

			switchPanel.Children.Add(errorLabel);
			switchPanel.Children.Add(messageLabel);

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
					bool error = state.Error_ != null;
					if (error)
					{
						errorLabel.Text = state.Error_.Message;
					}
					else
					{
						Debug.Assert(state.State_ != null);
						switch (state.State_.Value.Kind)
						{
						case DominatorStateKind.Dominated:
							using (section.Lock())
								sw.IsChecked = true;
							break;
						case DominatorStateKind.Submissive:
							using (section.Lock())
								sw.IsChecked = false;
							sw.UncheckedContent = L_YES;
							sw.UncheckedBackground = UncheckedBackgroundRed;
						break;
						case DominatorStateKind.Indetermined:
							using (section.Lock())
								sw.IsChecked = false;
							sw.UncheckedContent = L_N_A;
							sw.UncheckedBackground = UncheckedBackgroundOrange;
						break;
						}

						messageLabel.Text = state.State_.Value.Message;
					}

					errorLabel.Visibility = error ? Visibility.Visible : Visibility.Collapsed;
					messageLabel.Visibility = error ? Visibility.Collapsed : Visibility.Visible;
				});

			panel.Children.Add(switchPanel);
			return panel;
		}

		static HorizontalToggleSwitch createSwitch()
		{
			var sw = new HorizontalToggleSwitch
			{
				UncheckedContent = L_YES,
				CheckedContent = L_NO_,
				Margin = new Thickness(DefaultMargin, DefaultMargin, 0, DefaultMargin)
			};
		
			return sw;
		}

		static readonly Brush UncheckedBackgroundOrange = new SolidColorBrush(Colors.Orange);
		static readonly Brush UncheckedBackgroundRed = getOriginalUncheckedBrush();

		static Brush getOriginalUncheckedBrush()
		{
			var sw = new HorizontalToggleSwitch();
			return sw.UncheckedBackground;
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
