# Enterprise GPT — Functional Requirements v2

## Overview

Build a static HTML mockup (single file per page) for **"Enterprise GPT,"** an enterprise-grade LLM chat wrapper application. The design language should closely follow the modern, minimal aesthetic of **ChatGPT (2024–2025)** and **Claude.ai** — characterized by generous whitespace, subtle borders, muted color usage, clean typography, and a calm, professional tone throughout the UI.

The application consists of **four self-contained HTML files**, each with all CSS and JavaScript inline.

---

## Design Philosophy

The current enterprise palette (#355872, #7AAACE, #9CD5FF, #F7F8F0) should be used **sparingly as accents** — not as dominant surface colors. The overall look should be **neutral, quiet, and spacious**, using the brand colors only for interactive highlights, selected states, and key moments of emphasis.

### Key Aesthetic Principles

| Principle | Description |
|---|---|
| **Neutral surfaces** | Light mode uses white (`#FFFFFF`) and very light grays (`#F9FAFB`, `#F3F4F6`) for backgrounds — not the off-white `#F7F8F0` as a primary surface. Dark mode uses near-black (`#1A1A1A`, `#212121`) and dark grays (`#2A2A2A`, `#333333`) — not inverted brand blues. |
| **Minimal color** | Brand blues appear only on: the active nav indicator, primary action buttons, selected conversation highlight, unread badges, and focused input borders. Everything else is grayscale. |
| **Typography-first** | Use a clean, modern sans-serif system font stack: `-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif`. Body text at 14–15px, generous line-height (1.5–1.6). Headings are semibold, not heavy. |
| **Breathing room** | Generous padding inside containers (24–32px). Conversation messages have comfortable vertical spacing (16–24px between messages). Sidebar items have 10–12px vertical padding. Nothing should feel cramped. |
| **Subtle separation** | Use `1px solid` borders in very light gray (`#E5E7EB` light / `#333` dark) to separate regions — not colored borders or heavy shadows. Avoid box-shadows on cards unless absolutely necessary; prefer flat or single-pixel borders. |
| **Quiet interactions** | Hover states use subtle background shifts (e.g., `rgba(0,0,0,0.04)` in light mode). No bright color changes on hover. Transitions at 150ms ease. |

---

## Global Constraints

### Technical

- **CSS framework:** Bootstrap 5.3 (CDN) + Bootstrap Icons (CDN)
- **Custom styles:** Prefer Bootstrap utility classes. Custom CSS should focus on overriding Bootstrap defaults to achieve the modern neutral aesthetic described above — especially toning down Bootstrap's default blues and heavy borders.
- **JavaScript:** Vanilla JS only — no frameworks
- **Icons:** Bootstrap Icons only
- **Deliverable:** Four self-contained files: `index.html`, `conversations.html`, `notifications.html`, `administrator.html`
- **All interactions:** Purely client-side — no backend, no API calls

### UI Behavior Standards

- **Modals:** All modals use Bootstrap's native modal component — no `prompt()` or `confirm()` dialogs
- **Rename actions:** Inline editable text field (click title to edit in-place) — not a modal for simple renames
- **Delete confirmations:** Bootstrap modal with clear destructive action styling (red delete button)
- **Toast notifications:** Bootstrap Toasts on every CRUD action — positioned **bottom-center** (not bottom-right), minimal styling, auto-dismiss after 3 seconds. Toast should have a thin left accent border matching the action type (green for success, red for error).
- **Persistence:** Dark/light mode toggle and sidebar collapse state persist via `localStorage`

---

## Color System

### Brand Palette (accent use only)

| Token | Hex | Usage |
|---|---|---|
| `--brand-primary` | `#355872` | Primary buttons, active nav indicator, selected conversation bg tint |
| `--brand-secondary` | `#7AAACE` | Secondary accents, links, unread badges, focus rings |
| `--brand-light` | `#9CD5FF` | Subtle highlights, tag backgrounds (light mode only) |
| `--brand-surface` | `#F7F8F0` | Not used as a page background — reserve for special callout cards if needed |

### Neutral Palette

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `--bg-primary` | `#FFFFFF` | `#1A1A1A` | Page background, chat area |
| `--bg-secondary` | `#F9FAFB` | `#212121` | Sidebar background, card backgrounds |
| `--bg-tertiary` | `#F3F4F6` | `#2A2A2A` | Input fields, hover states, table header |
| `--bg-hover` | `rgba(0,0,0,0.04)` | `rgba(255,255,255,0.06)` | Hover state on list items, buttons |
| `--border-default` | `#E5E7EB` | `#333333` | All borders, dividers |
| `--border-strong` | `#D1D5DB` | `#444444` | Focused input borders (before brand-color focus) |
| `--text-primary` | `#111827` | `#E5E7EB` | Headings, body text |
| `--text-secondary` | `#6B7280` | `#9CA3AF` | Timestamps, labels, helper text |
| `--text-tertiary` | `#9CA3AF` | `#6B7280` | Placeholders, disabled text |

### Semantic Colors

| Token | Value | Usage |
|---|---|---|
| `--success` | `#16A34A` | Success badges, enabled status |
| `--error` | `#DC2626` | Error badges, delete buttons, suspended status |
| `--warning` | `#D97706` | Warning badges |
| `--info` | `#7AAACE` | Info badges (reuse brand secondary) |

---

## Shared Sidebar (all four pages)

The sidebar should feel like **Claude.ai's sidebar** — clean, understated, functional.

### Structure

- **Width:** 260px when open. Collapsible to 0px (fully hidden, not icon-only).
- **Background:** `--bg-secondary` with a `1px` right border in `--border-default`
- **Toggle button:** Small icon button (`bi-layout-sidebar`) pinned to the top-left of the main content area when sidebar is closed. Inside the sidebar header when open. No heavy styling — just an icon button with hover state.

### Content (top to bottom)

1. **New Conversation button**
   - Full-width, outlined style (not filled) — `1px solid --border-default`, text in `--text-primary`
   - Icon: `bi-plus-lg` on the left
   - On hover: subtle background fill (`--bg-hover`)
   - Matches ChatGPT's "+ New chat" button aesthetic

2. **Navigation links**
   - Simple text links with left-padding, small icon on left
   - Links: Chat (`bi-chat-dots`), Conversations (`bi-clock-history`), Notifications (`bi-bell`), Admin (`bi-gear`)
   - **Active page:** Text in `--brand-primary`, subtle left border or background tint
   - **Notifications badge:** Small pill counter next to the bell icon, `--brand-secondary` bg, white text, only visible when unread > 0
   - Vertical spacing: 4px between items, section separated from conversations list by a subtle divider

3. **Recent Conversations list**
   - Label: "Recent" in `--text-secondary`, small font, above the list
   - Each item: Single-line truncated title, `--text-primary`
   - **Hover:** `--bg-hover` background, reveal a `⋮` overflow menu button (hidden by default)
   - **Selected conversation:** Slightly tinted background using `rgba(53,88,114,0.08)` (brand-primary at low opacity)
   - **Overflow menu (⋮):** Dropdown with Rename, Delete
   - Shows the **10 most recent** conversations
   - List scrolls independently if it overflows

4. **User section (bottom, pinned)**
   - Separated by a top border (`--border-default`)
   - User initials in a circle badge: 32px, `--bg-tertiary` background, `--text-primary` text, subtle border
   - Name displayed next to avatar: "Rodrigo Rojas" in `--text-primary`, "Enterprise AI" label below in `--text-secondary` small text
   - **Dark/light mode toggle:** Small sun/moon icon button aligned to the right of the user section

---

## File 1: index.html — Chat Page

### Layout

Two regions: sidebar + main chat area. The chat area is **centered** within the available space (max-width 768px, auto margins) — matching how ChatGPT and Claude center their conversation threads.

### Chat Area — Empty State

When no conversation is loaded, show a centered empty state:
- Large "Enterprise GPT" wordmark or logo placeholder
- Subtitle: "How can I help you today?" in `--text-secondary`
- Optional: 2–3 suggested prompt chips (rounded pills, outlined, clickable) like ChatGPT's suggestion buttons

### Chat Area — Message Display

**User messages:**
- Right-aligned, contained in a rounded bubble (border-radius: 18px)
- Background: `--bg-tertiary` (light gray, NOT brand navy — matching ChatGPT's user bubble style)
- Text: `--text-primary`
- Max-width: 75% of the chat area
- **Copy button:** Appears on hover below the message, small icon button (`bi-clipboard`), `--text-secondary` color

**Assistant messages:**
- Left-aligned, **no bubble** — full width within the centered column
- Small avatar/icon to the left: "E" in a circle or a small branded icon, `--brand-primary` background, white text
- Label: "Enterprise GPT" in `--text-secondary` small text above the message content
- Content rendered in `--text-primary`, with proper paragraph spacing
- **Action buttons:** Appear on hover below the message — Copy (`bi-clipboard`), Thumbs up (`bi-hand-thumbs-up`), Thumbs down (`bi-hand-thumbs-down`) — all in `--text-secondary`, small and unobtrusive
- **Thinking/streaming indicator:** Three animated dots (opacity pulse animation), shown beneath the avatar/label while response is loading. Uses `--text-tertiary` color.
- **Streaming simulation:** `setInterval` appending tokens (words, not characters) to simulate realistic streaming speed. Typing cursor (blinking `|`) at the end of the stream, removed when complete.

### Prompt Input Box

Sticky to the bottom of the chat area (within the centered column, not full-width). Styled as a **single rounded container** (border-radius: 24px, `1px solid --border-default`, `--bg-primary` background) that holds all input elements — similar to ChatGPT's unified input bar.

**Inside the input container (bottom to top):**

1. **Textarea** — auto-expanding, no visible border (border: none), placeholder: "Message Enterprise GPT..."
2. **Bottom toolbar row** (inside the container, below the textarea):
   - Left side: File upload button (`bi-paperclip`, icon-only, `--text-secondary`), Model selector (small dropdown or pill button showing current model name, e.g., "GPT-4o" — subtle, not a full Bootstrap select)
   - Right side: Send button (`bi-arrow-up` in a filled circle, `--brand-primary` bg, white icon) — **only enabled when text is present or files are attached** (disabled state: `--bg-tertiary` bg, `--text-tertiary` icon). Stop button (`bi-stop-fill` in same circle position) replaces Send while streaming.

3. **File attachment area** (appears above textarea when files are selected):
   - Chips showing: file icon (based on type), file name (truncated), file size, remove `×` button
   - Chip style: `--bg-tertiary` background, rounded, small text
   - Supports multiple files

**Model selector options (static):** GPT-4o, GPT-4o Mini, Claude 3.5 Sonnet, Gemini 1.5 Pro

### Mock Chat Thread

Pre-load one conversation with:
- 2–3 user messages
- 2–3 completed assistant responses (include a code block in at least one response to show code formatting with a copy button and syntax highlighting via a `<pre><code>` block with a monospace font and `--bg-tertiary` background)
- 1 in-progress streaming response (actively appending words on page load)

### Subtle Footer Text

Below the input box, centered, very small text in `--text-tertiary`: "Enterprise GPT can make mistakes. Verify important information."

---

## File 2: conversations.html — Conversations List Page

### Layout

Sidebar + main content area. Content area is centered (max-width: 900px).

### Features

1. **Page header:** "Conversations" as a heading, clean and simple

2. **Search bar:**
   - Full-width input with `bi-search` icon inside, rounded (border-radius: 12px)
   - Filters list in real time (client-side)
   - Placeholder: "Search conversations..."

3. **Conversation list:**
   - Clean table or card-list layout (no heavy table borders)
   - Each row shows:
     - **Title** (clickable, `--text-primary`, semibold)
     - **Preview snippet** — first line of the last message, truncated, in `--text-secondary`
     - **Last updated** — relative time (e.g., "2 hours ago"), full ISO datetime on hover (Bootstrap tooltip)
     - **Overflow menu (⋮):** Open, Rename, Delete
   - Rows separated by `1px` bottom borders in `--border-default`
   - Hover state: `--bg-hover` background

4. **Pagination:**
   - Show 15 initially; "Load More" button (centered, outlined style) appends next batch
   - Counter above list: "Showing 15 of 63 conversations" in `--text-secondary`

5. **Multi-select deletion:**
   - Checkbox per row (subtle, `--border-default` border); Select All in header
   - When any are checked, a **floating bulk action bar** appears at the bottom (fixed position, centered, with shadow): "X selected" + "Delete Selected" button (red)
   - Confirmation modal required for bulk delete

6. **Clicking a title** navigates to `index.html?conversationId={id}`

---

## File 3: notifications.html — Notifications Center Page

### Layout

Sidebar + main content area. Content area is centered (max-width: 900px).

### Notification Severity Styles

Notifications use **very subtle** color coding — matching the calm overall aesthetic. Color is applied minimally.

| Type | Icon | Accent Color | Row Treatment (Light) | Row Treatment (Dark) |
|---|---|---|---|---|
| Success | `bi-check-circle-fill` | `#16A34A` | Left border 3px accent; icon in accent color; rest of row is default bg | Same pattern, dark bg |
| Error | `bi-x-circle-fill` | `#DC2626` | Same pattern | Same pattern |
| Warning | `bi-exclamation-triangle-fill` | `#D97706` | Same pattern | Same pattern |
| Info | `bi-info-circle-fill` | `#7AAACE` | Same pattern | Same pattern |

**Color application rules:**
- **Left border:** 3px solid accent color
- **Icon:** Accent color
- **Severity badge:** Small pill — accent color bg, white text
- **Unread rows:** Bold title text + small colored dot indicator next to title. Background is **default page bg** (not tinted — keep it calm)
- **Read rows:** Normal weight title, no dot, same default background

### Notification List Features

1. **Filter bar:**
   - Row of small pill/toggle buttons: All, Success, Error, Warning, Info
   - Active filter pill uses `--brand-primary` bg with white text; inactive pills are outlined
   - Filters update list in real time

2. **Notification rows, each showing:**
   - Severity icon (left) with accent color
   - Left border in accent color
   - **Title** (bold if unread) — short summary, e.g., "File upload failed"
   - **Message** — descriptive text, e.g., "The file report.pdf exceeded the 10MB size limit."
   - **Source** — pill label: Chat, File Upload, Admin, System (small, `--bg-tertiary` bg)
   - **Timestamp** — relative (e.g., "3 minutes ago"); full ISO datetime on hover
   - **Mark as Read** button (`bi-envelope-open`) — icon-only, appears on hover
   - **Dismiss** button (`bi-x-lg`) — icon-only, appears on hover; removes row with fade-out animation

3. **Bulk actions:**
   - Checkbox per row, Select All in header
   - Floating bottom bar when selected: "X selected" + "Mark as Read" + "Dismiss Selected" buttons
   - Counter updates dynamically

4. **"Mark All as Read"** button in the header area (outlined, small)

5. **Empty state:**
   - Centered: Large `bi-bell-slash` icon in `--text-tertiary`, "No notifications" heading, "You're all caught up." subtext
   - Clean and friendly

6. **Sidebar badge:** Notifications nav link shows live unread count, updates when items are marked as read or dismissed

7. **Pagination:** Show 15 initially; "Show More" centered button; counter: "Showing X of Y notifications"

### Mock Data

At least 25 hardcoded notifications covering all four types, mixed read/unread states, varied sources, timestamps spanning the last 30 days.

---

## File 4: administrator.html — Admin Management Page

### Access Control

- On page load: if `localStorage.isAdmin` is not `"true"`, show a **full-page access denied screen**
  - Centered content: `bi-shield-lock` icon (large, `--text-tertiary`), "Admin Access Required" heading, "You don't have permission to view this page." subtext, "Return to Chat" outlined button, and a "Login as Admin (Demo)" text link below that sets `localStorage.isAdmin = "true"` and reloads
- Sidebar: Admin link dimmed/muted when `isAdmin` is not true

### Layout

Sidebar + main content area (max-width: 1100px for admin — wider than chat/conversations to accommodate tables).

**Tabbed interface** with three tabs: Users, LLM Models, Permissions.

Tab styling: Underline tabs (not pill tabs) — active tab has `--brand-primary` bottom border and text color; inactive tabs are `--text-secondary`.

### Shared Tab Pattern

Each tab follows:
- **Toolbar row:** Search input (left, with icon) + filter dropdown(s) + "Add [Entity]" button (right, filled `--brand-primary` bg, white text)
- **Data table:** Clean, minimal borders (bottom border on each row only, no vertical cell borders)
- **Modals:** Bootstrap modal for Add/Edit — clean form layout, proper spacing
- **Delete:** Confirmation modal with entity name, destructive "Delete" button in red

### Tab 1: Users

**Table columns:**

| Column | Details |
|---|---|
| Avatar | Initials circle badge (32px, same style as sidebar avatar) |
| Full Name | First + Last, `--text-primary` |
| Email | `--text-secondary` |
| Role | Small badge: Admin (`--brand-primary` bg), User (`--brand-secondary` bg), Viewer (`--bg-tertiary` bg, `--text-secondary` text) |
| Status | Small badge: Active (`--success` bg, white text), Inactive (`--bg-tertiary` bg, `--text-secondary` text), Suspended (`--error` bg, white text) |
| Last Login | Relative timestamp; full datetime on hover |
| Actions | Icon buttons: `bi-pencil` Edit, `bi-trash` Delete — both `--text-secondary`, appear emphasized on row hover |

**Add/Edit User Modal:**
Fields: First Name, Last Name, Email, Role (dropdown), Status (dropdown), Password (add only), Confirm Password (add only).
Validation: All required, email format, passwords match. Bootstrap validation feedback.

**Delete User Modal:** "Delete [Full Name]?" with clear warning text. Cancel (outlined) + Delete (red filled) buttons.

**Search & Filters:** Real-time search by name/email; Role dropdown (All, Admin, User, Viewer); Status dropdown (All, Active, Inactive, Suspended).

**Mock data:** At least 15 users.

### Tab 2: LLM Models

**Table columns:**

| Column | Details |
|---|---|
| Icon | `bi-cpu` or similar |
| Model Name | e.g., GPT-4o |
| Provider | e.g., OpenAI, Anthropic, Google |
| Model ID / Slug | Monospace text, `--text-secondary` |
| Max Tokens | Formatted integer |
| Status | Badge: Enabled (`--success`), Disabled (`--bg-tertiary` + `--text-secondary`) |
| Available To | Badge: All Users, Admins Only, Custom |
| Actions | Edit, Delete |

**Add/Edit Modal:** Model Name, Provider (dropdown), Model ID, Max Tokens, API Endpoint (optional), Status toggle, Available To (dropdown), Description (textarea).

**Mock data:** At least 8 models across providers.

### Tab 3: Permissions

**Table columns:**

| Column | Details |
|---|---|
| Permission Name | e.g., "Can Upload Files" |
| Category | Badge/pill: Chat, Files, Admin, Models, System |
| Admin | Checkbox — always checked, disabled |
| User | Checkbox — editable |
| Viewer | Checkbox — editable |
| Actions | Edit, Delete |

**Add/Edit Modal:** Permission Name, Category (dropdown), Description, Role Access checkboxes (Admin always checked/disabled).

**Mock data:** At least 12 permissions.

---

## Mock Data Summary

| Page | Data |
|---|---|
| `index.html` | 30 conversations in sidebar; 1 pre-loaded chat thread (2–3 user, 2–3 assistant, 1 streaming) |
| `conversations.html` | Same 30 conversations, displayed in full list |
| `notifications.html` | 25 notifications across all types, mixed read/unread, varied sources, 30-day span |
| `administrator.html` | 15 users, 8 LLM models, 12 permissions |

---

## Animation & Transition Guidelines

- **All transitions:** 150ms ease (hover states, color changes, opacity)
- **Sidebar open/close:** 200ms ease-in-out, content area adjusts smoothly
- **Message appear:** New messages fade in + slight upward slide (translateY 8px → 0, opacity 0 → 1, 200ms)
- **Toast appear:** Slide up from bottom + fade in, 200ms
- **Notification dismiss:** Fade out + collapse height, 300ms
- **Streaming cursor:** Blink animation on the trailing `|` character (opacity toggle, 530ms)
- **Thinking dots:** Three dots with staggered opacity pulse (scale 0.4 → 1 → 0.4, 1.4s infinite, 160ms stagger between dots)
- **No jarring animations:** No bounce, no overshoot, no spring physics. Everything should feel calm and smooth.

---

## Responsive Behavior (Bonus)

While the primary target is desktop (1280px+), the layout should gracefully handle:
- **< 1024px:** Sidebar defaults to hidden (overlay mode when toggled open)
- **< 768px:** Chat input container goes full-width with reduced padding
- Tables in admin switch to card layout or horizontal scroll

---

## What Changed from v1 → v2

| Area | v1 (Old) | v2 (Improved) |
|---|---|---|
| **Surface colors** | Brand blues (#355872, #F7F8F0) used as primary backgrounds | Neutral white/gray backgrounds; brand blues as accent only |
| **User message bubbles** | Brand navy (#355872) background with light text | Light gray background with dark text (matches ChatGPT) |
| **Chat layout** | Full-width messages | Centered column (max-width 768px) like ChatGPT/Claude |
| **Input box** | Separate components (dropdown, textarea, buttons) | Unified rounded container with embedded toolbar |
| **Sidebar** | Colored/branded feel | Neutral, minimal, matches Claude's sidebar |
| **Typography** | Not specified | System font stack, specific sizes, generous line-height |
| **Notification rows** | Tinted background rows per severity | Minimal — left border + icon only, default bg for rows |
| **Hover interactions** | Not specified | Subtle bg shifts, action buttons revealed on hover |
| **Animations** | Not specified | Detailed timing specs for all transitions |
| **Empty states** | Not specified | Designed empty states for chat and notifications |
| **Toast position** | Bottom-right | Bottom-center (matches modern patterns) |
| **Color system** | 4 brand colors | Full neutral + brand + semantic token system |
| **Dark mode approach** | "Invert surface colors" | Specific dark mode values for every token |
| **Code blocks** | Not specified | Styled code blocks in assistant messages |
| **Action buttons** | Always visible | Appear on hover (cleaner resting state) |
| **Responsive** | Not mentioned | Basic responsive breakpoints defined |
