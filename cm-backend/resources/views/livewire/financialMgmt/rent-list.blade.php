@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.financialMgmt.rentList')}}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.general.search')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>{{__('web_message.general.username')}}：</label>
                        <input type="text" class="form-control" wire:model="username">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.minerName')}}：</label>
                        <input type="text" class="form-control" wire:model="minerName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.agentName')}}：</label>
                        <input type="text" class="form-control" wire:model="agentName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="minerStatus">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['statusList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.isExperience')}}：</label>
                        <select class="form-select form-control" wire:model="isExperience">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('web_message.general.yes')}}</option>
                            <option value="0">{{__('web_message.general.no')}}</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.operator')}}：</label>
                        <input type="text" class="form-control" wire:model="operator">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.createdAt')}}：</label>
                        <div class="time-range">
                            <input type="text" id="createTimeRange" class="form-control" wire:model="createTimeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCreateTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCreateTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCreateTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCreateTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCreateTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCreateTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.startTime')}}：</label>
                        <div class="time-range">
                            <input type="text" id="startTimeRange" class="form-control" wire:model="startTimeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setStartTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded"
                               wire:click="setStartTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setStartTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setStartTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded"
                               wire:click="setStartTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded"
                               wire:click="setStartTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.endTime')}}：</label>
                        <div class="time-range">
                            <input type="text" id="endTimeRange" class="form-control" wire:model="endTimeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setEndTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setEndTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setEndTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setEndTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setEndTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setEndTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.checkTime')}}：</label>
                        <div class="time-range">
                            <input type="text" id="checkTimeRange" class="form-control" wire:model="checkTimeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCheckTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCheckTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCheckTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCheckTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCheckTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setCheckTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" wire:click="showAddExperience()">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th>{{__('web_message.general.orderNo')}}</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.minerName')}}</th>
                            <th>{{__('web_message.general.agentName')}}</th>
                            <th>{{__('web_message.general.rentPrice')}}</th>
                            <th>{{__('web_message.general.cycle')}}</th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.isExperience')}}</th>
                            <th>{{__('web_message.general.operator')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                            <th>{{__('web_message.general.checkTime')}}</th>
                            <th>{{__('web_message.general.startTime')}}</th>
                            <th>{{__('web_message.general.endTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>{{$data->mm_id}}</td>
                                <td>{{$data->username}}</td>
                                <td>{{$data->name}}</td>
                                <td>{{$data->parent_username??''}}</td>
                                <td>{{number_format($data->rent_price??0,session()->get('defaultScale'))}}</td>
                                <td>{{$data->cycle_days??0}}</td>
                                <td>{{$data->statusTxt}}</td>
                                <td>{{$data->isExperience}}</td>
                                <td>{{$data->operator}}</td>
                                <td>{{$data->created_at}}</td>
                                <td>{{$data->check_time}}</td>
                                <td>{{$data->start_time}}</td>
                                <td>{{$data->end_time}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="13">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="13" class="footable-visible">
                                <div class="text-right" style="position: relative;">
                                    <div class="total-entries">
                                        {{(session()->get('pageCacheData')['totalRow']??0)}} {{__('web_message.general.entries')}}
                                    </div>
                                    <x-pagination></x-pagination>
                                </div>
                            </td>
                        </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
        <!-- addExperience -->
        <div class="modal fade in" id="addExperience" role="dialog"
             style="display: {{'addExperience' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="addClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.financialMgmt.addExperience')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addExperience">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.minerName')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control" wire:model="addExperienceMinerId"
                                                wire:change="changeMiner">
                                            <option value="0">
                                                {{__('web_message.general.select')}}
                                            </option>
                                            @forelse ((session()->get('pageCacheData')['minerList']??[]) as $data)
                                                <option value="{{$data->miner_id}}">{{$data->miner_name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.rentPrice')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <input type="text" class="form-control" wire:model="popPurchaseRentPrice"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.purchaseRentCount')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <input type="text" class="form-control" wire:model="popPurchaseRentCount"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.purchaseRentCycle')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <input type="text" class="form-control" wire:model="popPurchaseRentCycle"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.experienceCommission')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <input type="text" class="form-control" wire:model="popExperienceCommission"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addExperienceUsername">
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.add')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('#createTimeRange').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('#createTimeRange').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('createTimeRange', {range: range});
                });
                $('#createTimeRange').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('createTimeRange', {range: range});
                });
                $('#startTimeRange').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('#startTimeRange').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('startTimeRange', {range: range});
                });
                $('#startTimeRange').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('startTimeRange', {range: range});
                });
                $('#endTimeRange').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('#endTimeRange').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('endTimeRange', {range: range});
                });
                $('#endTimeRange').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('endTimeRange', {range: range});
                });
                $('#addClose').click(function () {
                    $('#addExperience').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });

                $('#checkTimeRange').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('#checkTimeRange').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('checkTimeRange', {range: range});
                });
                $('#checkTimeRange').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('checkTimeRange', {range: range});
                });
            });
        </script>
    </div>
@endsection
