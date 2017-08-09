function initializeUploader() {
    $("#job-uploader").fileinput({
        uploadUrl: "/Job/Upload",
        uploadAsync: true,
        minFileCount: 1,
        maxFileCount: 1,
        allowedFileExtensions: ["rar"],
        elErrorContainer: '#kv-error-1'
    }).on('filebatchpreupload', function (event, data, id, index) {
        $('#kv-success-1').html('<h4>Upload Status</h4><ul></ul>').hide();
    }).on('fileuploaded', function (event, data, id, index) {
        var status = data.response.success ? 'successfully' : 'failed';
        var fname = data.files[index].name,
            out = '<li>' + 'Uploaded file # ' + (index + 1) + ' - ' +
                fname + ' ' + status + '.' + '</li>';
        $('#kv-success-1 ul').append(out);
        $('#kv-success-1').fadeIn('slow');
    });
}

var d = {
    IsCheckAll: false,
    JobName: '',
    JobGroup: '',
    colHeaders: [
        { name: 'JobName', index: 'JobName', label: 'Job Name', sortable: true },
        { name: 'JobGroup', index: 'JobGroup', label: 'Job Group', sortable: true },
        { name: 'Status', index: 'Status', label: 'Status' },
        { name: 'AssemblyName', index: 'AssemblyName', label: 'Assembly Name' },
        { name: 'ClassName', index: 'ClassName', label: 'Class Name' },
        { name: 'CronExpression', index: 'CronExpression', label: 'Cron Expression' }
    ],
    sortKey: '',
    sortOrders: {},
    items: [],
    pageCurrent: 1,
    pageSize: 10,
    pageSizeList: [10, 20, 50],
    totalCount: 0,
    showAddOrEditModel: false,
    showConfigModel: false,
    showUploader: false,
    currentConfigJobID: '',
    dependentJobList: [],
    dependableJobList: [],
    selectedDependableJobList: [],
    addedDependableJobList: [],
    Model: {
        ID: '',
        JobName: '',
        JobGroup: '',
        AssemblyName: '',
        ClassName: '',
        CronExpression: ''
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
        initializeUploader();
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
        beRequired: function (field) {
            var tip = this.Model[field] ? '' : field + ' is required.';
            this.$set(this.validation, field, tip);
        },
        validate: function () {
            var validatingProperties = ["JobName", "AssemblyName", "ClassName", "CronExpression"];
            for (var i = 0; i < validatingProperties.length; i++) {
                this.beRequired(validatingProperties[i]);
            }
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
                    var vItems = result.Items;
                    vItems.forEach(function (item) {
                        item.Checked = false;
                    });
                    that.items = result.Items;
                    that.totalCount = result.TotalCount;
                    that.IsCheckAll = false;
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
                JobGroup: this.JobGroup
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
            this.showAddOrEditModel = true;
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
                        that.showAddOrEditModel = false;
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
                        that.showAddOrEditModel = true;
                    }
                });
                event.preventDefault();
            });
        },
        resetAddOrEdit: function () {
            setTimeout(() => {
                this.Model.JobName = '';
                this.Model.JobGroup = '';
                this.Model.AssemblyName = '';
                this.Model.ClassName = '';
                this.Model.CronExpression = '';
                this.Model.Status = '0';
                this.validation.JobName = null;
                this.validation.AssemblyName = null;
                this.validation.ClassName = null;
                this.validation.CronExpression = null;
                this.Model.ID = '';
            }, 200);
        },
        configurateDependency(id, event) {
            this.$nextTick(() => {
                var that = this;
                $.get('/Job/GetJobDependency/', { "ID": id }, function (data) {
                    if (data) {
                        that.currentConfigJobID = id;
                        that.dependableJobList = data.DependableJobList;
                        that.dependentJobList = data.DependentJobList;
                        that.showConfigModel = true;
                    }
                });
                event.preventDefault();
            });
        },
        SaveJobDependency(event) {
            this.$nextTick(() => {
                var that = this;
                $.ajax({
                    type: 'POST',
                    dataType: 'json',
                    url: '/Job/SaveJobDependency/',
                    data: JSON.stringify({ "ID": this.currentConfigJobID, "DependentJobIDs": this.dependentJobList.map(j => j.ID) }),
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        console.debug(data);
                        if (data && data.success) {
                            that.showConfigModel = false;
                        }
                    },
                    error: function (data) {
                        console.debug(data);
                    }
                });
                event.preventDefault();
            })
        },
        addJobDependency(index) {
            var item = this.dependableJobList[index];
            Vue.delete(this.dependableJobList, index);
            this.dependentJobList.push(item);
        },
        deleteJobDependency(index) {
            var item = this.dependentJobList[index];
            Vue.delete(this.dependentJobList, index);
            this.dependableJobList.push(item);
        },
        addMultiJobDependency(list, isAll) {
            var mList = list || [];
            if (mList.length == 0) return;
            for (var i = 0, flag = true; i < this.dependableJobList.length; flag ? i++ : i) {
                for (var j = 0; j < list.length; j++) {
                    if (!isAll && this.dependableJobList[i].ID === list[j] || isAll && this.dependableJobList[i] === list[j]) {
                        this.dependentJobList.push(this.dependableJobList.splice(i, 1)[0]);
                        flag = false;
                        break;
                    }
                    else {
                        flag = true;
                    }
                }
            }
        },
        deleteMultiJobDependency(list, isAll) {
            var mList = list || [];
            if (mList.length == 0) return;
            for (var i = 0, flag = true; i < this.dependentJobList.length; flag ? i++ : i) {
                for (var j = 0; j < list.length; j++) {
                    if (!isAll && this.dependentJobList[i].ID === list[j] || isAll && this.dependentJobList[i] === list[j]) {
                        this.dependableJobList.push(this.dependentJobList.splice(i, 1)[0]);
                        flag = false;
                        break;
                    }
                    else {
                        flag = true;
                    }
                }
            }
        },
        getSelectedItemIds: function () {
            var ids = [];
            this.items.forEach(function (item) {
                if (item.Checked) {
                    ids.push(item.ID);
                }
            });
            return ids;
        },
        StartSelectedJob: function (event) {
            var ids = this.getSelectedItemIds();
            if (ids.length == 0) {
                BootstrapDialog.alert('Nothing is checked!');
            }
            else {
                var _ids = '';
                ids.forEach(function (id) {
                    _ids += id + ',';
                });
                _ids = _ids.substr(0, _ids.length - 1);
                this.$nextTick(() => {
                    var that = this;
                    $.ajax({
                        url: '/Job/BatchOperate/',
                        data: { "IDS": _ids, "Type": "Start" },
                        type: 'POST',
                        dataType: 'json',
                        success: function (result) {
                            if (result && result.success) {
                                ids.forEach(function (id) {
                                    for (var i = 0; i < that.items.length; i++) {
                                        var item = that.items[i];
                                        if (id == item.ID) {
                                            item.Status = 1;
                                            break;
                                        }
                                    }
                                });
                            }
                        }
                    });
                    event.preventDefault();
                });
            }
        },
        StopSelectedJob: function (event) {
            var ids = this.getSelectedItemIds();
            if (ids.length == 0) {
                BootstrapDialog.alert('Nothing is checked!');
            }
            else {
                var _ids = '';
                ids.forEach(function (id) {
                    _ids += id + ',';
                });
                _ids = _ids.substr(0, _ids.length - 1);
                this.$nextTick(() => {
                    var that = this;
                    $.ajax({
                        url: '/Job/BatchOperate/',
                        data: { "IDS": _ids, "Type": "Stop" },
                        type: 'POST',
                        dataType: 'json',
                        success: function (result) {
                            if (result && result.success) {
                                ids.forEach(function (id) {
                                    for (var i = 0; i < that.items.length; i++) {
                                        var item = that.items[i];
                                        if (id == item.ID) {
                                            item.Status = 0;
                                            break;
                                        }
                                    }
                                });
                            }
                        }
                    });
                    event.preventDefault();
                });
            }
        },
        DeleteSelectedJob: function (event) {
            var ids = this.getSelectedItemIds();
            if (ids.length == 0) {
                BootstrapDialog.alert('Nothing is checked!');
            }
            else {
                var that = this;
                BootstrapDialog.confirm('Are you sure to delete selected job?', function (result) {
                    if (result) {
                        var _ids = '';
                        ids.forEach(function (id) {
                            _ids += id + ',';
                        });
                        _ids = _ids.substr(0, _ids.length - 1);
                        that.$nextTick(() => {
                            $.ajax({
                                url: '/Job/BatchOperate/',
                                data: { "IDS": _ids, "Type": "Delete" },
                                type: 'POST',
                                dataType: 'json',
                                success: function (result) {
                                    if (result) {
                                        if (result.success === true) {
                                            that.searchJobs();
                                        }
                                        else {
                                            BootstrapDialog.show({
                                                title: 'Delete Job Error',
                                                message: result.message.toString()
                                            });
                                        }
                                    }
                                }
                            });
                            event.preventDefault();
                        });
                    }
                });
            }
        }
    },
    computed: {
        sortType: function () {
            return this.sortOrders[this.sortKey] > 0 ? 'asc' : 'desc';
        },
        beforeFirstValidation: function () {
            var result = true;
            var validatingProperties = ["JobName", "AssemblyName", "ClassName", "CronExpression"];
            for (var i = 0; i < validatingProperties.length; i++) {
                result = result && validatingProperties[i] === null;
            }
            return result;
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
        showAddOrEditModel: function (val, oldVal) {
            if (val === false) {
                this.resetAddOrEdit();
            }
            else {
                if (this.Model.ID != '') {
                    this.validate();
                }
            }
        },
        IsCheckAll: function (val, oldVal) {
            this.items.forEach(function (item) {
                item.Checked = val;
            });
        }
    }
});