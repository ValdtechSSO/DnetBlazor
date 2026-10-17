/** Shared, source-only extraction of attributed C# component properties. */

const whitespace = (value) => value.replace(/\s+/g, " ").trim();

function skipComment(source, start) {
    if (source.startsWith("//", start)) {
        const end = source.indexOf("\n", start + 2);
        return end < 0 ? source.length : end;
    }
    if (source.startsWith("/*", start) || source.startsWith("@*", start)) {
        const terminator = source.startsWith("/*", start) ? "*/" : "*@";
        const end = source.indexOf(terminator, start + 2);
        return end < 0 ? source.length : end + 2;
    }
    return start;
}

function skipInterpolation(source, start, braces) {
    let depth = 0;
    for (let i = start; i < source.length;) {
        const comment = skipComment(source, i);
        if (comment > i) { i = comment; continue; }
        const quoted = skipQuoted(source, i);
        if (quoted > i) { i = quoted; continue; }
        if (depth === 0 && source.startsWith("}".repeat(braces), i)) return i + braces;
        if (source[i] === "{") depth++;
        else if (source[i] === "}") depth--;
        i++;
    }
    return source.length;
}

function skipQuoted(source, start) {
    if (source[start] === "'") {
        for (let i = start + 1; i < source.length; i++) {
            if (source[i] === "\\") i++;
            else if (source[i] === "'") return i + 1;
        }
        return source.length;
    }
    const prefix = /^(\$+@?|@\$?)?"/.exec(source.slice(start));
    if (!prefix) return start;
    const marker = prefix[1] ?? "";
    const dollars = (marker.match(/\$/g) ?? []).length;
    const verbatim = marker.includes("@");
    const opening = start + marker.length;
    let quotes = 1;
    while (source[opening + quotes] === '"') quotes++;
    const raw = quotes >= 3;
    if (!raw) quotes = 1;
    for (let i = opening + quotes; i < source.length;) {
        if (raw) {
            if (source.startsWith('"'.repeat(quotes), i)) return i + quotes;
            if (dollars && source.startsWith("{".repeat(dollars), i)) {
                i = skipInterpolation(source, i + dollars, dollars);
                continue;
            }
        } else {
            if (!verbatim && source[i] === "\\") { i += 2; continue; }
            if (verbatim && source.startsWith('""', i)) { i += 2; continue; }
            if (source[i] === '"') return i + 1;
            if (dollars && (source.startsWith("{{", i) || source.startsWith("}}", i))) { i += 2; continue; }
            if (dollars && source[i] === "{") { i = skipInterpolation(source, i + 1, 1); continue; }
        }
        i++;
    }
    return source.length;
}

// Keep offsets and line breaks: the original source still supplies documentation
// and initializer text, while braces inside literals cannot terminate a property.
function codeMask(source) {
    const result = source.split("");
    for (let i = 0; i < source.length;) {
        const end = Math.max(skipComment(source, i), skipQuoted(source, i));
        if (end > i) {
            for (let j = i; j < end; j++) if (result[j] !== "\n" && result[j] !== "\r") result[j] = " ";
            i = end;
        } else i++;
    }
    return result.join("");
}

function closingDelimiter(code, start) {
    const pairs = { "[": "]", "{": "}", "(": ")" };
    const stack = [pairs[code[start]]];
    for (let i = start + 1; i < code.length; i++) {
        if (pairs[code[i]]) stack.push(pairs[code[i]]);
        else if (code[i] === stack.at(-1)) {
            stack.pop();
            if (!stack.length) return i;
        }
    }
    return -1;
}

function expressionEnd(code, start) {
    for (let i = start; i < code.length; i++) {
        if ("[{(".includes(code[i])) {
            i = closingDelimiter(code, i);
            if (i < 0) return code.length;
        } else if (code[i] === ";") return i;
    }
    return code.length;
}

function documentation(source, start) {
    const lines = source.slice(0, start).split(/\r?\n/);
    let doc = "";
    for (let i = lines.length - 1; i >= 0; i--) {
        const line = lines[i].trim();
        if (line.startsWith("///")) { doc = line.replace(/^\/\/\/\s?/, "") + " " + doc; continue; }
        if (!line || line.startsWith("[")) continue;
        break;
    }
    return doc.trim();
}

/** Extract public [Parameter] properties, including custom and expression accessors. */
export function extractParameters(source) {
    const code = codeMask(source);
    const parameters = [];
    for (let i = 0; i < code.length; i++) {
        if (code[i] !== "[") continue;
        const attributeEnd = closingDelimiter(code, i);
        if (attributeEnd < 0) continue;
        const attribute = code.slice(i + 1, attributeEnd);
        if (!/(?:^|,)\s*(?:(?:global::)?[\w.]+\.)?Parameter(?:Attribute)?\b/.test(attribute)) { i = attributeEnd; continue; }
        let start = attributeEnd + 1;
        while (start < code.length) {
            if (/\s/.test(code[start])) { start++; continue; }
            if (code[start] === "[") { start = closingDelimiter(code, start) + 1; if (!start) break; continue; }
            break;
        }
        const declaration = /^public\s+(?:(?:new|virtual|override|sealed|required|readonly|unsafe|static)\s+)*([\w?<>,\s.[\]():]+?)\s+(@?\w+)\s*(\{|=>)/.exec(code.slice(start));
        if (!declaration) continue;
        const type = whitespace(declaration[1]);
        const name = declaration[2].replace(/^@/, "");
        const bodyStart = start + declaration[0].length - declaration[3].length;
        let bodyEnd;
        let computed = false;
        let def = null;
        if (declaration[3] === "{") {
            bodyEnd = closingDelimiter(code, bodyStart);
            if (bodyEnd < 0) continue;
            const body = code.slice(bodyStart + 1, bodyEnd);
            if (!/\bget\b/.test(body)) continue;
            computed = !/^\s*(?:(?:public|private|protected|internal)\s+)?get\s*;\s*(?:(?:public|private|protected|internal)\s+)?(?:set|init)\s*;\s*$/.test(body);
            let next = bodyEnd + 1;
            while (/\s/.test(code[next] ?? "") && next < code.length) next++;
            if (code[next] === "=" && code[next + 1] !== "=") {
                bodyEnd = expressionEnd(code, next + 1);
                def = source.slice(next + 1, bodyEnd).trim();
                computed = false;
            }
        } else {
            bodyEnd = expressionEnd(code, bodyStart + 2);
            computed = true;
        }
        parameters.push({ type, name, def, computed, doc: documentation(source, i), index: i });
        i = bodyEnd;
    }
    return parameters;
}
