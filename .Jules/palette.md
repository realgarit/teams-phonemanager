## 2026-05-19 - Standardized Busy Overlay Pattern
**Learning:** In Avalonia, a consistent busy overlay requires a combination of a backdrop (dimmed background), z-indexing, and proper grid spanning to effectively prevent user interaction and provide clear feedback during async operations.
**Action:** Use a `Grid` with `ZIndex="2000"`, `Background="#40000000"`, and `Grid.RowSpan`/`Grid.ColumnSpan` covering the entire layout. Bind to `IsBusy` and a dedicated `WaitingMessage` property in the ViewModel.

## 2026-05-19 - ViewModel-Driven Progress Messages
**Learning:** When using a global `IsBusy` flag, it's often insufficient for providing specific context to the user. Adding a `WaitingMessage` property to the base ViewModel allows for operation-specific feedback while maintaining a standard UI pattern.
**Action:** Implement `OnIsBusyChanged` in the base ViewModel to automatically clear the `WaitingMessage` when the busy state ends, reducing boilerplate in subclasses.
