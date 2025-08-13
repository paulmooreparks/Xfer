# Change Log

All notable changes to the "XferLang" extension will be documented in this file.

## [Unreleased]

## [0.15.4] - 2025-08-13

### Fixed

-  Interpolated strings now only treat `<...>` as embedded Xfer elements when the `<` is followed by a valid explicit-element starter (quotes, numeric specifiers, dynamic `|`, deref `_`, date `@`, collection openers, or `=`). This prevents false positives from `<` and `'` inside embedded code blocks like `<' JS code '>`.

## [0.15.3] - 2025-08-12

- Improved syntax highlighting

## [0.15.2] - 2025-08-11

- Improved syntax highlighting

## [0.15.1] - 2025-08-11

- Improved syntax highlighting

## [0.15.0] - 2025-08-11

- Initial public release
