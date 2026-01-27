import type {FsApi} from "./types";

const DATABASE_NAME = "DirectiveAthenaDB";
const STORE_NAME = "handles";

const openIndexedDb = (): Promise<IDBDatabase> =>
    new Promise((resolve, reject) => {
        const request = indexedDB.open(DATABASE_NAME, 1);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME);
            }
        };

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });

const storeHandle = async (key: string, handle: FileSystemDirectoryHandle): Promise<void> => {
    const db = await openIndexedDb();

    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE_NAME, "readwrite");
        tx.objectStore(STORE_NAME).put(handle, key);
        tx.oncomplete = () => resolve();
        tx.onerror = () => reject(tx.error);
        tx.onabort = () => reject(tx.error);
    });
};

const getHandle = async (key: string): Promise<FileSystemDirectoryHandle | null> => {
    const db = await openIndexedDb();

    return new Promise((resolve) => {
        const tx = db.transaction(STORE_NAME, "readonly");
        const request = tx.objectStore(STORE_NAME).get(key);
        request.onsuccess = () =>
            resolve((request.result as FileSystemDirectoryHandle | undefined) ?? null);
        request.onerror = () => resolve(null);
    });
};

const clearHandles = async (): Promise<void> => {
    const db = await openIndexedDb();

    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE_NAME, "readwrite");
        tx.objectStore(STORE_NAME).clear();
        tx.oncomplete = () => resolve();
        tx.onerror = () => reject(tx.error);
        tx.onabort = () => reject(tx.error);
    });
};

const ensureRootHandle = async (): Promise<FileSystemDirectoryHandle> => {
    const rootHandle = await getHandle("repo-root");
    if (!rootHandle) {
        throw new Error("No root handle found");
    }
    return rootHandle;
};

const getPathParts = (relativePath: string): string[] =>
    relativePath.split(/[\\/]/).filter((part) => part.length > 0);

export const registerFsApi = (): void => {
    const fsApi: FsApi = {
        isSupported: () => "showDirectoryPicker" in window,
        requestAccess: async () => {
            try {
                if (!window.showDirectoryPicker) {
                    return false;
                }

                const handle = await window.showDirectoryPicker({
                    mode: "readwrite"
                });
                await storeHandle("repo-root", handle);
                return true;
            } catch (error) {
                console.error("Access denied or failed", error);
                return false;
            }
        },
        hasAccess: async () => {
            const handle = await getHandle("repo-root");
            if (!handle) {
                return false;
            }

            const permission = await handle.queryPermission?.({mode: "readwrite"});
            if (!permission) {
                return false;
            }
            if (permission === "granted") {
                return true;
            }

            if (permission === "prompt") {
                // We don't want to prompt automatically, just return false so UI can show "Grant" button.
                return false;
            }

            return false;
        },
        verifyPermission: async () => {
            const handle = await getHandle("repo-root");
            if (!handle) {
                return false;
            }

            const permission = await handle.requestPermission?.({mode: "readwrite"});
            return permission === "granted";
        },
        resetAccess: async () => {
            await clearHandles();
            return true;
        },
        writeFile: async (relativePath: string, content: string) => {
            try {
                const rootHandle = await ensureRootHandle();
                const parts = getPathParts(relativePath);
                if (parts.length === 0) {
                    throw new Error("Invalid path: " + relativePath);
                }

                let currentHandle = rootHandle;
                for (let i = 0; i < parts.length - 1; i++) {
                    currentHandle = await currentHandle.getDirectoryHandle(parts[i], {
                        create: true
                    });
                }

                const fileHandle = await currentHandle.getFileHandle(
                    parts[parts.length - 1],
                    {create: true}
                );
                const writable = await fileHandle.createWritable();
                await writable.write(content);
                await writable.close();
                return true;
            } catch (error) {
                console.error("Failed to write file: " + relativePath, error);
                return false;
            }
        },
        readFile: async (relativePath: string) => {
            try {
                const rootHandle = await ensureRootHandle();
                const parts = getPathParts(relativePath);
                if (parts.length === 0) {
                    throw new Error("Invalid path: " + relativePath);
                }

                let currentHandle = rootHandle;
                for (let i = 0; i < parts.length - 1; i++) {
                    currentHandle = await currentHandle.getDirectoryHandle(parts[i]);
                }

                const fileHandle = await currentHandle.getFileHandle(parts[parts.length - 1]);
                const file = await fileHandle.getFile();
                return await file.text();
            } catch (error) {
                console.error("Failed to read file: " + relativePath, error);
                return null;
            }
        },
        deleteFile: async (relativePath: string) => {
            try {
                const rootHandle = await ensureRootHandle();
                const parts = getPathParts(relativePath);
                if (parts.length === 0) {
                    throw new Error("Invalid path: " + relativePath);
                }

                let currentHandle = rootHandle;
                for (let i = 0; i < parts.length - 1; i++) {
                    currentHandle = await currentHandle.getDirectoryHandle(parts[i]);
                }

                await currentHandle.removeEntry(parts[parts.length - 1]);
                return true;
            } catch (error) {
                if (error instanceof DOMException && error.name === "NotFoundError") {
                    return true;
                }

                console.error("Failed to delete file: " + relativePath, error);
                return false;
            }
        }
    };

    window.fsApi = fsApi;
};
