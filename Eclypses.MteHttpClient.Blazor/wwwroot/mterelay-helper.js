// ************************************** START stuff for MTE

import {
    MteWasm,
    MteBase,
    MteStatus,
    MteSdrStorage,
    MteMkeEnc,
    MteMkeDec,
} from "../../Mte.js";

/**
 * Declare some variables that are global to this module.
 */
let mteWasm = null;
let mteBase = null;
let mteSessionSdr = null;

// *************************************** END stuff for MTE



// *************************************** START stuff for ECDH
const algorithm = {
    name: "ECDH",
    namedCurve: "P-256",
};

export async function makeKeyPair() {
    const keys = await window.crypto.subtle.generateKey(algorithm, true, [
        "deriveBits",
        "deriveKey",
    ]);

    const publicKeyData = await window.crypto.subtle.exportKey(
        "raw",
        keys.publicKey
    );

    const privateKeyData = await window.crypto.subtle.exportKey(
        "pkcs8",
        keys.privateKey
    );

    var publicKey = arrayBufferToBase64(publicKeyData);
    var privateKey = arrayBufferToBase64(privateKeyData);

    return { publicKey: publicKey, privateKey: privateKey, rc: 0 }

}

export async function makeSecret(peerPublicKey, privateKey) {

    const privateKeyData = await window.crypto.subtle.importKey(
        'pkcs8',
        new Uint8Array(base64ToArrayBuffer(privateKey)),
        algorithm,
        true,
        ['deriveKey', 'deriveBits'],
    );

    const publicKey = await window.crypto.subtle.importKey(
        'raw',
        peerPublicKey,
        algorithm,
        true,
        [],
    );

    const secret = await window.crypto.subtle.deriveBits(
        { name: 'ECDH', public: publicKey },
        privateKeyData,
        256,
    );
    return arrayBufferToBase64(secret);
}
// *************************************** END stuff for ECDH

// ************************************** START stuff for MTE

/**
 * An asynchronous function that instantiates MteWasm, then sets up MteBase for future use.
 * This MUST be called before any other MTE methods can be used, usually as soon as the website loads.
 */
export async function instantiateMteWasm() {
    // assign mteWasm variable, and instantiate wasm
    mteWasm = new MteWasm();
    await mteWasm.instantiate();
    // assign mteBase variable
    mteBase = new MteBase(mteWasm);
    return true;
}

export function initLicense(licensedCompany, licenseKey) {
    if (!mteBase.initLicense(licensedCompany, licenseKey)) {
        const licenseStatus = MteStatus.mte_status_license_error;
        const status = mteBase.getStatusName(licenseStatus);
        const message = mteBase.getStatusDescription(licenseStatus);
        throw new Error(`Error with MTE License.\n${status}: ${message}`);
    }
}


export function makeAnEmptyEncoder() {
    return MteMkeEnc.fromdefault(mteWasm);
}

export function makeAnEmptyDecoder() {
    return MteMkeDec.fromdefault(mteWasm, 1000, -63);
}

export function initializeEncoder(
    mteEncoder,
    encoderEntropy,
    nonce,
    personalization
) {
    mteEncoder.setEntropyArr(encoderEntropy);
    mteEncoder.setNonce(nonce);
    const encoderStatus = mteEncoder.instantiate(personalization);
    return encoderStatus;
}

export function initializeDecoder(
    mteDecoder,
    decoderEntropy,
    nonce,
    personalization
) {
    mteDecoder.setEntropyArr(decoderEntropy);
    mteDecoder.setNonce(nonce);
    const decoderStatus = mteDecoder.instantiate(personalization);
    return decoderStatus;
}

export function retrieveEncoderState(mteEncoder) {
    return mteEncoder.saveStateB64();
}

export function retrieveDecoderState(mteDecoder) {
    return mteDecoder.saveStateB64();
}

export function restoreDecoderState(mteDecoder, mteState) {
    return mteDecoder.restoreStateB64(mteState);
}

export function restoreEncoderState(mteEncoder, mteState) {
    return mteEncoder.restoreStateB64(mteState);
}

