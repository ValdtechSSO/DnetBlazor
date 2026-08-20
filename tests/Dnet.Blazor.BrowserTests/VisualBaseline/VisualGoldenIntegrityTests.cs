using System.Security.Cryptography;
using Xunit;

namespace Dnet.Blazor.BrowserTests.VisualBaseline;

/// <summary>Guards the baseline itself: two named states must never freeze the same PNG.</summary>
[Collection("Visual baseline")]
public sealed class VisualGoldenIntegrityTests
{
    private static readonly string[] Viewports = ["desktop", "mobile"];

    // These are the hover scenarios with a declared visual treatment. Other
    // hover probes exercise interaction but intentionally have no visual delta.
    private static readonly HashSet<string> VisualHoverScenarios = ["button", "chips"];

    [Fact]
    public void Distinct_states_have_distinct_goldens()
    {
        if (!VisualBaselineFixture.Enabled)
        {
            return;
        }

        var testProjectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        foreach (var scenario in ComponentScenarios.All)
        {
            foreach (var viewport in Viewports)
            {
                if (viewport == "mobile" && scenario.SkipMobile)
                {
                    continue;
                }

                var captures = scenario.States
                    .Where(state => state != VisualState.Hover || VisualHoverScenarios.Contains(scenario.Name))
                    .Select(state => new
                {
                    State = state,
                    Path = VisualGoldenComparer.GoldenPath(testProjectRoot, scenario.Name, state.ToString().ToLowerInvariant(), viewport),
                })
                    .ToArray();

                foreach (var capture in captures)
                {
                    Assert.True(File.Exists(capture.Path), $"Missing golden '{capture.Path}'.");
                }

                var duplicates = captures
                    .GroupBy(capture => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture.Path))))
                    .Where(group => group.Count() > 1)
                    .Select(group => string.Join(", ", group.Select(capture => capture.State)))
                    .ToArray();

                Assert.True(duplicates.Length == 0,
                    $"{scenario.Name}/{viewport} has identical goldens for distinct states: {string.Join("; ", duplicates)}.");
            }
        }
    }
}
