using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Standalone <c>let</c> processing instruction: <c>&lt;! let name &lt;value&gt; !&gt;</c>
/// Mirrors the <c>let</c> operator used inside a script processing instruction but as a first-class PI.
/// </summary>
public class LetProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The processing instruction keyword (<c>let</c>).
    /// </summary>
    public const string Keyword = "let";
    /// <summary>
    /// The identifier introduced into the current binding scope by this PI.
    /// Distinct from the PI keyword ("let").
    /// </summary>
    public string BindingName { get; }
    /// <summary>
    /// The (possibly structured) value element bound to <see cref="BindingName"/>.
    /// All internal dereferences are resolved before binding.
    /// </summary>
    public Element BoundValue { get; private set; }
    private readonly Parser _parser;

    internal LetProcessingInstruction(KeyValuePairElement kvp, Parser parser) : base(kvp.Value, Keyword) {
        _parser = parser;
        // Expect kvp.Value to be a KeyValuePairElement representing name->value or a tuple ( name value )
        if (kvp.Value is KeyValuePairElement inner) {
            BindingName = inner.Key ?? string.Empty;
            BoundValue = inner.Value;
        }
        else if (kvp.Value is TupleElement tuple) {
            if (tuple.Children.Count == 1 && tuple.Children[0] is KeyValuePairElement innerKvp && innerKvp.Value is not null) {
                // Support form: <! let ( name value ) !> that parser normalized to single KVP child inside tuple
                BindingName = innerKvp.Key ?? string.Empty;
                BoundValue = innerKvp.Value;
            }
            else if (tuple.Children.Count >= 2) {
                var nameElem = tuple.Children[0];
                var valueElem = tuple.Children[1];
                BindingName = nameElem switch {
                    IdentifierElement id => id.Value,
                    KeywordElement kw => kw.Value,
                    TextElement te => te.Value,
                    _ => throw new InvalidOperationException("let binding name must be identifier or bareword text")
                };
                BoundValue = valueElem;
            }
            else {
                throw new InvalidOperationException("let PI requires: <! let name <value> !>");
            }
        }
        else {
            throw new InvalidOperationException("Unsupported let PI structure; expected <! let name <value> !> or <! let ( name <value> ) !>");
        }
        if (string.IsNullOrEmpty(BindingName)) { throw new InvalidOperationException("let binding name cannot be empty"); }
    }

    /// <summary>
    /// Binds the resolved value to <see cref="BindingName"/> in the parser scope so subsequent siblings can dereference it.
    /// Suppresses serialization of the PI itself.
    /// </summary>
    /// <param name="element">The target element the PI is attached to.</param>
    public override void ElementHandler(Element element) {
        // Bind immediately so following siblings can resolve
        if (ContainsSelfDereference(BoundValue, BindingName)) { throw new InvalidOperationException($"Self reference in let binding '{BindingName}'."); }
        // Resolve any dereferences inside the value using existing helper from script PI
        ScriptProcessingInstruction.ResolveDereferences(BoundValue, _parser);
        // Avoid double-binding if ElementHandler invoked after early execution
        if (!_parser.TryResolveBinding(BindingName, out var _existing)) {
                // Pre-bind the variable name with a placeholder so references in subsequent lets can resolve
                // Use a NullElement as placeholder (could be replaced with a dedicated PlaceholderElement if needed)
                if (!_parser.TryResolveBinding(BindingName, out _)) {
                    _parser.BindReference(BindingName, new NullElement());
                }

                // Update the binding with the real value
                _parser.BindReference(BindingName, BoundValue);
        }
        SuppressSerialization = true; // let PI should disappear after binding
        base.ElementHandler(element);
    }

    /// <summary>
    /// Executes the binding logic prior to normal element addition in contexts that support
    /// early PI execution (e.g., top-level document, collection elements). This mirrors
    /// early execution for script PIs so that later siblings can resolve the new binding during parse.
    /// </summary>
    internal void ExecuteEarly() {
        if (ContainsSelfDereference(BoundValue, BindingName)) { throw new InvalidOperationException($"Self reference in let binding '{BindingName}'."); }
        ScriptProcessingInstruction.ResolveDereferences(BoundValue, _parser);
        if (!_parser.TryResolveBinding(BindingName, out var _existing)) {
            _parser.BindReference(BindingName, BoundValue);
        }
        SuppressSerialization = true;
    }

    private static bool ContainsSelfDereference(Element element, string name) => ScriptProcessingInstruction.ContainsSelfDereference(element, name);
}
