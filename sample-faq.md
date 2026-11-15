# Northstar Notes — Product Knowledge Base

This document describes the plans, limits, security, and support policies for Northstar Notes. Upload this file and use the questions at the end to test retrieval accuracy.

## 1. Plans and Pricing

Northstar Notes has four plans. Starter is free and supports up to 3 users and 1 GB of storage. Team costs $12 per user per month and includes 100 GB of storage. Business costs $24 per user per month and includes 1 TB of storage, SAML single sign-on, and audit logs. Enterprise pricing is custom and includes a dedicated success manager and customer-managed encryption keys. Annual billing saves 20 percent compared with monthly billing.

## 2. Free Trial

Every new workspace can start a 14-day free trial of the Team or Business plan. No credit card is required. When the trial ends, the workspace automatically moves to the free Starter plan. All content is retained, but storage and features above the Starter limits become read-only until a paid plan is selected.

## 3. Storage and File Limits

Storage is shared across the whole workspace. Starter includes 1 GB, Team includes 100 GB, and Business includes 1 TB. A single uploaded file may be at most 250 MB. Files larger than 250 MB must be split before upload. Storage usage is recalculated every 15 minutes.

## 4. Supported File Types

Northstar Notes accepts Markdown, plain text, PDF, DOCX, PNG, JPG, and SVG files. ZIP archives, RAR archives, executables, and password-protected PDFs are rejected. Uploaded files are scanned for malware before they become available to workspace members.

## 5. Trash and Data Retention

Deleting a note moves it to Trash. Notes remain in Trash for 30 days and can be restored by an Admin or the note owner during that period. After 30 days, the note is permanently deleted. Encrypted backups are retained for 35 days and then rotated out. Deleted content may remain in backups until the next backup rotation completes.

## 6. Real-Time Collaboration

The number of people who can edit the same note at the same time depends on the plan: 3 on Starter, 10 on Team, and 50 on Business. Enterprise workspaces can request a higher limit. Active editors see live cursors and selections. People viewing a note or leaving comments do not count toward the concurrent editor limit.

## 7. Offline Mode and Version History

The desktop and mobile apps cache recently opened notes for 30 days, so they remain available offline. Offline edits are queued and synchronized when the device reconnects. If two people edit the same block while offline, the last writer wins for that block. Version history is kept for 30 days on Starter, 90 days on Team and Business, and one year on Enterprise. Automatic snapshots are created every 10 minutes while a note is being edited.

## 8. Search

Full-text search is available on every plan. For workspaces with fewer than 100,000 notes, search results are returned with a p95 latency of 200 milliseconds. Semantic search is available only on Business and Enterprise. Regular expressions and wildcard queries are not supported. Search results always respect the requesting member's permissions.

## 9. API and Rate Limits

The REST API is rate limited to 600 requests per minute per workspace, with a burst allowance of 100 requests in any 10-second window. When a limit is exceeded, the API returns HTTP 429 with a Retry-After header containing the number of seconds to wait. The notes endpoint is /v1/notes. API keys are scoped as read-only or read-write, and keys should be rotated every 90 days. The settings page shows when each key was last used.

## 10. Webhooks

Each workspace can configure up to 20 webhook endpoints. Failed deliveries are retried 5 times using exponential backoff after 1, 2, 4, 8, and 16 minutes. Every request includes an HMAC SHA-256 signature in the X-Northstar-Signature header. Supported events are note.created, note.updated, note.deleted, comment.created, and member.invited.

## 11. Security and Encryption

Data is encrypted with TLS 1.3 in transit and AES-256 at rest. Northstar Notes is SOC 2 Type II certified and completes an independent penetration test every year. Customer-managed encryption keys are available only on the Enterprise plan. Workspace admins can require multi-factor authentication for all members on Business and Enterprise.

## 12. Single Sign-On and Provisioning

Business and Enterprise support SAML 2.0 and OpenID Connect single sign-on. SCIM 2.0 user provisioning is available only on Enterprise. Just-in-time provisioning is supported for both SAML and OIDC. Identity-provider initiated logout is supported on Enterprise.

## 13. Roles and Permissions

Workspace roles are Owner, Admin, Editor, Commenter, and Viewer. Owners control billing, plan changes, and workspace deletion. Admins manage members, groups, and workspace settings but cannot change billing. Guests can only be Commenter or Viewer. A workspace must always have at least one Owner, so the last Owner cannot be removed or demoted. Ownership can be transferred only by the current Owner.

## 14. Data Regions

Workspaces can be created in the US East (Virginia), EU (Frankfurt), or APAC (Singapore) region. The region is selected at workspace creation and cannot be changed later. To move to another region, create a new workspace in the target region and migrate content manually. Primary data and backups remain in the selected region.

## 15. Support and Service Level

Business includes a 99.9 percent uptime commitment and 24/5 support from Monday to Friday. Enterprise includes a 99.95 percent uptime commitment and 24/7 support. For Enterprise, the first response target is 1 hour for P1 incidents and 8 hours for P2 incidents. For Business, the targets are 4 hours for P1 and one business day for P2. Current platform status is published on the status page.

## Test Questions

1. How much does the Team plan cost per user per month, and how much storage does it include?
2. What happens to a workspace when the 14-day free trial ends?
3. What is the maximum size of a single uploaded file?
4. Can I upload a ZIP archive or an executable?
5. How long are deleted notes kept in Trash, and how long are backups retained?
6. How many people can edit the same note simultaneously on the Business plan?
7. How long is version history retained on Team, and how are offline edit conflicts resolved?
8. Is semantic search available, and which plans include it?
9. What is the API rate limit, and what should I do after receiving a 429 response?
10. How many webhook endpoints can a workspace configure, and how many retries occur after a failed delivery?
11. What encryption is used in transit and at rest, and which plan offers customer-managed keys?
12. Which single sign-on protocols are supported, and which plan includes SCIM provisioning?
13. Who can transfer workspace ownership, and can the last Owner be removed?
14. Can a workspace change its data region after creation?
15. What are the Enterprise first-response targets for P1 and P2 incidents?
