# Changelog

## [4.0.0](https://github.com/realgarit/teams-phonemanager/compare/v3.21.3...v4.0.0) (2026-07-13)


### ⚠ BREAKING CHANGES

* ViewModel constructors now require ISharedStateService and IDialogService parameters

### Features

* **#61:** fully async PowerShell execution with progress and cancellation ([77c3d91](https://github.com/realgarit/teams-phonemanager/commit/77c3d91df6de5200bc7d68dc4809ef65bbe016ca))
* **#62:** enable throttle retry on idempotent read paths; note bulk pacer status ([f3e3495](https://github.com/realgarit/teams-phonemanager/commit/f3e34953f70d9e64ad16302638f8538df9c1ff0f))
* **#62:** Graph/Teams throttling (429) retry with backoff and pacing ([4980260](https://github.com/realgarit/teams-phonemanager/commit/4980260159aa246d5e9cdd7246ce2677da6a396b))
* Add Auto Attendant configuration dialog with greeting and routing options ([8716614](https://github.com/realgarit/teams-phonemanager/commit/871661443917e8b735d1e888f02c2948cad7d660))
* add call queue view and functionality; patch: fix Handling userPrincipalName already exists error; fix Handling Cannot process argument transformation on parameter ConfigurationId error; fix Error Handling ([c7aa39f](https://github.com/realgarit/teams-phonemanager/commit/c7aa39fe679e4d732a08ae85baf76d285946b78b))
* add check powershell modules; fix initializing and loop while importing ([19de21e](https://github.com/realgarit/teams-phonemanager/commit/19de21ea9256fb9a3029a65dcbe214677b0876a9))
* add documentation, wizard, and bulk operations pages with script preview dialogs ([0406e3d](https://github.com/realgarit/teams-phonemanager/commit/0406e3dc3624b246eff7a1b6a312a46338bccc7b))
* add license to Resource Account; patch: fix to InvokeMgGraphReq… ([6d6271e](https://github.com/realgarit/teams-phonemanager/commit/6d6271e68206462c1835641bd2bacadd481b1c97))
* add license to Resource Account; patch: fix to InvokeMgGraphRequest due to changes from Microsoft; add message about waiting time ; fix ArgumentNullException ([c5c6360](https://github.com/realgarit/teams-phonemanager/commit/c5c636080da29a27d70b97a9e5212842f771138a))
* add license to Resource Account; patch: fix to InvokeMgGraphRequest due to changes from Microsoft; add message about waiting time; fix ArgumentNullException ([58ce130](https://github.com/realgarit/teams-phonemanager/commit/58ce13018a75e8be89ae0e4b0e169dbeb635b6f2))
* add loggingservice; add powershell service; add all pages; fix theme; fix text and UI elements; change sidebar; fix settings icon and bottom bar misplacement; design welcome page; improve logging with clipboard functionality ([84c6f7d](https://github.com/realgarit/teams-phonemanager/commit/84c6f7d1fe22aa9ed03055b53fd1daf8029331c0))
* add loggingservice; add powershell service; add all pages; fix theme; fix text and UI elements; change sidebar; fix settings icon and bottom bar misplacement; design welcome page; improve logging with clipboard functionality ([6a03620](https://github.com/realgarit/teams-phonemanager/commit/6a03620a77dd9e19de8d4c1096bbd46ee7f55443))
* add loggingservice; add powershell service; add all pages; fix theme; fix text and UI elements; change sidebar; fix settings icon and bottom bar misplacement; design welcome page; improve logging with clipboard functionality ([98da089](https://github.com/realgarit/teams-phonemanager/commit/98da08975fc97d758cf5ecc4262768a2c1e2eeb0))
* Add rich Call Queue configuration with greetings, music on hold, and exception handling ([e0a80e8](https://github.com/realgarit/teams-phonemanager/commit/e0a80e83a8092c5dd5f219e9c6daabea52da4de1))
* add variable savestate across views; add M365 page and check/create functionality; patch: improve UI and logging ([03151c6](https://github.com/realgarit/teams-phonemanager/commit/03151c67400227a52cdf13bc3b52089faccc7e12))
* **app:** typed OperationResult replacing string-sniffed PowerShell output ([#60](https://github.com/realgarit/teams-phonemanager/issues/60)) ([afaeec5](https://github.com/realgarit/teams-phonemanager/commit/afaeec53e702eac7e8e7293a5db5935f59026429))
* automate releases for in-app updates ([#91](https://github.com/realgarit/teams-phonemanager/issues/91)) ([dc5a6d3](https://github.com/realgarit/teams-phonemanager/commit/dc5a6d3d0d45bdd4dc20b51387ad350b22830dfd))
* **brand:** T-Pad monogram icon assets ([1bd4604](https://github.com/realgarit/teams-phonemanager/commit/1bd4604d0ffc438289dac9ead4d6df2c1d7a58fa))
* **brand:** T-Pad monogram logo + professional README ([3996734](https://github.com/realgarit/teams-phonemanager/commit/3996734ae263cf626f6870e55bc0ac457c469ee9))
* **brand:** update BrandGradientBrush to T-Pad palette ([64a122c](https://github.com/realgarit/teams-phonemanager/commit/64a122cae54b7d86f3bac8cd4b2c34066ead6de8))
* bundle PowerShell modules to eliminate admin rights and installation dependencies ([c030aa4](https://github.com/realgarit/teams-phonemanager/commit/c030aa4ef774612ac6726d32801160b246f4248a))
* bundle PowerShell modules to eliminate admin rights and installation dependencies ([4e0b849](https://github.com/realgarit/teams-phonemanager/commit/4e0b849807aaa9e26afe2e38b3332d79987ef6a5))
* end-to-end user deliverability (icon, macOS .app + Homebrew, Windows installer, self-contained builds, update check) ([3a18aaf](https://github.com/realgarit/teams-phonemanager/commit/3a18aafa81f9cc7e86fce1f9ad25adfe4c28d8e1))
* Enhance UI styles and layout for improved navigation and readability ([097fe54](https://github.com/realgarit/teams-phonemanager/commit/097fe54131cbfaa8a6a2fb13fba2589cd61b38bc))
* exact-match Auto Attendant by name in attach command ([dfe46af](https://github.com/realgarit/teams-phonemanager/commit/dfe46af3640ccfb281647fcbd23408db658f5b64))
* fully async PowerShell execution with progress and cancellation ([#61](https://github.com/realgarit/teams-phonemanager/issues/61)) ([69272b4](https://github.com/realgarit/teams-phonemanager/commit/69272b44df8dd22e950eaca6ab347d96d1ad0d6f))
* Graph/Teams throttling retry with backoff and pacing foundations ([#62](https://github.com/realgarit/teams-phonemanager/issues/62)) ([96b8ffb](https://github.com/realgarit/teams-phonemanager/commit/96b8ffb5c9bacd7f44a8f92b78bcdd0e8c29330b))
* Implement comprehensive Auto Attendant management functionality ([0a0dc2e](https://github.com/realgarit/teams-phonemanager/commit/0a0dc2e110aad09ca3b9207b547225f8112ce110))
* Implement comprehensive Auto Attendant management functionality ([a662344](https://github.com/realgarit/teams-phonemanager/commit/a6623440ed2412ac0fbd736c66dc7a016e780bc5))
* initial WPF app structure ([b555aed](https://github.com/realgarit/teams-phonemanager/commit/b555aedb6543686101c4632aeec043d5b3663a3b))
* Integrate MSAL for Microsoft Graph authentication, capture PowerShell warning streams, and simplify version bumping script. ([08549eb](https://github.com/realgarit/teams-phonemanager/commit/08549eb6138dea505d4b2586714cd0ad6f9d2d85))
* introduce example fields; distinct locked fields; update dark theme ([edcdceb](https://github.com/realgarit/teams-phonemanager/commit/edcdcebf419893e9ef4f54e49b808618fcdaef8f))
* introduce variables page; fix redundant connection; fix crashes with TimeSpan and DatePicker controls ([37ef6b0](https://github.com/realgarit/teams-phonemanager/commit/37ef6b0c3276731674b52615fe484027652b2854))
* **macos:** stable self-signed identity (tertius pattern); center logo; brand-consistent welcome hero ([da49717](https://github.com/realgarit/teams-phonemanager/commit/da497173b214cc731a4669fbad9f16e01135ce91))
* major codebase refactor applying KISS principle, centralized all hardcoded constants, fixed Microsoft Graph module imports, resolved PowerShell string interpolation issues, reorganized converters from Helpers/ to Converters/ folder, simplified NavigationService, removed redundant PowerShellService wrapper, updated module checking to verify all Graph sub-modules individually instead of meta module, fixed GetStartedViewModel validation logic for new module checks, ensured Graph authentication context persists in persistent runspace ([7a89a1a](https://github.com/realgarit/teams-phonemanager/commit/7a89a1a8f648816b60cc6d847ce3b5e8ddc80cf0))
* Major enhancements to M365 Groups and Call Queues functionality ([a6822c8](https://github.com/realgarit/teams-phonemanager/commit/a6822c80b26363fd37bb75d3639fb449c9e8754c))
* Major enhancements to M365 Groups and Call Queues functionality ([ed7b403](https://github.com/realgarit/teams-phonemanager/commit/ed7b403a1df24c00234845cd90b11c2cef8d7c75))
* major refactoring and modularization of codebase ([eef9979](https://github.com/realgarit/teams-phonemanager/commit/eef9979d2b3cc246f279bf653c909a5dd0a4196e))
* redesign GetStarted view to be more UX friendly ([06675e7](https://github.com/realgarit/teams-phonemanager/commit/06675e766c8ba3588ee95f15ae6b7058631a84ea))
* redesign UI with custom theme, typography, and filled icon system (v3.15.0) ([75e4d09](https://github.com/realgarit/teams-phonemanager/commit/75e4d095daacfb9951236b47683c8a61966b5fa0))
* refactor log viewer to overlay dialog with improved UX and multi-line selection ([47e14b8](https://github.com/realgarit/teams-phonemanager/commit/47e14b86b1bce72c0e094903d9e357dc66a447a3))
* reorganize Variables page into tabs, refactor time selection ComboBox to MVVM ([1412e42](https://github.com/realgarit/teams-phonemanager/commit/1412e429c68d2c06c19e131154ea6321b3a46f67))
* resolve agent names in documentation export and simplify welcome page ([d31d2e6](https://github.com/realgarit/teams-phonemanager/commit/d31d2e6c990eafa2ccc44670600de5e12d07dd1c))
* security hardening, sidebar sync fix, and UI polish (v3.13.0) ([52ea20b](https://github.com/realgarit/teams-phonemanager/commit/52ea20bc6832bb2d81e2dd111d8887026238fc8e))
* typed OperationResult replacing string-sniffed PowerShell output ([#60](https://github.com/realgarit/teams-phonemanager/issues/60)) ([4076300](https://github.com/realgarit/teams-phonemanager/commit/40763001d266aa25d797a47ca247e90c6f61e37c))
* **ux:** add accessible labels and tooltips to list views ([cf6e59c](https://github.com/realgarit/teams-phonemanager/commit/cf6e59c85bc1a3b92c92d2cbf44ef9d82c4bc637))
* **ux:** add accessible labels and tooltips to list views ([f36c68e](https://github.com/realgarit/teams-phonemanager/commit/f36c68e00fc63f2dbe91ebd770467254119e63a8))


### Bug Fixes

* Add debug logging when dialog window is unavailable ([87a1e09](https://github.com/realgarit/teams-phonemanager/commit/87a1e091347ccc182696a836fb35211c4b810b99))
* Add debug logging when dialog window is unavailable ([bfff485](https://github.com/realgarit/teams-phonemanager/commit/bfff48513c6c803676a7d47ade283a3f4f1e8aa6))
* Add missing input sanitization to remaining script builder methods ([06551d7](https://github.com/realgarit/teams-phonemanager/commit/06551d77e4f063aef7c60619125c3a7af2d8a37f))
* add missing license step in CQ script and propagation waits (v3.14.0) ([55c0805](https://github.com/realgarit/teams-phonemanager/commit/55c080589be0a720120613a131cb9201753f00d5))
* add UseDefaultMusicOnHold parameter for default music on hold, add required SharedVoicemail greeting prompts, and fix UI alignment in configuration dialogs ([426f025](https://github.com/realgarit/teams-phonemanager/commit/426f025319787e1ca88b1cb10142b8890ec10a8d))
* add windows publish script ([a789432](https://github.com/realgarit/teams-phonemanager/commit/a789432395a5aeb6ec20df7c57d265e1ee3e7128))
* address security vulnerabilities, architecture issues, and test coverage gaps ([7238513](https://github.com/realgarit/teams-phonemanager/commit/72385136e539654c454aa9b3c0ecf7f8eb84ec24))
* address security vulnerabilities, architecture issues, and test coverage gaps ([1de5de2](https://github.com/realgarit/teams-phonemanager/commit/1de5de235d5d3d43d32725db21a972b7ecdf43ef))
* anchor release-please to v3.21.1 to prevent phantom major bumps ([#105](https://github.com/realgarit/teams-phonemanager/issues/105)) ([e1be303](https://github.com/realgarit/teams-phonemanager/commit/e1be303492176e85231173d4c0dd1ffafbb4eabc))
* **assets:** center banner.svg content within its canvas ([0440e11](https://github.com/realgarit/teams-phonemanager/commit/0440e116532cb9e5f535438703b60cd265a8c6c7))
* attach holiday prefill and PowerShell AA CallFlows update ([30fd622](https://github.com/realgarit/teams-phonemanager/commit/30fd6220ebd20e53bd4ef331044ef59316d6d1a9))
* **brand:** unify sidebar logo to T-Pad mark; center README banner ([764deee](https://github.com/realgarit/teams-phonemanager/commit/764deee59f25c642b17e082c404e0ca44a4f07ad))
* build draft releases from commit ([#96](https://github.com/realgarit/teams-phonemanager/issues/96)) ([a0fe34d](https://github.com/realgarit/teams-phonemanager/commit/a0fe34dde18b6eee6431902c75f896219fe94ac4))
* Critical security and bug fixes ([fc2007b](https://github.com/realgarit/teams-phonemanager/commit/fc2007b4cb92fcb26b3e917a681d75a39e4eefa4))
* Critical security and bug fixes ([53b865e](https://github.com/realgarit/teams-phonemanager/commit/53b865e7ea8065820ab087e32a2cae69aca24938))
* Disable WAM for Graph authentication to resolve 'window handle' error ([ea2d04a](https://github.com/realgarit/teams-phonemanager/commit/ea2d04a9884b1a5c39f68a8b2463b313a91d3db4))
* escape braces for exact-match AA filtering in PS string ([c6554de](https://github.com/realgarit/teams-phonemanager/commit/c6554de5693a5d76be30e678ed7235c5e96a9bbe))
* High severity improvements - async, architecture, memory leak, security ([282853a](https://github.com/realgarit/teams-phonemanager/commit/282853ad50d8ffe42577b0ed5ad0c656ab29b59b))
* High severity improvements - async, architecture, memory leak, security ([b9d94c8](https://github.com/realgarit/teams-phonemanager/commit/b9d94c8c027e64e999916acca3090f41b671014f))
* implement ExecuteCommandWithDetailsAsync on ScreenshotGenerator's stub PowerShell service ([f8ae992](https://github.com/realgarit/teams-phonemanager/commit/f8ae992b39e00778233f0be080adb4e0cb34eb62))
* make release PR creation self-contained ([#92](https://github.com/realgarit/teams-phonemanager/issues/92)) ([f2c548b](https://github.com/realgarit/teams-phonemanager/commit/f2c548bbb67c52ed1905d5b8515e478ec4dceb65))
* **release:** publish Release Please drafts in place ([#99](https://github.com/realgarit/teams-phonemanager/issues/99)) ([a8f26e5](https://github.com/realgarit/teams-phonemanager/commit/a8f26e5b32f2e7cd44fcbbc8045052e994b08f58))
* **release:** strip quarantine in cask postflight; only release on newly created tags ([10fb87c](https://github.com/realgarit/teams-phonemanager/commit/10fb87c0cd0a0c3037e179b17d7fb6b2fb81d27b))
* reliable window handle error fix by setting WAM disabling flags at process startup ([908b44b](https://github.com/realgarit/teams-phonemanager/commit/908b44bb5588d0f952a83480471e6a85356b7ccc))
* Remove redundant git config commands for user name and email in tag creation step ([78400ea](https://github.com/realgarit/teams-phonemanager/commit/78400ea3e0dd2a1471047ef97ed6cfc9ead3e9c8))
* restore execution policy via InitialSessionState to fix module loading (v3.14.1) ([dc21190](https://github.com/realgarit/teams-phonemanager/commit/dc211905a8a35fd75edf90224a6788e18185c681))
* Set PowerShell ExecutionPolicy to Bypass to allow loading bundled modules ([9da74e4](https://github.com/realgarit/teams-phonemanager/commit/9da74e47a880f93959f9b80c90f952a88ac69af3))
* **sidebar:** use canonical T-Pad app icon for sidebar brand mark ([f1dec11](https://github.com/realgarit/teams-phonemanager/commit/f1dec11250894eb04bc580a3ce801e240449038c))
* sync release-please version across all files and fix UI spacing ([780d8dd](https://github.com/realgarit/teams-phonemanager/commit/780d8dd6294fc552e22b8c3f19648f1b4bd796f4))
* sync release-please version across all files and fix UI spacing ([#102](https://github.com/realgarit/teams-phonemanager/issues/102)) ([45125d0](https://github.com/realgarit/teams-phonemanager/commit/45125d04961e0a7e25f66b7a591bb2be4392f340))
* target repository for release validation ([#94](https://github.com/realgarit/teams-phonemanager/issues/94)) ([aa64d6b](https://github.com/realgarit/teams-phonemanager/commit/aa64d6b197357dfe3739d86be254caf12711246e))
* UI alignment issues for Aargau info button and holiday series icons ([f37ac6a](https://github.com/realgarit/teams-phonemanager/commit/f37ac6af022977e659965c16ea46d5faeb5a0938))
* update app manifest version ([b9eb334](https://github.com/realgarit/teams-phonemanager/commit/b9eb33438879c6a016469846d7282c92de0ad164))
* update app manifest version ([6eb3270](https://github.com/realgarit/teams-phonemanager/commit/6eb327024f0157273d31dc4661fc034b4678dd79))
* Update copyright year, correct app.manifest XML version, and refine version bumping script regex. ([1eb03e0](https://github.com/realgarit/teams-phonemanager/commit/1eb03e0eb9aba644b5fd5b5c810676745b603e3f))
* update PowerShell execution policy to RemoteSigned ([d02d2ab](https://github.com/realgarit/teams-phonemanager/commit/d02d2ab157f9a6ced6fed8a84473403edc36006f))
* update to 3.6.1, upgrade `Microsoft.Identity.Client`, and remove old publish scripts. ([badefda](https://github.com/realgarit/teams-phonemanager/commit/badefda928934af94f566c0b79df8bc5afa5134f))
* Update UI version display to 1.14.0 ([a90f9fb](https://github.com/realgarit/teams-phonemanager/commit/a90f9fbf3d6f7eead9785c0b86fbc3b8ae321a71))
* Update UI version display to 1.14.0 ([bf567f4](https://github.com/realgarit/teams-phonemanager/commit/bf567f4d358c84dc20901fc96502206c31a3beea))
* updated project configuration, created publishiing script, updat… ([1152f02](https://github.com/realgarit/teams-phonemanager/commit/1152f026fad5504dd8365df9a8c501c1e9f17c35))
* updated project configuration, created publishiing script, updated documentation ([a861160](https://github.com/realgarit/teams-phonemanager/commit/a8611607e05a20213e93e4c8167bcddd9f556705))
* various popups and buttons, start fixing Variables page ([684cd84](https://github.com/realgarit/teams-phonemanager/commit/684cd849b2f0a6410f225bcb9b4e7675a6bba8d4))


### Reverts

* restore prior AA attach method using NameFilter ([61033e0](https://github.com/realgarit/teams-phonemanager/commit/61033e06e35535fff1c323fe0b81f74a5e3a7939))

## [3.21.3](https://github.com/realgarit/teams-phonemanager/compare/v3.21.2...v3.21.3) (2026-07-13)


### Bug Fixes

* anchor release-please to v3.21.1 to prevent phantom major bumps ([#105](https://github.com/realgarit/teams-phonemanager/issues/105)) ([e1be303](https://github.com/realgarit/teams-phonemanager/commit/e1be303492176e85231173d4c0dd1ffafbb4eabc))

## [3.21.2](https://github.com/realgarit/teams-phonemanager/compare/v3.21.1...v3.21.2) (2026-07-13)


### Bug Fixes

* sync release-please version across all files and fix UI spacing ([780d8dd](https://github.com/realgarit/teams-phonemanager/commit/780d8dd6294fc552e22b8c3f19648f1b4bd796f4))
* sync release-please version across all files and fix UI spacing ([#102](https://github.com/realgarit/teams-phonemanager/issues/102)) ([45125d0](https://github.com/realgarit/teams-phonemanager/commit/45125d04961e0a7e25f66b7a591bb2be4392f340))

## [3.21.1](https://github.com/realgarit/teams-phonemanager/compare/v3.21.0...v3.21.1) (2026-07-13)


### Bug Fixes

* **release:** publish Release Please drafts in place ([#99](https://github.com/realgarit/teams-phonemanager/issues/99)) ([a8f26e5](https://github.com/realgarit/teams-phonemanager/commit/a8f26e5b32f2e7cd44fcbbc8045052e994b08f58))

## [3.21.0](https://github.com/realgarit/teams-phonemanager/compare/v3.20.1...v3.21.0) (2026-07-13)


### Features

* automate releases for in-app updates ([#91](https://github.com/realgarit/teams-phonemanager/issues/91)) ([dc5a6d3](https://github.com/realgarit/teams-phonemanager/commit/dc5a6d3d0d45bdd4dc20b51387ad350b22830dfd))


### Bug Fixes

* build draft releases from commit ([#96](https://github.com/realgarit/teams-phonemanager/issues/96)) ([a0fe34d](https://github.com/realgarit/teams-phonemanager/commit/a0fe34dde18b6eee6431902c75f896219fe94ac4))
* make release PR creation self-contained ([#92](https://github.com/realgarit/teams-phonemanager/issues/92)) ([f2c548b](https://github.com/realgarit/teams-phonemanager/commit/f2c548bbb67c52ed1905d5b8515e478ec4dceb65))
* target repository for release validation ([#94](https://github.com/realgarit/teams-phonemanager/issues/94)) ([aa64d6b](https://github.com/realgarit/teams-phonemanager/commit/aa64d6b197357dfe3739d86be254caf12711246e))

## [3.20.1](https://github.com/realgarit/teams-phonemanager/compare/v3.20.0...v3.20.1) (2026-07-13)


### Bug Fixes

* build draft releases from commit ([#96](https://github.com/realgarit/teams-phonemanager/issues/96)) ([a0fe34d](https://github.com/realgarit/teams-phonemanager/commit/a0fe34dde18b6eee6431902c75f896219fe94ac4))

## [3.20.0](https://github.com/realgarit/teams-phonemanager/compare/v3.19.0...v3.20.0) (2026-07-13)


### Features

* automate releases for in-app updates ([#91](https://github.com/realgarit/teams-phonemanager/issues/91)) ([dc5a6d3](https://github.com/realgarit/teams-phonemanager/commit/dc5a6d3d0d45bdd4dc20b51387ad350b22830dfd))


### Bug Fixes

* make release PR creation self-contained ([#92](https://github.com/realgarit/teams-phonemanager/issues/92)) ([f2c548b](https://github.com/realgarit/teams-phonemanager/commit/f2c548bbb67c52ed1905d5b8515e478ec4dceb65))
* target repository for release validation ([#94](https://github.com/realgarit/teams-phonemanager/issues/94)) ([aa64d6b](https://github.com/realgarit/teams-phonemanager/commit/aa64d6b197357dfe3739d86be254caf12711246e))
