export const registerClipboard = (): void => {
    window.copyToClipboard = async (text: string): Promise<boolean> => {
        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(text);
                return true;
            }
        } catch (error) {
            console.warn("Clipboard API copy failed, falling back.", error);
        }

        try {
            const textarea = document.createElement("textarea");
            textarea.value = text;
            textarea.setAttribute("readonly", "");
            textarea.style.position = "absolute";
            textarea.style.left = "-9999px";
            document.body.appendChild(textarea);
            textarea.select();
            const success = document.execCommand("copy");
            textarea.remove();
            return success;
        } catch (error) {
            console.error("Clipboard fallback copy failed.", error);
            return false;
        }
    };
};
