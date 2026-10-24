@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.report.intervalDailyReport')}}</h1>
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
                        <label>{{__('web_message.general.managerName')}}：</label>
                        <input type="text" class="form-control" wire:model="managerName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.timeRange')}}：</label>
                        <div class="time-range">
                            <input type="text" name="dates" class="form-control" wire:model="reportTimeRange"
                                   autocomplete="off"
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
                        <label>{{__('web_message.general.type')}}：</label>
                        <select class="form-select form-control" wire:model="reportType">
                            <option value="1">{{__('web_message.general.summary')}}</option>
                            <option value="2">{{__('web_message.general.detail')}}</option>

                        </select>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>

        <div class="panel">
            <div class="panel-body" style="padding-bottom: 0;">
                <div class="form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left"></div>
                        <div class="col-sm-6 table-toolbar-right">
                            <button class="btn btn-mint btn-export" wire:click="doExport">
                                <i class="bi-download"></i> {{__('web_message.general.export')}}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
            <!--區間統計-->
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.intervalDailyReport.statistic')}}</h3>
            </div>
            <div class="panel-body">
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            @forelse(($this->statisticTitle??[]) as $title)
                                <th>{{__('web_message.intervalDailyReport.'.$title)}}</th>
                            @empty
                            @endforelse
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']['statistic']??[]) as $data)
                            <tr>
                                <td>{{__('web_message.intervalDailyReport.'.$data->type)}}</td>
                                <td>{{number_format($data->newRegister,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->todayLogin,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->activeMember,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->totalDeposit,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->totalWithdrawal,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->netDeposit,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->totalConsume,session()->get('defaultScale'))}}</td>
                                @if(2 == ($reportType ?? 1))
                                    <td>{{number_format($data->totalPurchase,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->totalRent,session()->get('defaultScale'))}}</td>
                                @endif
                                <td>{{number_format($data->totalIncome,session()->get('defaultScale'))}}</td>
                                @if(2 == ($reportType ?? 1))
                                    <td>{{number_format($data->minerIncome,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->rentIncome,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->inviteIncome,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->rebateIncome,session()->get('defaultScale'))}}</td>
                                @endif
                                <td>{{number_format($data->totalAward,session()->get('defaultScale'))}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="{{count($this->statisticTitle)}}">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                    </table>
                </div>
            </div>
            <!--日報表-->
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.intervalDailyReport.daily')}}</h3>
            </div>
            <div class="panel-body">
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            @forelse(($this->dailyTitle??[]) as $title)
                                <th>{{__('web_message.intervalDailyReport.'.$title)}}</th>
                            @empty
                            @endforelse
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']['data']??[]) as $data)
                            <tr>
                                <td>{{$data->date}}</td>
                                <td>{{number_format($data->newRegister)}}</td>
                                <td>{{number_format($data->todayLogin)}}</td>
                                <td>{{number_format($data->activeMember)}}</td>
                                <td>{{number_format($data->totalDeposit,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->totalWithdrawal,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->netDeposit,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->totalConsume,session()->get('defaultScale'))}}</td>
                                @if(2 == ($reportType ?? 1))
                                    <td>{{number_format($data->totalPurchase,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->totalRent,session()->get('defaultScale'))}}</td>
                                @endif
                                <td>{{number_format($data->totalIncome,session()->get('defaultScale'))}}</td>
                                @if(2 == ($reportType ?? 1))
                                    <td>{{number_format($data->minerIncome,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->rentIncome,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->inviteIncome,session()->get('defaultScale'))}}</td>
                                    <td>{{number_format($data->rebateIncome,session()->get('defaultScale'))}}</td>
                                @endif
                                <td>{{number_format($data->totalAward,session()->get('defaultScale'))}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="{{count($this->dailyTitle)}}">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('input[name="dates"]').daterangepicker({
                    timePicker: false,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="dates"]').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD') + ' ~ ' + picker.endDate.format('YYYY-MM-DD');
                    $(this).val(range);
                    Livewire.dispatch('reportTimeRange', {range: range});
                });
                $('input[name="dates"]').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('reportTimeRange', {range: range});
                });
            });
        </script>
    </div>
@endsection
