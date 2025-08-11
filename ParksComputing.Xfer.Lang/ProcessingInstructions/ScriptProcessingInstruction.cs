using System;
using System.Linq;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing Instruction that executes scripting operator invocations prior to parsing its target element.
/// New syntax example: <c>&lt;! script ( let name &lt;value&gt; ) !&gt; &lt;targetElement&gt;</c>
/// The <c>let</c> operator uses a plain <see cref="IdentifierElement"/> for the binding name instead of a reference element.
/// Additional operators may be appended within the same tuple in future extensions.
/// </summary>
public class ScriptProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The processing instruction keyword (<c>script</c>).
    /// </summary>
    public const string Keyword = "script";
    /// <summary>
    /// The tuple of operator invocations to execute prior to parsing the target element.
    /// Currently supports <c>let</c> bindings; future operators may be added.
    /// </summary>
    public TupleElement? Operations { get; }
    private readonly Parser _parser;

    internal ScriptProcessingInstruction(KeyValuePairElement kvp, Parser parser) : base(kvp.Value, Keyword) {
        _parser = parser;
        if (kvp.Value is TupleElement tuple) {
            Operations = tuple; // Children may not yet be fully parsed at construction time
        }
    }

    private bool _operatorsExecuted = false; // ensure we only execute once

    private void ExecuteOperatorsEarly() {
        if (_operatorsExecuted) { return; }
        if (Operations == null) { return; }
        bool onlyLets = true;
    // TEMP TRACE: verify execution timing and operation enumeration
    // Reduced trace noise: only emit a concise begin message in debug builds
#if DEBUG
    _parser.AddWarning(WarningType.Trace, $"[trace] script PI begin ops={Operations.Children.Count}", Keyword);
#endif
        for (int i = 0; i < Operations.Children.Count; i++) {
            var child = Operations.Children[i];

            // Form 1: legacy flat token sequence ( let name value ) that we originally designed for.
            if (child is KeywordElement kwFlat && string.Equals(kwFlat.Value, "let", StringComparison.Ordinal)) {
                if (i + 2 >= Operations.Children.Count) {
                    throw new InvalidOperationException("let requires: let <name> <value>");
                }
                var nameElement = Operations.Children[i + 1];
                string name = nameElement switch {
                    IdentifierElement idElem => idElem.Value,
                    KeywordElement kwName => kwName.Value,
                    _ => throw new InvalidOperationException("let binding name must be an identifier or bareword")
                };
                var valueElem = Operations.Children[i + 2];
                if (string.IsNullOrEmpty(name)) { throw new InvalidOperationException("let binding name cannot be empty"); }
                if (ContainsSelfDereference(valueElem, name)) { throw new InvalidOperationException($"Self reference in let binding '{name}'."); }
                ResolveDereferences(valueElem, _parser);
                _parser.BindReference(name, valueElem);
                // Core binding trace retained (optional) - downgrade by guarding with DEBUG
#if DEBUG
                _parser.AddWarning(WarningType.Trace, $"[trace] script PI let '{name}' -> {valueElem.GetType().Name}", name);
#endif
                i += 2; // skip name + value
                continue;
            }

            // Form 2: Parsed as a KeyValuePairElement with key 'let' and value another KVP representing name/value.
            if (child is KeyValuePairElement kvp && string.Equals(kvp.Key, "let", StringComparison.Ordinal)) {
                // Expect kvp.Value to be a KeyValuePairElement (name -> value) or an Object/Tuple/etc (unsupported yet)
                if (kvp.Value is KeyValuePairElement inner) {
                    var name = inner.Key ?? string.Empty;
                    if (string.IsNullOrEmpty(name)) { throw new InvalidOperationException("let binding name cannot be empty"); }
                    var valueElem = inner.Value;
                    if (ContainsSelfDereference(valueElem, name)) { throw new InvalidOperationException($"Self reference in let binding '{name}'."); }
                    ResolveDereferences(valueElem, _parser);
                    _parser.BindReference(name, valueElem);
                    #if DEBUG
                    _parser.AddWarning(WarningType.Trace, $"[trace] script PI let '{name}' -> {valueElem.GetType().Name}", name);
                    #endif
                    continue;
                }
                else {
                    // Future enhancement: support tuple/object grouping after 'let'
                    throw new InvalidOperationException("Unsupported let binding structure; expected ( let name <value> ) or ( let ( name <value> ) ).");
                }
            }

            // Non-let child: keep PI visible.
            onlyLets = false;
        }
    if (onlyLets) { SuppressSerialization = true; }
#if DEBUG
    _parser.AddWarning(WarningType.Trace, $"[trace] script PI end onlyLets={onlyLets}", Keyword);
#endif
        _operatorsExecuted = true;
    }

    private void ResolveNewlyAvailableDereferences(Element element) {
        // Mirror the parser's post-pass logic but confined to this subtree.
        if (element is ObjectElement obj) {
            foreach (var kv in obj.Dictionary) {
                var v = kv.Value.Value;
                if (v is ReferenceElement d2) {
                    if (_parser.TryResolveBinding(d2.Value, out var bound2)) {
                        kv.Value.Value = Helpers.ElementCloner.Clone(bound2!);
                        // local-pass resolution trace
                        #if DEBUG
                        _parser.AddWarning(WarningType.Trace, $"[trace] local-pass resolved '{d2.Value}' (object)", Keyword);
                        #endif
                        // suppress earlier unresolved warning if present
                        SuppressEarlierUnresolved(d2.Value);
                    }
                } else {
                    ResolveNewlyAvailableDereferences(v);
                }
            }
        } else if (element is CollectionElement coll) {
            for (int i = 0; i < coll.Children.Count; i++) {
                var child = coll.Children[i];
                if (child is ReferenceElement d) {
                    if (_parser.TryResolveBinding(d.Value, out var bound)) {
                        coll.Children[i] = Helpers.ElementCloner.Clone(bound!);
                        #if DEBUG
                        _parser.AddWarning(WarningType.Trace, $"[trace] local-pass resolved '{d.Value}' (collection)", Keyword);
                        #endif
                        SuppressEarlierUnresolved(d.Value);
                    }
                } else {
                    ResolveNewlyAvailableDereferences(child);
                }
            }
        }
    }

    private void SuppressEarlierUnresolved(string name) {
        var doc = _parser.CurrentDocument;
        if (doc == null) { return; }
        // find most recent unresolved warning for this name
        for (int i = doc.Warnings.Count - 1; i >= 0; i--) {
            var w = doc.Warnings[i];
            if (w.Type == WarningType.UnresolvedReference && string.Equals(w.Context, name, StringComparison.Ordinal)) {
                // convert to trace indicating suppression
                doc.Warnings[i] = new ParseWarning(WarningType.Trace, w.Message + " (suppressed: resolved in local-pass)", w.Row, w.Column, w.Context);
                break;
            }
        }
    }

    internal static bool ContainsSelfDereference(Element element, string name) {
        if (element is ReferenceElement de && de.Value == name) {
            return true;
        }
        if (element is CollectionElement coll) {
            foreach (var child in coll.Children) {
                if (ContainsSelfDereference(child, name)) { return true; }
            }
        } else if (element is ObjectElement obj) {
            foreach (var kv in obj.Dictionary) {
                if (ContainsSelfDereference(kv.Value.Value, name)) { return true; }
            }
        }
        return false;
    }

    internal static void ResolveDereferences(Element element, Parser parser) {
        if (element is ReferenceElement deref) {
            if (parser.TryResolveBinding(deref.Value, out var bound)) {
                // Replace element in its parent context by mutating where possible is complex; since we
                // resolve before binding insertion for lets, we can just ignore (valueElement may hold a collection).
                // Here we do nothing because dereferences are resolved during initial parse already. This is a placeholder
                // for future evaluation logic if value expressions become lazily parsed.
            }
            return;
        }
        if (element is CollectionElement coll) {
            for (int i = 0; i < coll.Children.Count; i++) {
                var child = coll.Children[i];
                if (child is ReferenceElement derefChild) {
                    if (parser.TryResolveBinding(derefChild.Value, out var bound2)) {
                        coll.Children[i] = Helpers.ElementCloner.Clone(bound2!);
                    }
                } else {
                    ResolveDereferences(child, parser);
                }
            }
        } else if (element is ObjectElement obj) {
            foreach (var kv in obj.Dictionary) {
                var v = kv.Value.Value;
                if (v is ReferenceElement d2) {
                    if (parser.TryResolveBinding(d2.Value, out var bound3)) {
                        kv.Value.Value = Helpers.ElementCloner.Clone(bound3!);
                    }
                } else {
                    ResolveDereferences(v, parser);
                }
            }
        }
    }

    /// <summary>
    /// Executes supported operators (currently <c>let</c>) before normal element processing and updates
    /// the target subtree to resolve any dereferences that became available.
    /// </summary>
    /// <param name="element">The target element associated with this PI.</param>
    public override void ElementHandler(Element element) {
        // Execute operators now that containing tuple/object content has been parsed.
        ExecuteOperatorsEarly();
    // After executing lets, attempt local dereference resolution inside the target subtree
    ResolveNewlyAvailableDereferences(element);
        base.ElementHandler(element);
        // If suppressed, attempt to remove self from parent collection to fully disappear (optional cleanup)
        if (SuppressSerialization && element.Parent is CollectionElement parent) {
            // Parent still holds this PI as a child; remove it.
            for (int i = parent.Children.Count - 1; i >= 0; i--) {
                if (ReferenceEquals(parent.Children[i], this)) {
                    parent.Children.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Allows the parser to execute 'let' operators for a top-level script PI
    /// before the root collection element is parsed. This ensures bindings are
    /// in place so that dereferences inside the soon-to-be-parsed root element
    /// can resolve immediately rather than deferring to the post-pass.
    /// </summary>
    internal void ExecuteTopLevelEarlyBindings() => ExecuteOperatorsEarly();
}
