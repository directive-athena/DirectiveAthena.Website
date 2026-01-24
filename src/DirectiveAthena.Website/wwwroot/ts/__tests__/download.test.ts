import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { registerDownload } from "../interop/download";

describe("download interop", () => {
    const originalCreateObjectURL = URL.createObjectURL;
    const originalRevokeObjectURL = URL.revokeObjectURL;
    const originalCreateElement = document.createElement.bind(document);

    beforeEach(() => {
        URL.createObjectURL = vi.fn(() => "blob:mock");
        URL.revokeObjectURL = vi.fn();
    });

    afterEach(() => {
        URL.createObjectURL = originalCreateObjectURL;
        URL.revokeObjectURL = originalRevokeObjectURL;
        document.createElement = originalCreateElement;
    });

    it("creates and revokes a download URL", () => {
        const anchor = originalCreateElement("a");
        const clickSpy = vi.fn();
        anchor.click = clickSpy;
        document.createElement = vi.fn((tagName: string) =>
            tagName === "a" ? anchor : originalCreateElement(tagName)
        ) as typeof document.createElement;

        registerDownload();
        window.downloadFile("file.txt", "content");

        expect(URL.createObjectURL).toHaveBeenCalled();
        expect(clickSpy).toHaveBeenCalled();
        expect(URL.revokeObjectURL).toHaveBeenCalledWith("blob:mock");
    });
});
