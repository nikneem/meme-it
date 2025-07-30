# Management Layout Component

The Management Layout Component provides a responsive navigation structure for the management section of the MemeIt application.

## Features

- **Responsive Design**: Adapts to different screen sizes with a collapsible mobile menu
- **Accessibility**: Includes ARIA labels, proper focus management, and keyboard navigation support
- **Smooth Animations**: CSS transitions for menu toggles and hover effects
- **Sticky Navigation**: Navigation bar stays at the top when scrolling

## Structure

```
/management
├── /memes (Meme Templates List)
└── /memes/create (Create New Meme)
```

## Navigation Elements

### Desktop View
- Logo (links to home)
- "Meme Templates" link
- "Create Meme" button (hidden on smaller screens)

### Mobile View
- Logo (links to home)
- Hamburger menu button
- Collapsible navigation menu with:
  - Meme Templates link
  - Create Meme link

## Responsive Breakpoints

- **Desktop**: >= 768px (md breakpoint)
  - Full navigation bar visible
  - Action buttons visible
- **Mobile**: < 768px
  - Hamburger menu replaces navigation links
  - "Create Meme" button hidden (accessible via mobile menu)

## Accessibility Features

- ARIA labels for all interactive elements
- Proper focus management
- Keyboard navigation support
- Screen reader friendly
- High contrast focus indicators

## Usage

The layout is automatically applied to all routes under `/management` through the Angular routing configuration. Child components are rendered in the `<router-outlet>` within the main content area.
