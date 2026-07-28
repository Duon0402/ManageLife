namespace App {
    interface NoteTagModel { id: string; name: string; color: string; }
    interface NoteGraphNodeData { id: string; label: string; tagIds: string[]; linkCount: number; }
    interface NoteGraphEdgeData { source: string; target: string; }
    interface NoteGraphModel { nodes: NoteGraphNodeData[]; edges: NoteGraphEdgeData[]; }

    export class NoteGraphPage extends BasePage {
        private cy: any;
        private tags: NoteTagModel[] = [];
        private tagColorMap: Record<string, string> = {};

        protected initialize(): void {
            this.loadGraph();
        }

        protected bindEvents(): void {
            this.root.find('#btn-fit').on('click', () => this.cy?.fit(undefined, 40));
            this.root.find('#btn-zoom-in').on('click', () => this.cy?.zoom({ level: (this.cy.zoom() * 1.2), renderedPosition: { x: this.cy.width() / 2, y: this.cy.height() / 2 } }));
            this.root.find('#btn-zoom-out').on('click', () => this.cy?.zoom({ level: (this.cy.zoom() * 0.8), renderedPosition: { x: this.cy.width() / 2, y: this.cy.height() / 2 } }));
            this.root.find('#ng-layout').on('change', (e) => {
                const layout = $(e.currentTarget).val() as string;
                this.cy?.layout({ name: layout, animate: true, animationDuration: 400 }).run();
            });
        }

        private async loadGraph(): Promise<void> {
            LoadingService.show();
            try {
                const [graphRes, tagsRes] = await Promise.all([
                    ApiService.get('/Note/GetGraphData'),
                    ApiService.get('/Note/GetTags')
                ]);

                this.tags = tagsRes.isOk() ? tagsRes.data || [] : [];
                this.tags.forEach(t => { this.tagColorMap[t.id] = t.color; });

                if (!graphRes.isOk()) { this.showEmpty(); return; }

                const graph: NoteGraphModel = graphRes.data;
                if (!graph.nodes.length) { this.showEmpty(); return; }

                this.renderLegend();
                this.initCytoscape(graph);
            } catch {
                ToastService.error('Lỗi hệ thống');
                this.showEmpty();
            } finally {
                LoadingService.hide();
            }
        }

        private initCytoscape(graph: NoteGraphModel): void {
            const cytoscape = (window as any).cytoscape;
            if (!cytoscape) { console.error('Cytoscape not loaded'); return; }

            const nodes = graph.nodes.map(n => ({
                data: {
                    id: n.id,
                    label: n.label,
                    color: this.getNodeColor(n.tagIds),
                    size: Math.max(30, Math.min(70, 30 + n.linkCount * 8))
                }
            }));

            const edges = graph.edges.map((e, i) => ({
                data: { id: `e${i}`, source: e.source, target: e.target }
            }));

            this.cy = cytoscape({
                container: document.getElementById('cy-container'),
                elements: { nodes, edges },
                style: [
                    {
                        selector: 'node',
                        style: {
                            'background-color': 'data(color)',
                            'label': 'data(label)',
                            'width': 'data(size)',
                            'height': 'data(size)',
                            'font-size': '11px',
                            'font-family': 'Nunito, sans-serif',
                            'font-weight': '600',
                            'color': '#2c2c54',
                            'text-valign': 'bottom',
                            'text-halign': 'center',
                            'text-margin-y': '4px',
                            'border-width': '2px',
                            'border-color': '#fff',
                            'border-opacity': 0.8,
                            'text-max-width': '100px',
                            'text-wrap': 'ellipsis',
                            'overlay-padding': '4px'
                        }
                    },
                    {
                        selector: 'node:hover',
                        style: {
                            'border-width': '3px',
                            'border-color': '#4b49ac',
                            'cursor': 'pointer'
                        }
                    },
                    {
                        selector: 'node:selected',
                        style: {
                            'border-width': '3px',
                            'border-color': '#4b49ac'
                        }
                    },
                    {
                        selector: 'edge',
                        style: {
                            'width': 1.5,
                            'line-color': '#d6d5f0',
                            'target-arrow-color': '#d6d5f0',
                            'target-arrow-shape': 'triangle',
                            'curve-style': 'bezier',
                            'arrow-scale': 0.8,
                            'opacity': 0.7
                        }
                    },
                    {
                        selector: 'edge:hover',
                        style: { 'line-color': '#4b49ac', 'target-arrow-color': '#4b49ac', 'opacity': 1 }
                    }
                ],
                layout: { name: 'cose', animate: true, animationDuration: 600, nodeRepulsion: () => 8000, idealEdgeLength: () => 120, nodeDimensionsIncludeLabels: true },
                wheelSensitivity: 0.3
            });

            this.bindCytoscapeEvents();
        }

        private bindCytoscapeEvents(): void {
            const $tooltip = $('#ng-tooltip');

            this.cy.on('mouseover', 'node', (e: any) => {
                const node = e.target;
                const pos = node.renderedPosition();
                $tooltip
                    .text(node.data('label'))
                    .css({ left: pos.x + 10, top: pos.y - 20 })
                    .removeClass('d-none');
            });

            this.cy.on('mouseout', 'node', () => $tooltip.addClass('d-none'));

            this.cy.on('tap', 'node', (e: any) => {
                const id = e.target.id();
                window.location.href = `/Note/Edit?id=${id}`;
            });

            this.cy.on('tap', (e: any) => {
                if (e.target === this.cy) $tooltip.addClass('d-none');
            });
        }

        private getNodeColor(tagIds: string[]): string {
            if (tagIds.length && this.tagColorMap[tagIds[0]])
                return this.tagColorMap[tagIds[0]];
            return '#7978d9';
        }

        private renderLegend(): void {
            const $legend = $('#ng-legend');
            $legend.empty();
            $legend.append(`<div class="ng-legend-item"><div class="ng-legend-dot" style="background:#7978d9"></div><span>Không có tag</span></div>`);
            this.tags.slice(0, 6).forEach(tag => {
                $legend.append(`<div class="ng-legend-item"><div class="ng-legend-dot" style="background:${tag.color}"></div><span>${tag.name}</span></div>`);
            });
        }

        private showEmpty(): void {
            this.root.find('#cy-container').addClass('d-none');
            this.root.find('#ng-empty').removeClass('d-none');
        }
    }
}
