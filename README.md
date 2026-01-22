# Directive Athena Website

A standalone Blazor WebAssembly landing page for Directive Athena, featuring a space/sci-fi aesthetic and a writing-first experience.

## Features

- **Space Aesthetic**: Dark cosmic palette with animated starfield and nebula backgrounds.
- **Multi-Page Architecture**: Dedicated pages for Home, Archive, Story, and Contact.
- **Twitch Integration**: Live stream embed on the Home page for real-time development tracking.
- **Glassmorphism**: Modern UI using MudBlazor with glassy effects on the App Bar and cards.
- **Writing-First**: Centralized writings system with latest post highlighting and a full archive with tag filtering.
- **Responsive**: Mobile-friendly navigation with a scroll-responsive App Bar.
- **GitHub Pages Ready**: Optimized for standalone deployment with SPA-safe routing.

## How to Run Locally

1. Ensure you have the .NET 10 SDK installed.
2. Clone the repository.
3. Navigate to the project directory: `cd src/DirectiveAthena.Website`
4. Run the application: `dotnet watch run`
5. Open your browser to `http://localhost:5000` (or the port specified in the output).

## How to Add New Writings

The project now supports localization. To add a new writing, you can use the built-in **Writings Manager** dev tool or edit the files manually.

### Using the Writings Manager
1. Run the site locally: `dotnet watch run`
2. Navigate to `/articles-manager` in your browser.
3. Use the interface to create/edit posts, add tags, and provide translations for English and Dutch.
4. Click **Download index.json** and replace the file at `wwwroot/content/articles/index.json`.
5. Click **Download Markdown Stubs** to get template files for your content and place them in `wwwroot/content/articles/`.
6. Edit the markdown files with your content.

### Manual Method
1. Create a new Markdown file in `wwwroot/content/articles/` (e.g., `my-new-post.md`).
2. (Optional) Create a localized version: `my-new-post.nl.md`.
3. Open `wwwroot/content/articles/index.json`.
4. Add a new entry to the JSON array:
   ```json
   {
     "id": "my-new-post",
     "date": "2026-01-21",
     "tags": ["AI", "Sovereignty"],
     "file": "my-new-post.md",
     "title": {
       "en": "My New Post Title",
       "nl": "Mijn Nieuwe Bericht Titel"
     },
     "summary": {
       "en": "A brief summary in English.",
       "nl": "Een korte samenvatting in het Nederlands."
     }
   }
   ```
5. Note: Tag IDs used in the `tags` array must have corresponding entries in the `.resx` files (e.g., `Tag.AI`).
6. Place the markdown files in culture-specific folders: `wwwroot/content/articles/en/my-new-post.md` and `wwwroot/content/articles/nl/my-new-post.md`.

## Localization

### Core Model
- **UI Strings**: Managed via `.resx` files in the `Resources` folder.
  - `Shared.resx`: Default (English) strings.
  - `Shared.nl.resx`: Dutch translations.
- **Tag Translations**: Also come from `.resx` files using the `Tag.` prefix (e.g., `Tag.ai`).
- **Files**: Localized markdown files are resolved by path: `content/articles/{culture}/{file}`.
- **Persistence**: User language choice is saved in `localStorage` and applied at startup.

### How to Add a New Culture
1. Open `src/DirectiveAthena.Website/Models/CultureConfig.cs`.
2. Add a new `CultureConfig` entry to the `SupportedCultures` list:
   ```csharp
   new("fr", "Français", "FR", "https://flagcdn.com/w40/fr.png")
   ```
3. Run the application locally: `dotnet watch run`.
4. Navigate to `/articles-manager` and grant File System Access.
5. The app will automatically create the missing `Shared.<culture>.resx` file in the `Resources` folder.
6. Use the **Tags** tab in the Writings Manager to add translations for the new culture.
7. Use the **Posts** and **Markdown** tabs to add translated metadata and content.
8. The new culture will automatically appear in the **CultureSelector** UI.

### Dev-only File System Access
Direct writes to `index.json`, `.md` files, and `.resx` files are only available on `localhost` via the **File System Access API**. In production, the Writings Manager provides download/copy buttons as fallbacks.

## How to Publish for GitHub Pages

1. Build the project in Release mode:
   ```bash
   dotnet publish -c Release
   ```
2. The output will be in `bin/Release/net10.0/publish/wwwroot`.
3. **Adjust Base Href**: If your site is hosted at `https://<username>.github.io/<repo-name>/`, open `index.html` in the output folder and change `<base href="/" />` to `<base href="/<repo-name>/" />`.
4. Deploy the contents of the `wwwroot` folder to your `gh-pages` branch or configure GitHub Actions to do so.
5. For SPA routing to work correctly on GitHub Pages (to handle direct links to sub-routes), you may need a `404.html` trick (copying `index.html` to `404.html`).

## Tech Stack

- **Framework**: Blazor WebAssembly (.NET 10)
- **Social Preview**: Open Graph and Twitter Cards for rich social sharing.
  - Preview image: `wwwroot/assets/social-preview.svg` (1200x630).
  - To update the preview: Replace or edit the SVG file and ensure `index.html` meta tags reflect any size changes.
- **UI Components**: [MudBlazor](https://mudblazor.com/)
- **Markdown Rendering**: [Markdig](https://github.com/xoofx/markdig)
- **Icons**: Material Icons & Custom Brands via MudBlazor

## Package Management

Packages are centrally managed in `Directory.Packages.props`. Add new `PackageVersion` entries there; project `PackageReferences` omit versions.
