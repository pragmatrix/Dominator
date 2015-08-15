using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

			var switchPanel = new DockPanel
			{
				Margin = new Thickness(0, DefaultMargin, 0, 0)
			};

			var sw = createSwitch();
			sw.VerticalAlignment = VerticalAlignment.Top;
			
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
				Margin = new Thickness(DefaultMargin, 0, 0, 0),
				Padding = new Thickness(0)
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
			var showTitle = hasExplanation;

			if (showTitle)
			{
				var title = CreateTextBlock(description.Title);
				title.FontSize = forGroup ? 22 : 16;
				panel.Children.Add(title);
			}

			var explanation = hasExplanation ? description.Explanation : description.Title;

			var explanationLabel = CreateTextBlock(explanation);
			if (!showTitle)
				explanationLabel.FontSize = forGroup ? 22 : 16;

			if (description.More_ != null)
			{
				var hPanel = new Grid
				{
					ColumnDefinitions =
					{
						new ColumnDefinition
						{
							Width=new GridLength(1, GridUnitType.Star)
						},
						new ColumnDefinition
						{
							Width = GridLength.Auto
						}
					}
				};
				hPanel.Children.Add(explanationLabel);
				var moreLink = createHyperlink("more...", description.More_.Value.Action, description.More_.Value.Info);
				moreLink.Margin = new Thickness(0, 0, 0, DefaultMargin);
				moreLink.VerticalAlignment = VerticalAlignment.Bottom;
				Grid.SetColumn(moreLink, 1);
				hPanel.Children.Add(moreLink);
				panel.Children.Add(hPanel);
			}
			else
			{
				panel.Children.Add(explanationLabel);
			}

			return panel;
		}

		static TextBlock createHyperlink(string text, Action action, string info)
		{
			var hl = new Hyperlink
			{
				Inlines =
				{
					new Run
					{
						Text = text
					}
				}
			};

			hl.Click += (sender, args) =>
			{
				try
				{
					action();
				}
				catch (Exception)
				{
					// ignored
				}
			};

            var tb = new TextBlock
			{
				Inlines = { hl },
			};

			if (info != "")
			{
				tb.ToolTip = info;
			}

			return tb;
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
