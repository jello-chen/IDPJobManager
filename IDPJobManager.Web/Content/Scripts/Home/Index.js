var vm = new Vue({
    el: '#container',
    mounted: function () {
        this.bindChart();
    },
    data: function () {
        return {};
    },
    methods: {
        bindChart: function () {
            $.ajax({
                url: '/Home/GetJobPerformanceTrend',
                type: 'GET',
                dataType: 'json',
                success: function (result) {
                    Morris.Area({
                        element: 'job_performance_chart',
                        data: result.data,
                        xkey: 'period',
                        ykeys: result.ykeys,
                        labels: result.ykeys,
                        pointSize: 2,
                        hideHover: 'auto',
                        resize: true
                    });
                }
            });
        }
    }
});