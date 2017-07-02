Vue.component('vb-pager', {
    template: `<div class="pager">
                        <span class="form-inline">
                            <select class="form-control" v-on:change="changePageSize" v-model="vPageSize" number>
                                <option v-if="isArryEmpty(pageSizeList)" :value="vPageSize" selected>{{vPageSize}}</option>
                                <option v-for="row in pageSizeList" :value="row" :selected="row | getSelectedState(vPageSize)">{{row}}</option>
                            </select>
                        </span>
                        <span class="btn btn-default" :class="{'disabled' : convertedPageCurrent <= 1}" v-on:click="showFirst">
                            First
                        </span>
                        <span class="btn btn-default" :class="{'disabled' : convertedPageCurrent <= 1}" v-on:click="showPrevious">
                            Previous
                        </span>
                        <span class="form-inline">
                            <span>Page</span>
                            <input class="pageIndex form-control" value="1" style="width:60px;text-align:center" type="text" v-model="vPageCurrent" v-on:input="validateNumber" v-on:keyup.enter="showPage" number/>
                            <span>of</span> {{pageCount}}
                        </span>
                        <span class="btn btn-default" :class="{'disabled' : convertedPageCurrent >= pageCount}" v-on:click="showNext">
                            Next
                        </span>
                        <span class="btn btn-default" :class="{'disabled' : convertedPageCurrent >= pageCount}" v-on:click="showLast">
                            Last
                        </span>
                        <span>View {{startIndex}} - {{endIndex}} of {{totalCount}}</span>
                    </div>
               </div>`,
    props: {
        pageCurrent: {
            type: Number,
            default: 1
        },
        pageSize: {
            type: Number,
            required: true
        },
        pageSizeList: {
            type: Array
        },
        totalCount: {
            type: Number,
            required: true
        }
    },
    data: function () {
        return {
            vPageCurrent: this.pageCurrent,
            vPageSize: this.pageSize,
            pageCount: 0
        }
    },
    mounted: function () {

    },
    filters: {
        getSelectedState: function (value, current) {
            return current == value ? 'selected' : '';
        }
    },
    computed: {
        convertedPageCurrent: function () {
            return this.vPageCurrent == undefined ? (this.vPageCurrent = 1) : this.vPageCurrent;
        },
        disabledStyle: function () {
            return this.vPageCurrent <= 1 ? 'disabled' : '';
        },
        startIndex: function () {
            return (this.convertedPageCurrent - 1) * this.vPageSize + 1;
        },
        endIndex: function () {
            return Math.min(this.convertedPageCurrent * this.vPageSize, this.totalCount);
        }
    },
    methods: {
        validateNumber: function () {
            var ev = this.vPageCurrent;
            if (ev == '' || isNaN(ev) || ev < 1 || ev > this.pageCount) {
                this.vPageCurrent = 1;
            }
        },
        isArryEmpty: function (value) {
            return value == undefined || value.length == 0;
        },
        pageChanged: function () {
            this.$emit('page-changed', { pageCurrent: this.vPageCurrent, pageSize: this.vPageSize });
        },
        pageSizeChanged: function () {
            this.$emit('page-size-changed', { pageCurrent: this.vPageCurrent, pageSize: this.vPageSize });
        },
        changePageSize: function (event) {
            this.vPageCurrent = 1;
            this.pageSizeChanged();
        },
        calculatePageCount: function () {
            this.pageCount = Math.ceil(this.totalCount / this.vPageSize);
        },
        showFirst: function () {
            if (this.vPageCurrent > 1) {
                this.vPageCurrent = 1;
                this.pageChanged();
            }
        },
        showPrevious: function () {
            if (this.vPageCurrent > 1) {
                this.vPageCurrent--;
                this.pageChanged();
            }
        },
        showNext: function () {
            if (this.vPageCurrent < this.pageCount) {
                this.vPageCurrent++;
                this.pageChanged();
            }
        },
        showLast: function () {
            if (this.vPageCurrent < this.pageCount) {
                this.vPageCurrent = this.pageCount;
                this.pageChanged();
            }
        },
        showPage: function () {
            var _pageCurrent = parseInt(this.vPageCurrent);
            if (_pageCurrent >= 1 && _pageCurrent <= this.pageCount) {
                this.vPageCurrent = _pageCurrent;
                this.pageChanged();
            }
        }
    },
    watch: {
        pageCurrent: function (value) {
            this.vPageCurrent = value;
        },
        pageSize: function (value) {
            this.vPageSize = value;
        },
        vPageSize: function (value) {
            this.calculatePageCount();
        },
        totalCount: function (value) {
            this.calculatePageCount();
        }
    }
});