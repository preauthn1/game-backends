@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.financialMgmt.rebateInfo')}}</h1>
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
                        <label>{{__('web_message.general.childAccount')}}：</label>
                        <input type="text" class="form-control" wire:model="account">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.rebate')}}{{__('web_message.general.time')}}：</label>
                        <div class="time-range">
                            <input type="text" name="datetimes" class="form-control" wire:model="timeRange" autocomplete="off"
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
                            <th>{{__('web_message.general.childAccount')}}</th>
                            <th>{{__('web_message.general.totalAmount')}}</th>
                            <th>{{__('web_message.general.rate')}}</th>
                            <th>{{__('web_message.general.rebateAmount')}}</th>
                            <th>{{__('web_message.general.rebate')}}{{__('web_message.general.time')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>{{$data->id}}</td>
                                <td>{{$data->username}}</td>
                                <td>{{$data->name}}</td>
                                <td>{{$data->accounts}}</td>
                                <td>{{number_format($data->total_amount,session()->get('defaultScale'))}}</td>
                                <td>{{bcmul($data->percentage,100)}}%</td>
                                <td>{{number_format($data->amount,session()->get('defaultScale'))}}</td>
                                <td>{{$data->summary_time}}</td>
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
                $('input[name="datetimes"]').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="datetimes"]').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
                $('input[name="datetimes"]').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
            });
        </script>
    </div>
@endsection
