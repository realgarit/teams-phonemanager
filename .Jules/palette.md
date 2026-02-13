## 2024-05-23 - Visual-Only Status Indicators
**Learning:** In GetStartedView, steps use `SymbolIcon` and `Border` visual changes to indicate "Pending" vs "Completed" status, but this state is not programmatically exposed to screen readers (no `aria-live` or `AutomationProperties.HelpText` updates).
**Action:** Future enhancements should ensure status changes modify the accessible name or description of the container, or use a status announcement mechanism.

## 2026-02-13 - Accessible Names for Icon-Only Elements
**Learning:** Icon-only buttons and informative images in this Avalonia project lacked programmatic names, making them inaccessible to screen readers. Relying only on visual icons or ToolTips (which are often not read by screen readers unless specifically configured) is insufficient for accessibility.
**Action:** Always use `AutomationProperties.Name` for icon-only buttons and provide descriptive names for informative images to ensure they are properly announced by screen readers.
