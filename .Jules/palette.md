## 2024-05-23 - Visual-Only Status Indicators
**Learning:** In GetStartedView, steps use `SymbolIcon` and `Border` visual changes to indicate "Pending" vs "Completed" status, but this state is not programmatically exposed to screen readers (no `aria-live` or `AutomationProperties.HelpText` updates).
**Action:** Future enhancements should ensure status changes modify the accessible name or description of the container, or use a status announcement mechanism.
