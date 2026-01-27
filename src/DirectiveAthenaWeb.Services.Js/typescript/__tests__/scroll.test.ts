import {describe, it, expect, beforeEach, afterEach, vi} from "vitest";
import {registerScrollFunctions} from "../interop/scroll";

describe("scroll interop", () => {
    beforeEach(() => {
        document.body.innerHTML = `<div id="target"></div>`;
    });

    afterEach(() => {
        document.body.innerHTML = "";
        delete (window as unknown as { scrollToElement?: unknown }).scrollToElement;
        delete (window as unknown as { registerScrollListener?: unknown }).registerScrollListener;
    });

    it("scrolls to an element when requested", () => {
        registerScrollFunctions();
        const target = document.getElementById("target") as HTMLElement;
        const scrollSpy = vi.fn();
        target.scrollIntoView = scrollSpy;

        window.scrollToElement("target");

        expect(scrollSpy).toHaveBeenCalledWith({behavior: "smooth"});
    });

    it("invokes the dotnet helper on scroll", () => {
        registerScrollFunctions();
        const dotNetHelper = {
            invokeMethodAsync: vi.fn()
        };

        Object.defineProperty(window, "scrollY", {value: 123, configurable: true});
        window.registerScrollListener(dotNetHelper);
        window.dispatchEvent(new Event("scroll"));

        expect(dotNetHelper.invokeMethodAsync).toHaveBeenCalledWith("OnScroll", 123);
    });
});
