declare namespace bootstrap {
    class Toast {
        constructor(element: Element, options?: any);
        show(): void;
        hide(): void;
        dispose(): void;
        static getInstance(element: Element): Toast | null;
        static getOrCreateInstance(element: Element, options?: any): Toast;
    }
}
