using System;
using System.Collections.Generic;
using System.Linq;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace IfPiDemo;

/// <summary>
/// Post-processing utility to remove elements that failed their conditional checks.
/// This demonstrates how to achieve true conditional behavior by filtering the document
/// after parsing, since the current PI architecture doesn't support conditional creation.
/// </summary>
public static class ConditionalDocumentProcessor {

    /// <summary>
    /// Removes all elements marked as "conditional-false" from the document.
    /// This provides true conditional behavior by post-processing the parsed document.
    /// </summary>
    /// <param name="doc">The document to process</param>
    /// <returns>The number of elements removed</returns>
    public static int RemoveFailedConditionals(XferDocument doc) {
        int removedCount = 0;

        if (doc.Root != null) {
            removedCount += RemoveFailedConditionalsFromElement(doc.Root);
        }

        return removedCount;
    }

    /// <summary>
    /// Recursively removes failed conditional elements from a container element.
    /// </summary>
    /// <param name="element">The container element to process</param>
    /// <returns>The number of elements removed</returns>
    private static int RemoveFailedConditionalsFromElement(Element element) {
        int removedCount = 0;

        switch (element) {
            case TupleElement tuple:
                removedCount += ProcessTupleElement(tuple);
                break;
            case ArrayElement array:
                removedCount += ProcessArrayElement(array);
                break;
            case ObjectElement obj:
                removedCount += ProcessObjectElement(obj);
                break;
        }

        // Recursively process remaining children
        foreach (var child in element.Children.ToList()) {
            removedCount += RemoveFailedConditionalsFromElement(child);
        }

        return removedCount;
    }

    private static int ProcessTupleElement(TupleElement tuple) {
        int removedCount = 0;
        var elementsToRemove = new List<Element>();

        for (int i = 0; i < tuple.Count; i++) {
            var element = tuple.GetElementAt(i);
            if (element != null && IsFailedConditional(element)) {
                elementsToRemove.Add(element);
            }
        }

        foreach (var element in elementsToRemove) {
            if (RemoveFromTuple(tuple, element)) {
                removedCount++;
            }
        }

        return removedCount;
    }

    private static int ProcessArrayElement(ArrayElement array) {
        int removedCount = 0;
        var elementsToRemove = new List<Element>();

        for (int i = 0; i < array.Count; i++) {
            var element = array.GetElementAt(i);
            if (element != null && IsFailedConditional(element)) {
                elementsToRemove.Add(element);
            }
        }

        foreach (var element in elementsToRemove) {
            if (RemoveFromArray(array, element)) {
                removedCount++;
            }
        }

        return removedCount;
    }

    private static int ProcessObjectElement(ObjectElement obj) {
        int removedCount = 0;
        var keysToRemove = new List<string>();

        foreach (var kvp in obj.Dictionary) {
            if (IsFailedConditional(kvp.Value.Value)) {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove) {
            if (obj.Remove(key)) {
                removedCount++;
            }
        }

        return removedCount;
    }

    /// <summary>
    /// Checks if an element has been marked as a failed conditional.
    /// </summary>
    private static bool IsFailedConditional(Element element) {
        return element.Id?.Contains("conditional-false") == true;
    }

    /// <summary>
    /// Attempts to remove an element from a tuple by recreating the tuple without the element.
    /// This is necessary because TupleElement doesn't have a direct Remove method.
    /// </summary>
    private static bool RemoveFromTuple(TupleElement tuple, Element elementToRemove) {
        // This is a limitation - we'd need to rebuild the tuple or extend the API
        // For now, we'll mark it as processed but can't actually remove it
        return false;
    }

    /// <summary>
    /// Attempts to remove an element from an array by finding and removing it.
    /// This is challenging because ArrayElement enforces homogeneous typing.
    /// </summary>
    private static bool RemoveFromArray(ArrayElement array, Element elementToRemove) {
        // This is a limitation - ArrayElement doesn't have a direct Remove method
        // For now, we'll mark it as processed but can't actually remove it
        return false;
    }

    /// <summary>
    /// Creates a summary of conditional processing results.
    /// </summary>
    public static ConditionalProcessingSummary AnalyzeDocument(XferDocument doc) {
        var summary = new ConditionalProcessingSummary();

        if (doc.Root != null) {
            AnalyzeElement(doc.Root, summary);
        }

        return summary;
    }

    private static void AnalyzeElement(Element element, ConditionalProcessingSummary summary) {
        // Check if this element is conditional
        if (element.Id?.Contains("conditional-false") == true) {
            summary.FailedConditionals++;
        } else if (element.Id?.Contains("conditional") == true) {
            summary.SuccessfulConditionals++;
        }

        // Recursively analyze children
        foreach (var child in element.Children) {
            AnalyzeElement(child, summary);
        }
    }
}

/// <summary>
/// Summary of conditional processing results.
/// </summary>
public class ConditionalProcessingSummary {
    public int SuccessfulConditionals { get; set; } = 0;
    public int FailedConditionals { get; set; } = 0;
    public int TotalConditionals => SuccessfulConditionals + FailedConditionals;
}
