import type { DotNetInvoker } from "./types";

export const registerScrollFunctions = (): void => {
    window.scrollToElement = (elementId: string): void => {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: "smooth" });
        }
    };

    window.registerScrollListener = (dotNetHelper: DotNetInvoker): void => {
        window.addEventListener("scroll", () => {
            dotNetHelper.invokeMethodAsync("OnScroll", window.scrollY);
        });
    };
};
