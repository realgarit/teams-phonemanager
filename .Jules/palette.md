## 2024-05-23 - Visual-Only Status Indicators
**Learning:** In GetStartedView, steps use `SymbolIcon` and `Border` visual changes to indicate "Pending" vs "Completed" status, but this state is not programmatically exposed to screen readers (no `aria-live` or `AutomationProperties.HelpText` updates).
**Action:** Future enhancements should ensure status changes modify the accessible name or description of the container, or use a status announcement mechanism.

## 2026-02-13 - Accessible Names for Icon-Only Elements
**Learning:** Icon-only buttons and informative images in this Avalonia project lacked programmatic names, making them inaccessible to screen readers. Relying only on visual icons or ToolTips (which are often not read by screen readers unless specifically configured) is insufficient for accessibility.
**Action:** Always use `AutomationProperties.Name` for icon-only buttons and provide descriptive names for informative images to ensure they are properly announced by screen readers.

## 2026-02-13 - Dynamic Status Announcements
**Learning:** For setup/process-driven views like GetStartedView, visual icons alone are insufficient for screen readers. Using `AutomationProperties.LiveSetting="Polite"` combined with descriptive status properties in the ViewModel allows asynchronous updates (like command completion) to be automatically announced.
**Action:** When implementing multi-step processes or long-running checks, bind `AutomationProperties.Name` to a descriptive status string and set `LiveSetting="Polite"` on the status indicator container.

## 2026-02-14 - Contextual Clarity with Watermarks
**Learning:** Input fields that rely on external example text (like "Example: Acme Corp") rather than explicit labels are difficult for screen readers and confusing for users. Adding a `Watermark` provides immediate context and often serves as the fallback accessible name.
**Action:** Use `Watermark` in `TextBox` controls to clearly identify the field's purpose, especially when layout constraints prevent adding standard labels.
