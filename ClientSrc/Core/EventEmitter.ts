namespace App {
    export type EventHandler<T = unknown> = (data: T) => void;

    export class EventEmitter {
        private readonly events = new Map<string, Set<EventHandler>>();

        public on<T>(event: string, handler: EventHandler<T>): void {
            let handlers = this.events.get(event);

            if (!handlers) {
                handlers = new Set<EventHandler>();
                this.events.set(event, handlers);
            }

            handlers.add(handler as EventHandler);
        }

        public off<T>(event: string, handler: EventHandler<T>): void {
            const handlers = this.events.get(event);

            if (!handlers) {
                return;
            }

            handlers.delete(handler as EventHandler);

            if (handlers.size === 0) {
                this.events.delete(event);
            }
        }

        public emit<T>(event: string, data: T): void {
            const handlers = this.events.get(event);

            if (!handlers) {
                return;
            }

            handlers.forEach(handler => {
                handler(data);
            });
        }

        public removeAllListeners(event?: string): void {
            if (event) {
                this.events.delete(event);
                return;
            }

            this.events.clear();
        }
    }
}