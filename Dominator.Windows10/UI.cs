using System.Windows;
using System.Windows.Controls;
using Dominator.Net;

namespace Dominator.Windows10
{
	static class UI
	{
		public static void Populate(Panel parent, IDominator dominator)
		{
			dominator.DispatchTo(group => PopulateGroup(parent, group), item => PopulateItem(parent, item));
		}

		public static void PopulateItem(Panel parent, IDominatorItem item)
		{
			var itemBox = new StackPanel();
			

			parent.Children.Add(itemBox);
		}

		public static void PopulateGroup(Panel parent, IDominatorGroup group)
		{
			var description = CreateDescription(group.Description);
			parent.Children.Add(description);
		}

		public static UIElement CreateDescription(DominatorDescription description)
		{
			var itemBox = new StackPanel();
			var title = new Label()
			{
				Content = description.Title,
				FontSize = 14
			};
			itemBox.Children.Add(title);
			if (description.Explanation != "")
			{
				var expl = new Label()
				{
					Content = description.Explanation
				};
				itemBox.Children.Add(expl);
			}
			return itemBox;
		}
	}
}
