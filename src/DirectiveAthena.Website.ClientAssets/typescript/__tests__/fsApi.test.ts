import "fake-indexeddb/auto";
import { describe, it, expect, beforeEach } from "vitest";
import { registerFsApi } from "../interop/fsApi";

describe("fsApi interop", () => {
    beforeEach(() => {
        delete (window as unknown as { showDirectoryPicker?: unknown }).showDirectoryPicker;
        registerFsApi();
    });

    it("reports unsupported when directory picker is unavailable", () => {
        expect(window.fsApi.isSupported()).toBe(false);
    });

    it("returns false when requestAccess cannot prompt", async () => {
        await expect(window.fsApi.requestAccess()).resolves.toBe(false);
    });

    it("returns false when no handle is stored", async () => {
        await expect(window.fsApi.hasAccess()).resolves.toBe(false);
    });
});
