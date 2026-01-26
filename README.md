# Directive Athena Website

Blazor WebAssembly site for Directive Athena. Built as a writing-first, sci-fi themed experience.

## About

Directive Athena is a standalone website, not a library. This repo focuses on content, presentation, and a custom writing workflow with localization.

## Features

- Space/sci-fi visual direction with animated backgrounds
- Multi-page layout (Home, Archive, Story, Contact)
- Writing-first archive with tags and localized metadata
- Writings Manager for local authoring
- Responsive layout with mobile-friendly navigation
- GitHub Pages deployment workflow

## Quick Start

1. `cd src/DirectiveAthena.Website`
2. `dotnet watch run`
3. Open `http://localhost:5000` (or the port in the output)

## Content and Writings

Use the Writings Manager locally:

1. Run the site locally: `dotnet watch run`
2. Open `http://localhost:5000/articles-manager`
3. Create/edit posts, tags, and translations

Where content lives:

- Article index and localized markdown files live in the site content directory
- Tag and UI strings live in the localization resources

Note: direct file writes use the browser File System Access API and are only available on `localhost`. In production, the manager offers download/copy workflows.

## Content Storage (R2)

The public site reads content from a public R2 bucket. The `wwwroot/appsettings.json` file is generated during the GitHub Pages workflow and is not committed.

Public config shape:

```json
{
  "ContentStorage": {
    "R2": {
      "PublicBaseUrl": "https://content.example.com/",
      "EnableWrites": false
    }
  }
}
```

Manager-only config (local file, not committed):

```json
{
  "ContentStorage": {
    "R2": {
      "PublicBaseUrl": "https://content.example.com/",
      "BucketName": "directiveathena-website-content",
      "AccountId": "YOUR_ACCOUNT_ID",
      "AccessKeyId": "YOUR_R2_ACCESS_KEY_ID",
      "SecretAccessKey": "YOUR_R2_SECRET_ACCESS_KEY",
      "EnableWrites": true
    }
  }
}
```

## Localization

- UI strings and tag labels are stored in `.resx` files under `src/DirectiveAthena.Website/Resources`.
- Localized markdown files are resolved by culture code: `content/articles/{culture}/{file}`.
- User language selection is stored in `localStorage` and applied at startup.

Adding a new culture:

1. Add the culture to the supported localization list in the code
2. Run locally and open `/articles-manager`
3. The manager can generate missing localization files and stubs for new cultures

## Deployment

Deployment runs through the GitHub Pages workflow with SPA-friendly routing.

## Tech Stack

- Framework: Blazor WebAssembly (.NET 10)
- UI: MudBlazor
- Markdown: InfiniLore.InfiniBlazor (custom built by Anna Sas)
- Icons: Material Icons and custom brands

## Contributing

- Keep changes focused and small per PR.
- Prefer editing content via the Writings Manager when possible.
- Add or update tests for new behavior.
