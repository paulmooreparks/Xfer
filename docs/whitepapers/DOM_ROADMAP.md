# XferLang DOM-like Features Roadmap

This document outlines the current DOM-like functionality in XferLang and suggests future enhancements.

## ✅ Implemented Features

### Element Navigation
- **Parent/Child Relationships**: Full parent-child relationship management
- **Sibling Navigation**: `NextSibling`, `PreviousSibling` properties
- **Child Access**: `FirstChild`, `LastChild` properties
- **Tree Traversal**: `GetSiblings()`, `GetAncestors()`, `GetDescendants()` methods

### Element Removal
- **Self Removal**: `Remove()` method with automatic parent cleanup
- **Child Removal**: `RemoveChild()`, `RemoveChildAt()`, `RemoveAllChildren()` methods
- **Element Replacement**: `ReplaceChild()` method

### Element Queries
- **ID-based Search**: `GetElementById()` (document), `FindElementById()` (element)
- **Tag-based Search**: `GetElementsByTag()` (document), `FindElementsByTag()` (element)
- **Type-based Search**: `GetElementsByType<T>()` and `GetElementsByType(Type)` methods

## 🚀 Future Enhancement Ideas

### 1. CSS-like Selector Language
**Priority**: High
**Description**: Define and implement a CSS-like selector syntax for XferLang
- Basic selectors: `#id`, `.tag`, `ElementType`
- Combinators: descendant (`A B`), child (`A > B`), sibling (`A + B`, `A ~ B`)
- Pseudo-selectors: `:first-child`, `:last-child`, `:nth-child(n)`
- Attribute selectors: `[id="value"]`, `[tag^="prefix"]`

**Implementation**:
```csharp
// Document-level
Element? QuerySelector(string selector)
IReadOnlyCollection<Element> QuerySelectorAll(string selector)

// Element-level
Element? QuerySelector(string selector)
IReadOnlyCollection<Element> QuerySelectorAll(string selector)
```

### 2. Element Insertion Methods
**Priority**: Medium
**Description**: DOM-like methods for inserting elements at specific positions

**Methods to implement**:
```csharp
// Insert at specific positions
void InsertBefore(Element newElement, Element referenceElement)
void InsertAfter(Element newElement, Element referenceElement)
void PrependChild(Element child)  // Insert as first child
void AppendChild(Element child)   // Insert as last child (alias for AddChild)

// Insert at index
void InsertChildAt(int index, Element child)
```

### 3. Element Cloning
**Priority**: Medium
**Description**: Create deep and shallow copies of elements

**Methods to implement**:
```csharp
Element Clone(bool deep = true)
Element CloneNode(bool deep = true)  // DOM-compatible alias
```

### 4. Element Validation and Schema Support
**Priority**: Medium
**Description**: Validate elements against XferLang schemas

**Methods to implement**:
```csharp
bool IsValid()
bool IsValid(XferSchema schema)
IReadOnlyCollection<ValidationError> GetValidationErrors()
IReadOnlyCollection<ValidationError> GetValidationErrors(XferSchema schema)
```

### 5. Advanced Tree Manipulation
**Priority**: Low
**Description**: More sophisticated tree operations

**Methods to implement**:
```csharp
// Move elements
bool MoveTo(Element newParent)
bool MoveTo(Element newParent, int index)

// Swap elements
bool SwapWith(Element other)

// Tree comparison
bool IsEqualNode(Element other)
bool IsSameNode(Element other)
```

### 6. Event System
**Priority**: Low
**Description**: DOM-like event system for element changes

**Events to implement**:
```csharp
event EventHandler<ElementChangedEventArgs> ChildAdded;
event EventHandler<ElementChangedEventArgs> ChildRemoved;
event EventHandler<ElementChangedEventArgs> AttributeChanged;
event EventHandler<ElementChangedEventArgs> ValueChanged;
```

### 7. Advanced Query Features
**Priority**: Low
**Description**: More sophisticated query capabilities

**Features to implement**:
- XPath-like expressions
- LINQ-style queries with extension methods
- Regular expression matching for text content
- Complex type hierarchies (e.g., "all numeric types")

### 8. Element Collections and Live NodeLists
**Priority**: Low
**Description**: Dynamic collections that update automatically

**Features to implement**:
- Live collections that reflect document changes
- Filtered views of element collections
- Observable collections with change notifications

## 📋 Implementation Notes

### Selector Language Syntax Proposal
When implementing CSS-like selectors, consider this syntax:

```
# Basic selectors
StringElement           # Type selector
#myId                  # ID selector
.myTag                 # Tag selector

# Combinators
ObjectElement StringElement     # Descendant (space)
ArrayElement > StringElement    # Direct child
.section + .footer             # Adjacent sibling
.item ~ .summary               # General sibling

# Pseudo-selectors
:first-child           # First child element
:last-child            # Last child element
:nth-child(2)          # Second child element
:nth-child(odd)        # Odd-numbered children
:nth-child(2n+1)       # Every second child starting from first

# Attribute selectors
[id]                   # Has ID attribute
[id="header"]          # ID equals "header"
[tag^="text"]          # Tag starts with "text"
[tag$="Element"]       # Tag ends with "Element"
[tag*="item"]          # Tag contains "item"
```

### Performance Considerations
- Index-based lookups for IDs and tags (already implemented in XferDocument)
- Lazy evaluation for complex selectors
- Caching of frequently-used queries
- Efficient tree traversal algorithms

### Compatibility Notes
- Maintain backward compatibility with existing APIs
- Follow DOM standards where applicable
- Use familiar method names from HTML/XML DOM
- Consider C# naming conventions and .NET patterns

## 🎯 Next Steps

1. **Complete Current Implementation**: Ensure all existing DOM methods work correctly
2. **Define Selector Language**: Create formal grammar for XferLang selectors
3. **Implement Basic Selectors**: Start with simple `QuerySelector`/`QuerySelectorAll`
4. **Add Insertion Methods**: Implement element insertion at specific positions
5. **Design Event System**: Plan change notification architecture

This roadmap provides a clear path for evolving XferLang into a full-featured document manipulation system while maintaining simplicity and performance.
