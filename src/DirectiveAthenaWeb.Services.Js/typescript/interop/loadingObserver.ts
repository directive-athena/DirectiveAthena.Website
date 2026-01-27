export const initLoadingObserver = (): void => {
    const observer = new MutationObserver(() => {
        const app = document.getElementById("app");
        const loadingScreen = document.getElementById("loading-screen");

        if (!app) {
            return;
        }

        // If app contains content other than the loading screen, it means Blazor has rendered.
        const hasRendered =
            app.children.length > 1 ||
            (app.children.length === 1 && app.children[0]?.id !== "loading-screen");

        if (!hasRendered) {
            return;
        }

        if (loadingScreen && !loadingScreen.classList.contains("activated")) {
            loadingScreen.classList.add("activated");

            // Remove from DOM after animation completes.
            window.setTimeout(() => {
                loadingScreen.remove();
            }, 1000);
        }

        observer.disconnect();
    });

    const app = document.getElementById("app");
    if (app) {
        observer.observe(app, {attributes: true, childList: true, subtree: true});
    }
};
