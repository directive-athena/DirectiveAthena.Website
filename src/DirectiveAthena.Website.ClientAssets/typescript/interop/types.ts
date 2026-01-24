export interface DotNetInvoker {
    invokeMethodAsync(methodIdentifier: string, ...args: unknown[]): Promise<unknown>;
}

export interface FsApi {
    isSupported: () => boolean;
    requestAccess: () => Promise<boolean>;
    hasAccess: () => Promise<boolean>;
    verifyPermission: () => Promise<boolean>;
    resetAccess: () => Promise<boolean>;
    writeFile: (relativePath: string, content: string) => Promise<boolean>;
    readFile: (relativePath: string) => Promise<string | null>;
    deleteFile: (relativePath: string) => Promise<boolean>;
}
