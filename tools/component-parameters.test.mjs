import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { extractParameters } from "./component-parameters.mjs";

test("extracts auto, attributed, block and expression properties without swallowing the next member", () => {
    const parameters = extractParameters(`
/// <summary>A localized label.</summary>
[Parameter(CaptureUnmatchedValues = false)] [EditorRequired]
public string Label { get => _label ?? Localizer["Common.Search"]; set => _label = value; }
private string? _label;
[Parameter] public Dictionary<string, Func<int, bool>>? Options { get; set; } = new() { ["}; text"] = value => value > 0 };
[Parameter] public int Count { get { if (ready) { return count; } return 0; } set { count = value; } }
[Parameter] public string Description => $"{Format(new { Value = "}" })}";
[Parameter] public int Height { get; set; } = 56;
`);
    assert.deepEqual(parameters.map(p => p.name), ["Label", "Options", "Count", "Description", "Height"]);
    assert.equal(parameters[0].doc, "<summary>A localized label.</summary>");
    assert.equal(parameters[0].computed, true);
    assert.equal(parameters[1].type, "Dictionary<string, Func<int, bool>>?");
    assert.equal(parameters[1].def, 'new() { ["}; text"] = value => value > 0 }');
    assert.equal(parameters[2].computed, true);
    assert.equal(parameters[3].computed, true);
    assert.equal(parameters[4].def, "56");
});

test("ignores commented attributes and balances braces inside verbatim, raw and interpolated strings", () => {
    const source = [
        '// [Parameter] public string Hidden { get; set; }',
        '@* [Parameter] public string AlsoHidden { get; set; } *@',
        '[Parameter] public string Label { get { var a = @"}; ""quoted"""; var b = """ } ; "text" """; return $@"{{literal}} {Format(new { Name = "}" })}"; } set { _label = value; } }',
        '[Parameter] public string Raw { get => $$"""literal {braces} {{Format(new { Text = "}" })}} """; set => _raw = value; }',
        '/* [Parameter] public int Nope { get; set; } */',
        '[global::Microsoft.AspNetCore.Components.ParameterAttribute] public string[] Names { get; init; } = ["one", "two;}"];',
    ].join('\n');
    const parameters = extractParameters(source);
    assert.deepEqual(parameters.map(p => p.name), ["Label", "Raw", "Names"]);
    assert.equal(parameters[2].type, "string[]");
    assert.equal(parameters[2].def, '["one", "two;}"]');
});

test("keeps the complete live Paginator API including all eight localized label parameters", () => {
    const source = readFileSync(new URL("../src/Dnet.Blazor/Components/Paginator/DnetPaginator.razor", import.meta.url), "utf8");
    const parameters = extractParameters(source);
    assert.equal(parameters.length, 22);
    for (const name of ["PaginationLabel", "PageText", "OfText", "PageInputLabel", "FirstPageLabel", "PreviousPageLabel", "NextPageLabel", "LastPageLabel"])
        assert.equal(parameters.find(p => p.name === name)?.computed, true, name);
});
