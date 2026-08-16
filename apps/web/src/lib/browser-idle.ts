type IdleWindow = Window & {
  requestIdleCallback?: (callback: () => void, options?: { timeout: number }) => number;
  cancelIdleCallback?: (handle: number) => void;
};

export function scheduleWhenIdle(callback: () => void, timeout = 1500) {
  const browser = window as IdleWindow;

  if (browser.requestIdleCallback && browser.cancelIdleCallback) {
    const handle = browser.requestIdleCallback(callback, { timeout });
    return () => browser.cancelIdleCallback?.(handle);
  }

  const handle = window.setTimeout(callback, Math.min(timeout, 250));
  return () => window.clearTimeout(handle);
}
