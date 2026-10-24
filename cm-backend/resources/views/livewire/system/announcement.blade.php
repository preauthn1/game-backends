@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.system.announcement')}}</h1>
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
                        <label>{{__('web_message.general.type')}}：</label>
                        <select class="form-select form-control" wire:model="announcementType">
                            <option value="0"
                                    selected>{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.announceTime')}}：</label>
                        <div class="time-range">
                            <input type="text" name="datetimes" class="form-control" wire:model="announcementRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="announcementStatus">
                            <option value="-1" selected>{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('web_message.general.statusList.enable')}}</option>
                            <option value="0">{{__('web_message.general.statusList.disable')}}</option>

                        </select>
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
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addAnnouncement">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.type')}}</th>
                            <th>{{__('web_message.general.title')}}</th>
                            <th>{{__('web_message.general.content')}}</th>
                            <th>{{__('web_message.example.invalid')}}</th>
                            <th>{{__('web_message.general.startTime')}}</th>
                            <th>{{__('web_message.general.endTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>
                                    <button class="btn btn-warning" wire:click="showModify({{$data->announcement_id}})"
                                            id="showModify">
                                        <i class="bi-pencil-square"></i>
                                    </button>
                                </td>
                                <td style="font-size: 16px;"><i
                                        class="{{(1==$data->status)?'bi-toggle-on':'bi-toggle-off'}}"></i></td>
                                <td>{{$data->typeTxt}}</td>
                                <td>{{$data->title}}</td>
                                <td>
                                    <div>{{$data->contentTxt}}</div>
                                </td>
                                <td><i class="{{(1==$data->is_top)?'bi-check-lg':'bi-x-lg'}}"></i></td>
                                <td>{{$data->start_time}}</td>
                                <td>{{$data->end_time}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="8">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="8" class="footable-visible">
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
        <!-- addAnnouncement -->
        <div class="modal fade" id="addAnnouncement" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.system.addAnnouncement')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addAnnouncement">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.type')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="addAnnouncementType">
                                            <option value="0"
                                                    selected>{{__('web_message.general.select')}} {{__('web_message.general.type')}}</option>
                                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.title')}}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="addAnnouncementTitle">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.content')}}
                                    </label>
                                    <div class="col-sm-9">
                                        <textarea rows="13" class="form-control"
                                                  wire:model="addAnnouncementContent"></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.example.invalid')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="add-announcement-top" class="toggle-switch" type="checkbox"
                                               wire:model="addAnnouncementTop">
                                        <label for="add-announcement-top"></label>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.startTime')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                               id="addAnnouncementStartTime">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.endTime')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                               id="addAnnouncementEndTime">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.status')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="add-announcement-status" class="toggle-switch" type="checkbox"
                                               wire:model="addAnnouncementStatus">
                                        <label for="add-announcement-status"></label>
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
        <!-- modifyAnnouncement -->
        <div class="modal fade in" id="modifyAnnouncement" role="dialog"
             style="display: {{'modifyAnnouncement' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifyClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.system.modifyAnnouncement')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModify">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyAnnouncementId">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.type')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="modifyAnnouncementType">
                                            <option value="0"
                                                    selected>{{__('web_message.general.select')}}{{__('web_message.general.type')}}</option>
                                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.title')}}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="modifyAnnouncementTitle">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.content')}}
                                    </label>
                                    <div class="col-sm-9">
                                        <textarea rows="13" class="form-control"
                                                  wire:model="modifyAnnouncementContent"></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.example.invalid')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="modify-announcement-top" class="toggle-switch" type="checkbox"
                                               wire:model="modifyAnnouncementTop">
                                        <label for="modify-announcement-top"></label>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.startTime')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                               id="modifyAnnouncementStartTime" wire:model="announcementStartTime">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.endTime')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                               id="modifyAnnouncementEndTime"
                                               wire:model="announcementEndTime">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.status')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="modify-announcement-status" class="toggle-switch" type="checkbox"
                                               wire:model="modifyAnnouncementStatus">
                                        <label for="modify-announcement-status"></label>
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.modify')}}</button>
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
                $('input[name="datetimes"]').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="datetimes"]').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('announcementRange', {range: range});
                });
                $('input[name="datetimes"]').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('announcementRange', {range: range});
                });
                $('#modifyClose').click(function () {
                    $('#modifyAnnouncement').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
                $('input[name="dates"]').daterangepicker({
                    singleDatePicker: true,
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                    locale: {
                        format: 'YYYY-MM-DD HH:mm'
                    },
                    drops: 'up',
                });
                $('#addAnnouncementStartTime').on('apply.daterangepicker', function (ev, picker) {
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('announcementStartTime', {date: date});
                });
                $('#addAnnouncementStartTime').on('cancel.daterangepicker', function (ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('announcementStartTime', {date: date});
                });
                $('#addAnnouncementStartTime').blur(function () {
                    Livewire.dispatch('announcementStartTime', {date: $(this).val()});
                });
                $('#addAnnouncementEndTime').on('apply.daterangepicker', function (ev, picker) {
                    console.log(ev.id);
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('announcementEndTime', {date: date});
                });
                $('#addAnnouncementEndTime').on('cancel.daterangepicker', function (ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('announcementEndTime', {date: date});
                });
                $('#addAnnouncementEndTime').blur(function () {
                    Livewire.dispatch('announcementEndTime', {date: $(this).val()});
                });
                $('#modifyAnnouncementStartTime').on('apply.daterangepicker', function (ev, picker) {
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('announcementStartTime', {date: date});
                });
                $('#modifyAnnouncementStartTime').on('cancel.daterangepicker', function (ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('announcementStartTime', {date: date});
                });
                $('#modifyAnnouncementStartTime').blur(function () {
                    Livewire.dispatch('announcementStartTime', {date: $(this).val()});
                });
                $('#modifyAnnouncementEndTime').on('apply.daterangepicker', function (ev, picker) {
                    console.log(ev.id);
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('announcementEndTime', {date: date});
                });
                $('#modifyAnnouncementEndTime').on('cancel.daterangepicker', function (ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('announcementEndTime', {date: date});
                });
                $('#modifyAnnouncementEndTime').blur(function () {
                    Livewire.dispatch('announcementEndTime', {date: $(this).val()});
                });
            });
        </script>
    </div>
@endsection
