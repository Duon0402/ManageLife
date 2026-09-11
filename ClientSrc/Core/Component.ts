namespace App {
    export abstract class Component {
        protected readonly element: JQuery<HTMLElement>;

        private readonly eventEmitter = new EventEmitter();

        private mounted = false;
        private destroyed = false;

        constructor(element: JQuery<HTMLElement>) {
            this.element = element;
        }

        public mount(): void {
            if (this.destroyed) {
                throw new Error("Cannot mount a destroyed component.");
            }

            if (this.mounted) {
                return;
            }

            this.onMount();

            this.mounted = true;
        }

        public destroy(): void {
            if (this.destroyed) {
                return;
            }

            this.onDestroy();

            this.eventEmitter.removeAllListeners();

            this.mounted = false;
            this.destroyed = true;
        }

        public on<T>(event: string, handler: EventHandler<T>): void {
            this.eventEmitter.on(event, handler);
        }

        public off<T>(event: string, handler: EventHandler<T>): void {
            this.eventEmitter.off(event, handler);
        }

        protected emit<T>(event: string, data: T): void {
            this.eventEmitter.emit(event, data);
        }

        protected onMount(): void { }

        protected onDestroy(): void { }

        public get isMounted(): boolean {
            return this.mounted;
        }

        public get isDestroyed(): boolean {
            return this.destroyed;
        }
    }
}