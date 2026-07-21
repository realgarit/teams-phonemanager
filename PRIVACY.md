# Privacy Policy — PhoneDesk for Microsoft Teams

**Last updated: 21 July 2026**

PhoneDesk is a desktop application that helps administrators manage Microsoft Teams telephony (call queues, auto attendants, holidays, and related configuration) in their own Microsoft 365 tenant.

## The short version

PhoneDesk does not collect, store, or transmit any of your data to the developer. There are no analytics, no telemetry, no tracking, and no developer-operated servers. All data the app handles stays between your device and Microsoft's own services.

## What the app processes, and where it goes

### Microsoft 365 sign-in and tenant data

To do its job, the app signs you in to your Microsoft 365 tenant using the Microsoft Authentication Library (MSAL). Authentication happens directly between your device and Microsoft's identity platform; the developer is never part of this exchange and never sees your credentials or tokens.

- Sign-in tokens are cached **locally on your device** (using the platform's protected storage where available) so you don't have to sign in every time.
- Teams telephony configuration (call queues, auto attendants, resource accounts, phone numbers, and similar) is read from and written to **Microsoft Graph and Microsoft Teams PowerShell endpoints in your own tenant only**.
- No tenant data ever leaves the direct connection between your device and Microsoft.

### Update check

On startup, the app makes a single anonymous request to the GitHub API (`api.github.com`) to see whether a newer release is available. This request contains no personal data or identifiers; like any web request, your IP address is visible to GitHub (see [GitHub's privacy statement](https://docs.github.com/site-policy/privacy-policies/github-privacy-statement)). If the request fails, the app simply continues without update information.

### Local logs

The app writes activity logs (for example, the results of operations you run) **locally on your device** to help you review and troubleshoot what it did. These logs are never transmitted anywhere.

## What the developer receives

Nothing. The developer operates no backend, receives no telemetry, and has no access to your tenant, your credentials, or your configuration data.

## Data retention and removal

All data the app stores (token cache, settings, logs) lives on your device. Uninstalling the application and deleting its local application-data folder removes it completely. You can also revoke the app's access to your tenant at any time via the Microsoft Entra admin center (Enterprise applications).

## Third-party services

Your use of Microsoft 365, Microsoft Graph, and Microsoft Teams is governed by your organization's agreements with Microsoft and the [Microsoft Privacy Statement](https://privacy.microsoft.com/privacystatement).

## Changes to this policy

Changes to this policy are published in this document in the app's public repository. The "Last updated" date above reflects the latest revision.

## Contact

Questions about this policy: open an issue at
<https://github.com/realgarit/phonedesk/issues>
