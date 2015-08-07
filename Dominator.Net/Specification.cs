namespace Dominator.Net
{
	public enum DominatorState
	{
		Submissive,
		Dominated,
		Indetermined
	}

	public struct DominatorDescription
	{
		public DominatorDescription(string title, string explanation)
		{
			Title = title;
			Explanation = explanation;
		}

		public readonly string Title;
		public readonly string Explanation;
	}

	public interface IDominator
	{
		DominatorDescription Description { get; }
	}

	public interface IDominatorItem : IDominator
	{
		DominatorState GetState();
		void MakeSubmissive();
		void Dominate();
	}

	public interface IDominatorGroup : IDominator
	{
		IDominator[] Nested { get; }
	}
}
