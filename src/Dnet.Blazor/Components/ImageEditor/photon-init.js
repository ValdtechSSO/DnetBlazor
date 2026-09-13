// Photon WASM initialization module.
//
// Loading is lazy on purpose: the module and its 1.8 MB wasm payload are only
// fetched when an operation actually needs a resample, never when the editor
// merely opens. Nothing in the UI blocks on it.

let photonModule = null;
let photonPromise = null;

/**
 * Initialize Photon WASM module.
 * @returns {Promise} Promise that resolves when Photon is loaded.
 */
async function initPhoton() {
    if (photonModule) {
        return photonModule;
    }

    if (photonPromise) {
        return photonPromise;
    }

    photonPromise = (async () => {
        try {
            const basePath = document.baseURI || window.location.origin + '/';
            const modulePath = new URL('photon_rs.js', basePath).href;

            const module = await import(/* webpackIgnore: true */ modulePath);

            const wasmPath = new URL('photon_rs_bg.wasm', basePath).href;
            await module.default({ module_or_path: wasmPath });

            photonModule = module;

            return module;
        } catch (error) {
            console.error('Dnet.Blazor: Photon WASM could not be initialized.', error);
            photonPromise = null;
            throw error;
        }
    })();

    return photonPromise;
}

/**
 * Get the initialized Photon module, loading it on first use.
 * @returns {Promise} Promise that resolves to the Photon module.
 */
async function getPhoton() {
    if (!photonModule) {
        await initPhoton();
    }

    return photonModule;
}

/**
 * Check whether Photon is already initialized.
 * @returns {boolean} True when Photon is ready.
 */
function isPhotonReady() {
    return photonModule !== null;
}

window.photonInit = {
    init: initPhoton,
    get: getPhoton,
    isReady: isPhotonReady
};
