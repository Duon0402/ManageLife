namespace App {
    interface CronJobModel {
        jobId: string;
        title: string;
        enabled: boolean;
        url: string;
    }

    export class AdminCronJobPage extends BasePage {
        private gridBuilder: GridBuilder<CronJobModel>;

        protected initialize(): void {
            this.initGrid();
        }

        private initGrid(): void {
            this.gridBuilder = new GridBuilder<CronJobModel>('#cronJobsTable')
                .setDataSource({
                    url: '/Admin/CronJob/GetListCronJobs'
                })
                .addColumn({ field: 'jobId', title: 'Job ID' })
                .addColumn({ field: 'title', title: 'Tiêu đề' })
                .addColumn(new GridColumnBuilder<CronJobModel>('enabled', 'Trạng thái')
                    .render((data) => data ? 'Hoạt động' : 'Tạm dừng')
                )
                .addColumn({ field: 'url', title: 'URL' })
                .setOptions({
                    paging: false,
                    info: false,
                    searching: true,
                    autoWidth: false
                });

            this.gridBuilder.build();
        }
    }
}
