
var d = {
    JobName: '',
    JobGroup: '',
    StartDate: '',
    EndDate: '',
    colHeaders: [
        { name: 'JobName', index: 'JobName', label: 'Job Name', sortable: true },
        { name: 'JobGroup', index: 'JobGroup', label: 'Job Group', sortable: true },
        { name: 'StartTime', index: 'StartTime', label: 'Start Time' },
        { name: 'EndTime', index: 'EndTime', label: 'End Time' },
        { name: 'ElapsedTime', index: 'ElapsedTime', label: 'Elapsed Time', sortable: false },
        { name: 'Interval', index: 'Interval', label: 'Interval' },
        { name: 'CPU', index: 'CPU', label: 'CPU(s)' },
        { name: 'Memory', index: 'Memory', label: 'Memory(MB)' },
        { name: 'CreateTime', index: 'CreateTime', label: 'Create Time' }
    ],
    sortKey: '',
    sortOrders: {},
    items: [],
    pageCurrent: 1,
    pageSize: 10,
    pageSizeList: [10, 20, 50],
    totalCount: 0
};

var vm = new Vue({
    el: '#container',
    mounted: function () {
        this.initDateTimePicker();
        this.searchJobs();
    },
    data: function () {
        var that = this;
        d.colHeaders.forEach(function (ch) {
            if (that.changeSortable(ch.sortable)) {
                d.sortOrders[ch.name] = 1;
            }
        });
        return d;
    },
    methods: {
        initDateTimePicker: function () {
            var that = this;
            $('#dtStart').datetimepicker({
                format: 'YYYY-MM-DD'
            });

            $('#dtEnd').datetimepicker({
                format: 'YYYY-MM-DD',
                useCurrent: false //Important! See issue #1075
            });

            $("#dtStart").on("dp.change", function (e) {
                that.StartDate = $('input', $('#dtStart')).val();
                $('#dtEnd').data("DateTimePicker").minDate(e.date);
            });

            $("#dtEnd").on("dp.change", function (e) {
                that.EndDate = $('input', $('#dtEnd')).val();
                $('#dtStart').data("DateTimePicker").maxDate(e.date);
            });
        },
        pageChanged: function (page) {
            this.pageCurrent = page.pageCurrent;
            this.pageSize = page.pageSize;
            this.bindData();
        },
        pageSizeChanged: function (page) {
            this.pageCurrent = page.pageCurrent;
            this.pageSize = page.pageSize;
            this.bindData();
        },
        changeSortable: function (value) {
            return value == true || value == undefined;
        },
        sortBy: function (ch) {
            if (this.changeSortable(ch.sortable)) {
                var key = ch.name;
                this.sortKey = key;
                this.sortOrders[key] = this.sortOrders[key] * -1;
                this.bindData();
            }
        },
        bindData: function () {
            var that = this;
            $.ajax({
                url: '/Performance/GetPerformanceList/',
                data: this.getParameters(),
                type: 'GET',
                dataType: 'json',
                success: function (result) {
                    that.items = result.Items;
                    that.totalCount = result.TotalCount;
                }
            });
        },
        getParameters: function () {
            return {
                PageCurrent: this.pageCurrent,
                PageSize: this.pageSize,
                SortKey: this.sortKey,
                SortType: this.sortType,
                JobName: this.JobName,
                JobGroup: this.JobGroup,
                StartDate: this.StartDate,
                EndDate: this.EndDate
            }
        },
        searchJobs: function (event) {
            var that = this;
            this.$nextTick(() => {
                that.pageCurrent = 1;
                that.bindData();
                if (event) event.preventDefault();
            });
        }
    },
    computed: {
        sortType: function () {
            return this.sortOrders[this.sortKey] > 0 ? 'asc' : 'desc';
        }
    }
});