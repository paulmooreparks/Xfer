---
title: XferLang Documentation
description: Documentation for the XferLang data-interchange format
---

# XferLang Documentation

## Introduction

XferLang is a data-interchange format designed to support data serialization, data transmission, and offline use cases such as configuration management.

```xfer
{
    name "Alice"
    age 30
    isActive ~true
}
```

## Design Philosophy

XferLang is built around four core principles:

### 1. Clarity and Readability
The syntax is designed to be human-readable without requiring separators like commas.

### 2. Explicit Typing
All values are explicitly typed using prefixes.

## Basic Syntax

XferLang documents consist of elements separated by whitespace.

```xfer
</ This is a comment />
name "John Doe"
age 30
```
