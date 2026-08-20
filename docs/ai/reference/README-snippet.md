# Add this to README.md

Place it after the Installation section.

---

## Using Dnet.Blazor with an AI coding agent

We publish a machine-readable reference for each release: every component's
parameters, every styling token, plus setup and theming guides. Plain markdown,
so it works with Claude, Codex, Cursor, Gemini CLI or anything else that reads
project docs.

**Download:**
[`dnet-blazor-agent-reference-6.0.2.zip`](https://github.com/ValdtechSSO/DnetBlazor/releases/latest/download/dnet-blazor-agent-reference-6.0.2.zip)
— attached to every release.

Unzip it into your project and point your agent at it. The archive's
`AGENTS-snippet.md` has text you can paste into your own `AGENTS.md`,
`CLAUDE.md` or `.cursorrules`.

```bash
unzip dnet-blazor-agent-reference-6.0.2.zip -d docs/
```

Prefer to browse it first, or pull it straight from source?

```bash
npx degit ValdtechSSO/DnetBlazor/docs/ai/reference docs/dnet-blazor
```

The reference is a snapshot of the version it ships with — grab the matching one
when you upgrade the package.

---

# Also: add to the release checklist

Before tagging a release:

```bash
node tools/build-reference.mjs     # regenerates docs/ai/reference + artifacts/*.zip
git add docs/ai/reference
```

Then **attach `artifacts/dnet-blazor-agent-reference-<version>.zip` to the GitHub
release**. The folder is committed; the zip is not — see below.

---

# Also: add to .gitignore

```gitignore
# Build artifacts (agent reference archives)
artifacts/
```

## Why the folder is committed but the zip isn't

The **folder** is committed because it makes API changes visible in pull request
diffs — if a parameter disappears or a token is renamed, you see it in review
instead of hearing about it from a consumer. That's worth the repo space.

The **zip** is a build artifact. Binaries in git bloat history permanently,
produce useless diffs, and cause a merge conflict on every release. GitHub
Releases is what artifact hosting is for, and it gives you a stable
`releases/latest/download/...` URL to put in the README.
