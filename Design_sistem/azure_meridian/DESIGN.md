# Design System Specification: The Corporate Curator

## 1. Overview & Creative North Star
The "Creative North Star" for this design system is **The Digital Curator**. In an era of cluttered SaaS dashboards and rigid, boxy templates, this system moves in the opposite direction. It treats data and corporate workflows with an editorial sensibility—prioritizing breathing room, tonal depth, and a "quiet" authority.

The system moves beyond "standard" UI by rejecting the traditional box-model. Instead of defining elements by their borders, we define them by their presence. Through intentional asymmetry, a restricted palette of deep blues and soft greys, and a sophisticated layering of surfaces, the interface should feel less like a "tool" and more like a high-end, bespoke digital workspace.

---

## 2. Colors & Surface Logic
The palette is rooted in professional stability (`primary: #003461`) but avoids the sterility of typical corporate blues by introducing "living" neutrals and a vibrant emerald tertiary accent.

### The "No-Line" Rule
**Explicit Instruction:** Designers are prohibited from using 1px solid borders to define sections. Layout boundaries must be established solely through:
1.  **Background Color Shifts:** Placing a `surface_container_low` section against a `surface` background.
2.  **Tonal Transitions:** Using whitespace and subtle shifts in surface tiers to imply structure.

### Surface Hierarchy & Nesting
Treat the UI as a physical stack of premium materials. Use the `surface_container` tiers to create depth:
*   **Base Layer:** `surface` (#f8f9fb) – The canvas.
*   **Structural Layer:** `surface_container_low` (#f2f4f6) – For sidebar backgrounds or secondary content zones.
*   **Focus Layer:** `surface_container_lowest` (#ffffff) – For the main cards or data entry areas that need to "pop" forward.
*   **Overlay Layer:** Use Glassmorphism (Surface color at 70% opacity + 16px backdrop-blur) for floating menus or navigation bars.

### Signature Textures
Main CTAs and Hero sections should utilize a **Subtle Linear Gradient** (135deg) from `primary` (#003461) to `primary_container` (#004b87). This adds a "lithic" weight and professional polish that flat fills cannot replicate.

---

## 3. Typography: Editorial Authority
The typography system uses a dual-sans-serif approach to balance character with legibility.

*   **Display & Headlines (Manrope):** Chosen for its geometric precision and modern "tech" feel. Use `display-lg` and `headline-md` with tighter letter-spacing (-0.02em) to create a commanding, editorial look for key data points and section titles.
*   **Body & Labels (Inter):** The industry standard for legibility. Inter provides a neutral, high-contrast clarity for dense SaaS data.
*   **The Hierarchy Strategy:** Use `title-lg` for card headers in `primary` color to anchor the user's eye, while using `label-md` in `on_surface_variant` for metadata to ensure a clear information architecture.

---

## 4. Elevation & Depth: The Layering Principle
We reject traditional drop shadows in favor of **Tonal Layering**. 

*   **Soft Lift:** Achieve separation by placing a `surface_container_lowest` card on a `surface_container_low` background. The difference in hex values provides a natural, "built-in" lift.
*   **Ambient Shadows:** If an element must float (e.g., a Modal), use a shadow with a 32px blur, 0px spread, and 6% opacity of the `on_surface` color (#191c1e). This mimics natural light rather than a digital effect.
*   **The Ghost Border:** If accessibility requires a stroke (e.g., in high-contrast modes), use the `outline_variant` (#c2c6d1) at **15% opacity**. It should be felt, not seen.

---

## 5. Components

### Buttons
*   **Primary:** Gradient fill (`primary` to `primary_container`), `on_primary` text, `md` (0.375rem) rounded corners.
*   **Secondary:** `surface_container_highest` fill with `on_surface` text. No border.
*   **Tertiary/Ghost:** Transparent background, `primary` text. Use for low-priority actions.

### Cards & Lists
*   **Rule:** Forbid the use of divider lines.
*   **Execution:** Separate list items using 8px of vertical whitespace or a hover state that shifts the background to `surface_container_high`.
*   **Style:** Cards use `surface_container_lowest` and `lg` (0.5rem) corner radius for a friendly yet structured appearance.

### Input Fields
*   **Default:** `surface_container_lowest` background with a `ghost border` (15% `outline_variant`).
*   **Focus:** Transition the ghost border to 100% `primary` and add a 2px outer glow of `primary_fixed` (#d3e4ff).

### Signature Component: The "Status Emerald" Chip
For SaaS indicators (Active, Success, Online), use `tertiary_container` (#00544b) with `on_tertiary_container` (#63cbbb) text. The deep emerald-on-indigo vibe provides a sophisticated "pro" look compared to standard bright green.

---

## 6. Do’s and Don’ts

### Do
*   **Do** use asymmetrical margins (e.g., a wider left margin than right) for dashboard headers to create an editorial feel.
*   **Do** use `on_surface_variant` for secondary text to maintain a soft visual hierarchy.
*   **Do** utilize `xl` (0.75rem) rounding for large containers to soften the corporate edge.

### Don't
*   **Don't** use pure black (#000000) for text; always use `on_surface` (#191c1e).
*   **Don't** use 100% opaque borders to separate content; let the background tiers do the work.
*   **Don't** use standard "Select" dropdowns; use custom glassmorphic overlays for a premium feel.