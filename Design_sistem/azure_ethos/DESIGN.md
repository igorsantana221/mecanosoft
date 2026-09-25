# Design System Specification: High-End Digital Editorial

## 1. Overview & Creative North Star: "The Architectural Curator"
This design system moves beyond the "standard SaaS" aesthetic to embrace an editorial, high-end feel defined as **The Architectural Curator**. 

The goal is to move away from rigid, boxed-in layouts and instead treat the web application as a series of curated, intentional spaces. We achieve this through **Atmospheric Depth** (using tonal shifts rather than lines) and **Editorial Scale** (leveraging dramatic typographic contrast). By prioritizing whitespace as a functional element rather than a void, we create a professional environment that feels calm, authoritative, and bespoke.

---

## 2. Colors: Tonal Depth & The "No-Line" Rule
The palette is rooted in deep, authoritative blues (`primary`) and sophisticated, neutral grays (`surface`). 

### The "No-Line" Rule
**Explicit Instruction:** Prohibit the use of 1px solid borders for sectioning or containment. 
Boundaries must be defined solely through background color shifts. For example, a `surface-container-low` section should sit directly on a `surface` background to create a "pocket" of content without a hard stroke.

### Surface Hierarchy & Nesting
Treat the UI as physical layers of fine paper. 
- **Level 0 (Foundation):** `surface` (#f7f9fc)
- **Level 1 (Sectioning):** `surface-container-low` (#f2f4f7)
- **Level 2 (Interaction/Cards):** `surface-container-lowest` (#ffffff)
- **Level 3 (High Prominence):** `surface-container-high` (#e6e8eb)

### The Glass & Gradient Rule
To prevent a "flat" feel, use **Glassmorphism** for floating elements (like Navigation Bars or Popovers). Use `surface` at 80% opacity with a `24px` backdrop-blur. 
For primary CTAs, apply a subtle linear gradient: `primary` (#003461) to `primary-container` (#004b87) at a 135° angle. This adds "visual soul" and a tactile, premium finish.

---

## 3. Typography: The Editorial Voice
We utilize a dual-sans-serif pairing to distinguish between "Action" and "Content."

*   **Display & Headlines (Manrope):** Chosen for its geometric precision and modern warmth. Use high-contrast sizing (e.g., `display-lg` at 3.5rem) to create clear entry points.
*   **Body & Labels (Inter):** The workhorse. Inter provides exceptional legibility at small sizes for data-heavy application needs.

**Hierarchy Strategy:** 
- Use `display-md` for Hero sections to establish immediate authority.
- Use `title-sm` in `primary` (#003461) for sub-headers to anchor the eye.
- Maintain a generous line-height (1.6x) for `body-lg` to reinforce the "plenty of whitespace" requirement.

---

## 4. Elevation & Depth: Tonal Layering
Traditional drop shadows are often a crutch for poor layout. In this system, depth is achieved through the **Layering Principle**.

### The Layering Principle
Stack surface tiers to create "lift." A card (`surface-container-lowest`) placed on a page (`surface-container-low`) creates a natural, soft separation.

### Ambient Shadows
When a floating effect is mandatory (e.g., a Modal), use **Ambient Shadows**:
- **Blur:** 40px - 60px
- **Spread:** -10px
- **Color:** `on-surface` (#191c1e) at 6% opacity. 
This mimics natural light dispersion rather than a dated "drop shadow."

### The "Ghost Border" Fallback
If accessibility testing requires a border, use a **Ghost Border**: `outline-variant` (#c2c6d1) at **15% opacity**. It should be felt, not seen.

---

## 5. Components: Intentional Primitives

### Buttons
- **Primary:** Gradient fill (`primary` to `primary-container`), `on-primary` text. `xl` roundedness (1.5rem). 
- **Secondary:** `surface-container-highest` background with `on-surface` text. No border.
- **Tertiary:** Pure text using `primary` color, with an `on-primary-container` background shift on hover.

### Input Fields
- **Styling:** Use `surface-container-low` as the field background. 
- **States:** On focus, transition background to `surface-container-lowest` and add a 2px "Ghost Border" using `surface-tint`.
- **Corner Radius:** Use `md` (0.75rem) for a friendly, approachable feel.

### Cards & Lists
- **Rule:** Forbid divider lines. 
- **Execution:** Separate list items with 16px of vertical whitespace. For cards, use `surface-container-lowest` against a `surface-container-low` background. 
- **Interactivity:** On hover, a card should shift from `surface-container-lowest` to `surface-bright` and gain an Ambient Shadow.

### Chips (Navigation & Filtering)
- Use `full` roundedness (pill shape). 
- **Selected:** `secondary-container` background with `on-secondary-container` text.
- **Unselected:** `surface-container-high` background.

---

## 6. Do's and Don'ts

### Do:
- **Do** use asymmetrical margins (e.g., a wider left margin than right) to create an editorial, "curated" feel in headers.
- **Do** use `primary-fixed-dim` for subtle accent backgrounds in data visualization.
- **Do** ensure all interactive targets are at least 44px tall for accessibility.

### Don't:
- **Don't** use 100% black (#000000) for text. Always use `on-surface` (#191c1e) to maintain the "soft gray" professional tonal balance.
- **Don't** use the `none` roundedness setting. Everything must have at least a `sm` (0.25rem) radius to stay "friendly."
- **Don't** crowd components. If a layout feels busy, increase the whitespace by 1.5x before considering a divider line.