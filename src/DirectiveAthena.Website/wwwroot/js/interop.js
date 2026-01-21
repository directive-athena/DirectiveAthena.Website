// Progress observer
const observer = new MutationObserver(() => {
    const app = document.getElementById('app');
    const loadingScreen = document.getElementById('loading-screen');

    if (app) {
        // If app contains content other than the loading screen, it means Blazor has rendered
        if (app.children.length > 1 || (app.children.length === 1 && app.children[0].id !== 'loading-screen')) {
            if (loadingScreen && !loadingScreen.classList.contains('activated')) {
                loadingScreen.classList.add('activated');
                
                // Remove from DOM after animation completes
                setTimeout(() => {
                    loadingScreen.remove();
                }, 1000);
            }
            observer.disconnect();
        }
    }
});

if (document.getElementById('app')) {
    observer.observe(document.getElementById('app'), { attributes: true, childList: true, subtree: true });
}

window.scrollToElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
};

window.registerScrollListener = (dotNetHelper) => {
    window.addEventListener('scroll', () => {
        // noinspection JSUnresolvedReference
        dotNetHelper.invokeMethodAsync('OnScroll', window.scrollY);
    });
};

window.downloadFile = (fileName, content) => {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    window.URL.revokeObjectURL(url);
};

// File System Access API Interop
// const FS_HANDLES_KEY = 'da_fs_handles';

async function getIndexedDB() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('DirectiveAthenaDB', 1);
        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            db.createObjectStore('handles');
        };
        request.onsuccess = (event) => resolve(event.target.result);
        request.onerror = (event) => reject(event.target.error);
    });
}

async function storeHandle(key, handle) {
    const db = await getIndexedDB();
    const tx = db.transaction('handles', 'readwrite');
    const store = tx.objectStore('handles');
    store.put(handle, key);
    return tx.complete;
}

async function getHandle(key) {
    const db = await getIndexedDB();
    const tx = db.transaction('handles', 'readonly');
    const store = tx.objectStore('handles');
    return new Promise((resolve) => {
        const request = store.get(key);
        request.onsuccess = () => resolve(request.result);
    });
}

// noinspection JSUnusedGlobalSymbols
window.fsApi = {
    isSupported: () => {
        return 'showDirectoryPicker' in window;
    },
    requestAccess: async () => {
        try {
            // noinspection JSUnresolvedReference
            const handle = await window.showDirectoryPicker({
                mode: 'readwrite'
            });
            await storeHandle('repo-root', handle);
            return true;
        } catch (e) {
            console.error('Access denied or failed', e);
            return false;
        }
    },
    hasAccess: async () => {
        const handle = await getHandle('repo-root');
        if (!handle) return false;
        
        // noinspection JSUnresolvedReference
        const permission = await handle.queryPermission({ mode: 'readwrite' });
        if (permission === 'granted') return true;
        if (permission === 'prompt') {
            // We don't want to prompt automatically, just return false so UI can show "Grant" button
            return false;
        }
        return false;
    },
    verifyPermission: async () => {
        const handle = await getHandle('repo-root');
        if (!handle) return false;
        
        // noinspection JSCheckFunctionSignatures
        const permission = await handle.requestPermission({ mode: 'readwrite' });
        return permission === 'granted';
    },
    resetAccess: async () => {
        const db = await getIndexedDB();
        const tx = db.transaction('handles', 'readwrite');
        tx.objectStore('handles').clear();
        return true;
    },
    writeFile: async (relativePath, content) => {
        try {
            const rootHandle = await getHandle('repo-root');
            if (!rootHandle) { 
                // noinspection ExceptionCaughtLocallyJS
                throw new Error('No root handle found');
            }

            const parts = relativePath.split(/[\\/]/);
            let currentHandle = rootHandle;

            for (let i = 0; i < parts.length - 1; i++) {
                currentHandle = await currentHandle.getDirectoryHandle(parts[i], { create: true });
            }

            const fileHandle = await currentHandle.getFileHandle(parts[parts.length - 1], { create: true });
            const writable = await fileHandle.createWritable();
            await writable.write(content);
            await writable.close();
            return true;
        } catch (e) {
            console.error('Failed to write file: ' + relativePath, e);
            return false;
        }
    },
    readFile: async (relativePath) => {
        try {
            const rootHandle = await getHandle('repo-root');
            if (!rootHandle) { 
                // noinspection ExceptionCaughtLocallyJS
                throw new Error('No root handle found');
            }

            const parts = relativePath.split(/[\\/]/);
            let currentHandle = rootHandle;

            for (let i = 0; i < parts.length - 1; i++) {
                currentHandle = await currentHandle.getDirectoryHandle(parts[i]);
            }

            const fileHandle = await currentHandle.getFileHandle(parts[parts.length - 1]);
            const file = await fileHandle.getFile();
            return await file.text();
        } catch (e) {
            console.error('Failed to read file: ' + relativePath, e);
            return null;
        }
    }
};