export function decodeToString(mteDecoder, payload) {
    var ret = mteDecoder.decodeStrB64(payload);
    if (ret.status != MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(ret.status);
        const message = mteBase.getStatusDescription(ret.status);
        throw new Error(`Error decoding string.\n${status}: ${message}`);
    }
    return ret.str;
}

export function decodeToByteArray(mteDecoder, payload) {
    var ret = mteDecoder.decode(payload);
    if (ret.status != MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(ret.status);
        const message = mteBase.getStatusDescription(ret.status);
        throw new Error(`Error decoding byte array.\n${status}: ${message}`);
    }
    return ret.arr;
}

export function encodeToString(mteEncoder, payload) {
    var ret = mteEncoder.encodeStrB64(payload);
    if (ret.status != MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(ret.status);
        const message = mteBase.getStatusDescription(ret.status);
        throw new Error(`Error encoding string.\n${status}: ${message}`);
    }
    return ret;
}

export function encode(mteEncoder, payload) {
    var ret = mteEncoder.encode(payload);
    if (ret.status != MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(ret.status);
        const message = mteBase.getStatusDescription(ret.status);
        throw new Error(`Error encoding payload.\n${status}: ${message}`);
    }
    var ret2 = { status: ret.status, str: arrayBufferToBase64(ret.arr) };

    return ret2;
}



// ************************************** END stuff for MTE

// ************************************** START stuff for SDR

/**
 * Returns a base-64 encoded string
 * of a byte array consisting of
 * cryptographically secure random
 * values.
 * @param {any} size The size of the byte array you wish to receive.
 */
export async function getEntropy(size) {
    var entropy = new Uint8Array(size);
    window.crypto.getRandomValues(entropy);
    var value = btoa(entropy);
    return value;
}

/**
 * Initializes a local MteSdr
 * to use for securely storing and retrieving
 * data items from the browser storage.
 * @param {any} category This allows for specifying what kind of data you with to manage.
 * @param {any} entropy This is a byte array of secure values to use for entropy.
 * @param {any} nonce This is a ulong used to instantiate the MTE.
 * @param {any} persist If this is true, the data is stored in LocalStorage, otherwise it is kept in Session Storage.
 */
function initializeSdr(category, entropy, nonce, persist) {
    const mteSdr = MteSdrStorage.fromdefault(mteWasm, category, persist);
    mteSdr.initSdr(entropy, nonce);
    return mteSdr;
}

/**
 * Initializes a SessionStorage SDR for data only kept within this browser session.
 * @param {any} category This allows for specifying what kind of data you with to manage.
 * @param {any} entropy This is a byte array of secure values to use for entropy.
 * @param {any} nonce This is a ulong used to instantiate the MTE.
 */
export async function initializeSessionSdr(category, entropy, nonce) {
    mteSessionSdr = initializeSdr(category, entropy, nonce, false);
}

/**
 * Retrieves, reveals, and returns the requested data item.
 * @param {any} name The name associated with the item you with to retrieve.
 */
export async function read(name) {
    try {
        return mteSessionSdr.readString(name);
    } catch (err) {
        return null;
    }
}

/**
 * Conceals and stores the requested data item.
 * @param {any} name The name associated with the item you with to store.
 * @param {any} data A string of data you wish to store in session storage.
 */
export async function write(name, data) {
    mteSessionSdr.writeString(name, data);
}

/**
 * Removes the requested data item from storage.
 * @param {any} name The name associated with the item you with to remove.
 */
export async function remove(name) {
    mteSessionSdr.remove(name);
}

// ************************************** END stuff for SDR

// *************************************** START support methods

/**
 * Utility to convert a array buffer to a base64 string.
 * @param buffer A typed array.
 * @returns A base64 encoded string of the original array buffer data.
 */
function arrayBufferToBase64(buffer) {
    return btoa(String.fromCharCode.apply(null, new Uint8Array(buffer)));
}

/**
 * Utility to convert a base64 string into an array buffer.
 * @param base64Str A base64 encoded string.
 * @returns An array buffer.
 */
function base64ToArrayBuffer(base64Str) {
    const str = window.atob(base64Str);
    let i = 0;
    const iMax = str.length;
    const bytes = new Uint8Array(iMax);
    for (; i < iMax; ++i) {
        bytes[i] = str.charCodeAt(i);
    }
    return bytes.buffer;
}
// *************************************** END support methods