import {registerClipboard} from "./interop/clipboard";
import {registerDownload} from "./interop/download";
import {registerFsApi} from "./interop/fsApi";
import {initLoadingObserver} from "./interop/loadingObserver";
import {registerScrollFunctions} from "./interop/scroll";

initLoadingObserver();
registerScrollFunctions();
registerClipboard();
registerDownload();
registerFsApi();
