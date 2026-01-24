import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { initLoadingObserver } from "../interop/loadingObserver";

describe("initLoadingObserver", () => {
    beforeEach(() => {
        document.body.innerHTML = `
            <div id="app">
                <div id="loading-screen" class="loading-container"></div>
            </div>
        `;
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
        document.body.innerHTML = "";
    });

    it("activates and removes the loading screen after render", async () => {
        const app = document.getElementById("app") as HTMLElement;
        const loadingScreen = document.getElementById("loading-screen") as HTMLElement;

        initLoadingObserver();
        app.appendChild(document.createElement("div"));

        await Promise.resolve();

        expect(loadingScreen.classList.contains("activated")).toBe(true);

        vi.runAllTimers();

        expect(document.getElementById("loading-screen")).toBeNull();
    });
});
