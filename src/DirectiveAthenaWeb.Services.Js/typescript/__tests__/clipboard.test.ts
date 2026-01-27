import {describe, it, expect, afterEach, beforeEach, vi} from "vitest";
import {registerClipboard} from "../interop/clipboard";

describe("clipboard interop", () => {
    const originalExecCommand = document.execCommand;

    beforeEach(() => {
        Object.defineProperty(window, "isSecureContext", {value: false, configurable: true});
        document.execCommand = vi.fn(() => true);
    });

    afterEach(() => {
        document.execCommand = originalExecCommand;
        document.body.innerHTML = "";
    });

    it("falls back to execCommand when Clipboard API is unavailable", async () => {
        registerClipboard();

        const result = await window.copyToClipboard("hello");

        expect(result).toBe(true);
        expect(document.execCommand).toHaveBeenCalledWith("copy");
    });
});
