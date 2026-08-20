using Xunit;

namespace Dnet.Blazor.BrowserTests.VisualBaseline;

/// <summary>Visual captures and their integrity check share mutable golden files.</summary>
[CollectionDefinition("Visual baseline", DisableParallelization = true)]
public sealed class VisualBaselineCollection : ICollectionFixture<VisualBaselineFixture>
{
}
