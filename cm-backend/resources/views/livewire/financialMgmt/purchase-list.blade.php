@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.financialMgmt.purchaseList')}}</h1>
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
                        <label>{{__('web_message.general.createdAt')}}：</label>
                        <div class="time-range">
                            <input type="text" id="createTimeRange" class="form-control" wire:model="timeRange" autocomplete="off"
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
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th>{{__('web_message.general.orderNo')}}</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.minerName')}}</th>
                            <th>{{__('web_message.general.purchasePrice')}}</th>
                            <th>{{__('web_message.general.agentName')}}</th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                            <th>{{__('web_message.general.checkTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>{{$data->mm_id}}</td>
                                <td>{{$data->username}}</td>
                                <td>{{$data->name}}</td>
                                <td>{{number_format($data->purchase_price??0,session()->get('defaultScale'))}}</td>
                                <td>{{$data->parent_username??''}}</td>
                                <td>{{$data->statusTxt}}</td>
                                <td>{{$data->created_at}}</td>
                                <td>{{$data->check_time}}</td>
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
