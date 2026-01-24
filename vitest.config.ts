import { defineConfig } from "vitest/config";

export default defineConfig({
    test: {
        environment: "jsdom",
        include: ["src/DirectiveAthena.Website/wwwroot/ts/**/*.test.ts"]
    }
});
