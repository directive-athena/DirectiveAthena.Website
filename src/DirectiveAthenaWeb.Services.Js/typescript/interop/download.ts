export const registerDownload = (): void => {
    window.downloadFile = (fileName: string, content: string): void => {
        const blob = new Blob([content], {type: "text/plain"});
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        window.URL.revokeObjectURL(url);
    };
};
