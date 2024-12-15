import { useState } from "react";

export const useWakeLock = () => {

    const [hasLock, setHasLock] = useState(false);
    const [lock, setLock] = useState<WakeLockSentinel>();

    const acquireWakeLock = async () => {
        try {
            if(hasLock) {
                return true;
            }

            if (!("wakeLock" in navigator)) {
                return false;
            }

            const lock = await navigator.wakeLock.request("screen");
            setLock(lock);
            setHasLock(true);

            return true;

        } catch (err) {
            console.warn("Unable to acquire wake lock", err);
            return false;
        }
    }

    const releaseWakeLock = async () => {
        if(!hasLock || !lock) {
            return;
        }

        if (!("wakeLock" in navigator)) {
            return;
        }

        await lock.release();

        setLock(undefined);
        setHasLock(false);
    }
    
    return {
        acquireWakeLock,
        releaseWakeLock,
        hasWakeLock: hasLock
    };
}