import type {DotNetInvoker, FsApi} from "./interop/types";

declare global {
    interface FileSystemHandlePermissionDescriptor {
        mode?: "read" | "readwrite";
    }

    interface FileSystemDirectoryPickerOptions {
        mode?: "read" | "readwrite";
    }

    interface FileSystemGetDirectoryOptions {
        create?: boolean;
    }

    interface FileSystemGetFileOptions {
        create?: boolean;
    }

    interface FileSystemRemoveOptions {
        recursive?: boolean;
    }

    interface FileSystemWritableFileStream {
        write(data: string): Promise<void>;

        close(): Promise<void>;
    }

    interface FileSystemFileHandle {
        getFile(): Promise<File>;

        createWritable(): Promise<FileSystemWritableFileStream>;
    }

    interface FileSystemDirectoryHandle {
        queryPermission?: (descriptor: FileSystemHandlePermissionDescriptor) => Promise<PermissionState>;
        requestPermission?: (descriptor: FileSystemHandlePermissionDescriptor) => Promise<PermissionState>;

        getDirectoryHandle(
            name: string,
            options?: FileSystemGetDirectoryOptions
        ): Promise<FileSystemDirectoryHandle>;

        getFileHandle(name: string, options?: FileSystemGetFileOptions): Promise<FileSystemFileHandle>;

        removeEntry(name: string, options?: FileSystemRemoveOptions): Promise<void>;
    }

    // noinspection JSUnusedGlobalSymbols
    interface Window {
        showDirectoryPicker?: (
            options?: FileSystemDirectoryPickerOptions
        ) => Promise<FileSystemDirectoryHandle>;
        scrollToElement: (elementId: string) => void;
        registerScrollListener: (dotNetHelper: DotNetInvoker) => void;
        copyToClipboard: (text: string) => Promise<boolean>;
        downloadFile: (fileName: string, content: string) => void;
        fsApi: FsApi;
    }
}

export {};
