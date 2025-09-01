# CSS Build Process

This project uses Tailwind CSS v3.4 for styling, installed locally via pnpm.

## Building CSS

### Production Build (Minified)

```bash
pnpm run build-css-prod
```

### Development Build (Watch Mode)

```bash
pnpm run build-css
```

## Setup for New Development Environment

1. Install pnpm (if not already installed):

   ```bash
   npm install -g pnpm
   ```

2. Install dependencies:

   ```bash
   pnpm install
   ```

3. Build CSS:
   ```bash
   pnpm run build-css-prod
   ```

## File Structure

- `wwwroot/css/input.css` - Source CSS file with Tailwind directives
- `wwwroot/css/tailwind.css` - Generated CSS file (included in layout)
- `tailwind.config.js` - Tailwind configuration
- `postcss.config.js` - PostCSS configuration

## Configuration

The Tailwind configuration includes:

- Content scanning for all `.cshtml` files in Views/
- Custom font family (Inter)
- Forms and Typography plugins
- Custom touch-target utilities

## Notes

- The generated `tailwind.css` file is included in the repository for deployment
- Always run `pnpm run build-css-prod` after making changes to styles
- For continuous development, use `pnpm run build-css` to watch for changes
