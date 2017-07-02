﻿
var d = {
    JobName: '',
    StartDate: '',
    EndDate: '',
    colHeaders: [
        { name: 'JobName', index: 'JobName', label: 'Job Name', sortable: true },
        { name: 'Status', index: 'Status', label: 'Status' },
        { name: 'AssemblyName', index: 'AssemblyName', label: 'Assembly Name' },
        { name: 'ClassName', index: 'ClassName', label: 'Class Name' },
        { name: 'CronExpression', index: 'CronExpression', label: 'Cron Expression' },
        { name: 'StartTime', index: 'StartTime', label: 'Start Time' },
        { name: 'EndTime', index: 'EndTime', label: 'End Time' }
    ],
    sortKey: '',
    sortOrders: {},
    items: [],
    pageCurrent: 1,
    pageSize: 10,
    pageSizeList: [10, 20, 50],
    totalCount: 0,
    showModal: false,
    Model: {
        ID: '',
        JobName: '',
        AssemblyName: '',
        ClassName: '',
        CronExpression: '',
        StartTimeString: '',
        EndTimeString: ''
    },
    valid: false,
    validation: {
        JobName: null,
        AssemblyName: null,
        ClassName: null,
        CronExpression: null
    }
};

var options = {
    inputClasses: {
        valid: 'form-control-success',
        invalid: 'form-control-danger'
    }
}

var vm = new Vue({
    el: '#container',
    mounted: function () {
        this.initDateTimePicker();
        this.initEditDateTimePicker();
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
        initEditDateTimePicker: function () {
            var that = this;
            $('#dtModelStartTime').datetimepicker({
                format: 'YYYY-MM-DD'
            });
            $('#dtModelEndTime').datetimepicker({
                format: 'YYYY-MM-DD',
                useCurrent: false //Important! See issue #1075
            });
            $("#dtModelStartTime").on("dp.change", function (e) {
                that.Model.StartTimeString = $('input', $('#dtModelStartTime')).val();
                $('#dtModelEndTime').data("DateTimePicker").minDate(e.date);
            });
            $("#dtModelEndTime").on("dp.change", function (e) {
                that.Model.EndTimeString = $('input', $('#dtModelEndTime')).val();
                $('#dtModelStartTime').data("DateTimePicker").maxDate(e.date);
            });
        },
        beRequired: function (field) {
            var tip = this.Model[field] ? '' : field + ' is required.';
            this.$set(this.validation, field, tip);
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
                url: '/Job/GetJobList/',
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
        },
        startJob: function (id, i, event) {
            this.$nextTick(() => {
                var that = this;
                $.ajax({
                    url: '/Job/Start/',
                    data: { "ID": id },
                    type: 'POST',
                    dataType: 'json',
                    success: function (result) {
                        if (result && result.success) {
                            that.items[i].Status = 1;
                        }
                    }
                });
                event.preventDefault();
            });
        },
        stopJob: function (id, i, event) {
            this.$nextTick(() => {
                var that = this;
                $.ajax({
                    url: '/Job/Stop/',
                    data: { "ID": id },
                    type: 'POST',
                    dataType: 'json',
                    success: function (result) {
                        if (result && result.success) {
                            that.items[i].Status = 0;
                        }
                    }
                });
                event.preventDefault();
            });
        },
        AddJob: function () {
            this.showModal = true;
        },
        SaveJob: function (event) {
            if (this.valid) {
                var that = this;
                var operation = that.Model.ID != '' ? 'Edit' : 'Add';
                this.$nextTick(() => {
                    $.post('/Job/' + operation + '/', that.Model, function (data) {
                        if (data && data.success) {
                            that.searchJobs();
                        }
                        that.showModal = false;
                    });
                    if (event) event.preventDefault();
                });
            }
        },
        deleteJob: function (id, event) {
            this.$nextTick(() => {
                var that = this;
                BootstrapDialog.confirm('Are you sure to delete this job?', function (result) {
                    if (result) {
                        $.post('/Job/Delete/', { "ID": id }, function (data) {
                            if (data) {
                                if (data.success === true) {
                                    that.searchJobs();
                                }
                                else {
                                    BootstrapDialog.show({
                                        title: 'Delete Job Error',
                                        message: data.message.toString()
                                    });
                                }
                            }

                        });
                    }
                });
            });
        },
        editJob: function (id, event) {
            this.$nextTick(() => {
                var that = this;
                $.get('/Job/Get/', { "ID": id }, function (data) {
                    if (data && data.success === true) {
                        that.Model = data.model;
                        that.showModal = true;
                    }
                });
                event.preventDefault();
            });
        },
        resetAddOrEdit: function () {
            setTimeout(() => {
                this.Model.JobName = '';
                this.Model.AssemblyName = '';
                this.Model.ClassName = '';
                this.Model.CronExpression = '';
                this.Model.StartTimeString = '';
                this.Model.EndTimeString = '';
                this.Model.Status = '0';
                this.validation.JobName = null;
                this.validation.AssemblyName = null;
                this.validation.ClassName = null;
                this.validation.CronExpression = null;
                this.Model.ID = '';
            }, 200);
        }
    },
    computed: {
        sortType: function () {
            return this.sortOrders[this.sortKey] > 0 ? 'asc' : 'desc';
        }
    },
    watch: {
        validation: {
            handler: function (val, oldVal) {
                console.log(val, oldVal);
                for (var i in val) {
                    if (val[i] === null || val[i] != '') {
                        this.valid = false;
                        return;
                    }
                }
                this.valid = true;
            },
            deep: true
        },
        showModal: function (val, oldVal) {
            if (val === false) {
                this.resetAddOrEdit();
            }
        },
        StartDate: function (val, oldVal) {

        }
    }
});